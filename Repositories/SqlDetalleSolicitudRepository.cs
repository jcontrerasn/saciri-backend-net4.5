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
    public class SqlDetalleSolicitudRepository : IDetalleSolicitudRepository
    {
        private readonly string connectionString;

        public SqlDetalleSolicitudRepository()
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

        public async Task<IList<DetalleSolicitud>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var detalles = new List<DetalleSolicitud>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdDetalleSolicitud, IdSolicitud, CentroCosto, Lugar "
                + "FROM dbo.CIRI_DetalleSolicitudes "
                + "ORDER BY IdDetalleSolicitud", connection))
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
                        detalles.Add(new DetalleSolicitud
                        {
                            IdDetalleSolicitud = Convert.ToInt32(
                                reader["IdDetalleSolicitud"]),
                            IdSolicitud = Convert.ToInt32(
                                reader["IdSolicitud"]),
                            CentroCosto = LeerTexto(reader, "CentroCosto"),
                            Lugar = LeerTexto(reader, "Lugar")
                        });
                    }
                }
            }

            return detalles;
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
