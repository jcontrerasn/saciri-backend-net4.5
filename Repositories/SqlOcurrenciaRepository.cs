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

        public async Task<Ocurrencia> CrearAsync(
            int idSubcategoria,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_Ocurrencias "
                    + "(IdSubcategoria, Nombre, Descripcion, Activo, FechaRegistro) "
                    + "VALUES (@IdSubcategoria, @Nombre, @Descripcion, 1, GETDATE()); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdSubcategoria", SqlDbType.Int).Value = idSubcategoria;
                    command.Parameters.Add(
                        "@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                    command.Parameters.Add(
                        "@Descripcion", SqlDbType.NVarChar, 250).Value =
                        (object)descripcion ?? DBNull.Value;

                    var idOcurrencia = Convert.ToInt32(
                        await command.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection,
                        idOcurrencia,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<Ocurrencia>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var ocurrencias = new List<Ocurrencia>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdOcurrencia, IdSubcategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Ocurrencias "
                + "ORDER BY IdOcurrencia",
                connection))
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
                        ocurrencias.Add(LeerOcurrencia(reader));
                    }
                }
            }

            return ocurrencias;
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
                        ocurrencias.Add(LeerOcurrencia(reader));
                    }
                }
            }

            return ocurrencias;
        }

        public async Task<Ocurrencia> ActualizarAsync(
            int idOcurrencia,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_Ocurrencias "
                    + "SET Nombre = @Nombre, Descripcion = @Descripcion "
                    + "WHERE IdOcurrencia = @IdOcurrencia",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                    command.Parameters.Add(
                        "@Descripcion", SqlDbType.NVarChar, 250).Value =
                        (object)descripcion ?? DBNull.Value;
                    command.Parameters.Add(
                        "@IdOcurrencia", SqlDbType.Int).Value = idOcurrencia;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idOcurrencia,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<Ocurrencia> CambiarEstadoAsync(
            int idOcurrencia,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_Ocurrencias "
                    + "SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END "
                    + "WHERE IdOcurrencia = @IdOcurrencia",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdOcurrencia", SqlDbType.Int).Value = idOcurrencia;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idOcurrencia,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<Ocurrencia> ObtenerPorIdAsync(
            SqlConnection connection,
            int idOcurrencia,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT IdOcurrencia, IdSubcategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Ocurrencias "
                + "WHERE IdOcurrencia = @IdOcurrencia",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdOcurrencia", SqlDbType.Int).Value = idOcurrencia;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerOcurrencia(reader)
                        : null;
                }
            }
        }

        private static Ocurrencia LeerOcurrencia(
            SqlDataReader reader)
        {
            return new Ocurrencia
            {
                IdOcurrencia = Convert.ToInt32(
                    reader["IdOcurrencia"]),
                IdSubcategoria = Convert.ToInt32(
                    reader["IdSubcategoria"]),
                Nombre = LeerTexto(reader, "Nombre"),
                Descripcion = LeerTexto(reader, "Descripcion"),
                Activo = Convert.ToBoolean(reader["Activo"]),
                FechaRegistro = Convert.ToDateTime(
                    reader["FechaRegistro"])
            };
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
