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
    public class SqlSolicitudRepository : ISolicitudRepository
    {
        private readonly string connectionString;

        public SqlSolicitudRepository()
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

        public async Task<IList<SolicitudFlag>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var solicitudes = new List<SolicitudFlag>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_ListarSolicitudesXFiltro", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                command.Parameters.Add(
                    "@Pusuario_c", SqlDbType.VarChar, 100).Value = usuario;
                command.Parameters.Add(
                    "@Pflag_c", SqlDbType.VarChar, 5).Value = flag;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        solicitudes.Add(new SolicitudFlag
                        {
                            CodigoSolicitud = LeerTexto(
                                reader, "CodigoSolicitud"),
                            TipoRegistro = LeerTexto(reader, "TipoRegistro"),
                            Categoria = LeerTexto(reader, "Categoria"),
                            CreadoPor = LeerTexto(reader, "CreadoPor"),
                            SolicitadoPara = LeerTexto(
                                reader, "SolicitadoPara"),
                            AsignadoA = LeerTexto(reader, "AsignadoA"),
                            FechaSolicitud = Convert.ToDateTime(
                                reader["FechaSolicitud"]),
                            FechaEstimada = LeerFechaNullable(
                                reader, "FechaEstimada"),
                            CopiaJefatura = LeerTexto(
                                reader, "CopiaJefatura"),
                            Estado = LeerTexto(reader, "Estado")
                        });
                    }
                }
            }

            return solicitudes;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }

        private static DateTime? LeerFechaNullable(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(valor);
        }
    }
}
