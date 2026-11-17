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
    public class SqlEstadoSolicitudRepository : IEstadoSolicitudRepository
    {
        private readonly string connectionString;

        public SqlEstadoSolicitudRepository()
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

        public async Task<IList<EstadoSolicitud>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var estados = new List<EstadoSolicitud>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdEstadoSolicitud, Nombre, Activo, FechaRegistro "
                + "FROM dbo.CIRI_EstadosSolicitud "
                + "ORDER BY IdEstadoSolicitud", connection))
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
                        estados.Add(new EstadoSolicitud
                        {
                            IdEstadoSolicitud = Convert.ToInt32(
                                reader["IdEstadoSolicitud"]),
                            Nombre = LeerTexto(reader, "Nombre"),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return estados;
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
