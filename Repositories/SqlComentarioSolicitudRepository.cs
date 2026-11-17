using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Repositories
{
    public class SqlComentarioSolicitudRepository : IComentarioSolicitudRepository
    {
        private readonly string connectionString;

        public SqlComentarioSolicitudRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings[
                "SgtaConnection"];

            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                throw new InvalidOperationException(
                    "No se configuró la conexión SgtaConnection.");
            }

            connectionString = setting.ConnectionString;
        }

        public async Task<IList<ComentarioSolicitud>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var comentarios = new List<ComentarioSolicitud>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdComentario, IdSolicitud, Comentario, Tipo, "
                + "Usuario, FechaRegistro "
                + "FROM dbo.CIRI_ComentariosSolicitudes "
                + "ORDER BY FechaRegistro, IdComentario", connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        comentarios.Add(new ComentarioSolicitud
                        {
                            IdComentario = Convert.ToInt32(
                                reader["IdComentario"]),
                            IdSolicitud = Convert.ToInt32(
                                reader["IdSolicitud"]),
                            Comentario = LeerTexto(reader, "Comentario"),
                            Tipo = LeerTexto(reader, "Tipo"),
                            Usuario = LeerTexto(reader, "Usuario"),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return comentarios;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }
    }
}
