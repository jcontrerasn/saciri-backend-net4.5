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
    public class SqlCentroCostoRepository : ICentroCostoRepository
    {
        private readonly string connectionString;

        public SqlCentroCostoRepository()
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

        public async Task<IList<CentroCosto>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var resultados = new List<CentroCosto>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_sm_ListarCentroCostoXFiltro", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "Pfiltro_c", SqlDbType.VarChar, 50).Value = filtro;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        resultados.Add(new CentroCosto
                        {
                            CentroCostoCodigo = LeerTexto(
                                reader, "CentroCosto"),
                            Area = LeerTexto(reader, "Area"),
                            NombreCentroCosto = LeerTexto(
                                reader, "NombreCentroCosto"),
                            Activo = LeerBooleano(reader, "Activo")
                        });
                    }
                }
            }

            return resultados;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? string.Empty : Convert.ToString(valor);
        }

        private static bool LeerBooleano(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];

            if (valor == DBNull.Value)
            {
                return false;
            }

            if (valor is bool)
            {
                return !(bool)valor;
            }

            if (valor is byte
                || valor is short
                || valor is int
                || valor is long
                || valor is decimal)
            {
                return Convert.ToDecimal(valor) == 0;
            }

            var texto = Convert.ToString(valor);

            if (string.Equals(texto, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(texto, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(texto, "n", StringComparison.OrdinalIgnoreCase)
                || string.Equals(texto, "no", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
