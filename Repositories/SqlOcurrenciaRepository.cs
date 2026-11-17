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
    public class SqlOcurrenciaRepository : IOcurrenciaRepository
    {
        private readonly string connectionString;

        public SqlOcurrenciaRepository()
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

        public async Task<IList<Ocurrencia>> ObtenerPorSubcategoriaAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var ocurrencias = new List<Ocurrencia>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdOcurrencia, IdSubcategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Ocurrencias "
                + "WHERE IdSubcategoria = @IdSubcategoria "
                + "ORDER BY IdOcurrencia",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSubcategoria", SqlDbType.Int).Value = idSubcategoria;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        ocurrencias.Add(new Ocurrencia
                        {
                            IdOcurrencia = Convert.ToInt32(
                                reader["IdOcurrencia"]),
                            IdSubcategoria = Convert.ToInt32(
                                reader["IdSubcategoria"]),
                            Nombre = LeerTexto(reader, "Nombre"),
                            Descripcion = LeerTexto(
                                reader, "Descripcion"),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return ocurrencias;
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
