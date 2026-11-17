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
    public class SqlSubcategoriaRepository : ISubcategoriaRepository
    {
        private readonly string connectionString;

        public SqlSubcategoriaRepository()
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

        public async Task<Subcategoria> CrearAsync(
            int idCategoria,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_Subcategorias "
                    + "(IdCategoria, Nombre, Descripcion, Activo, FechaRegistro) "
                    + "VALUES (@IdCategoria, @Nombre, @Descripcion, 1, GETDATE()); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdCategoria", SqlDbType.Int).Value = idCategoria;
                    command.Parameters.Add(
                        "@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                    command.Parameters.Add(
                        "@Descripcion", SqlDbType.NVarChar, 250).Value =
                        (object)descripcion ?? DBNull.Value;

                    var idSubcategoria = Convert.ToInt32(
                        await command.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection,
                        idSubcategoria,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<Subcategoria>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var subcategorias = new List<Subcategoria>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdSubcategoria, IdCategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Subcategorias "
                + "ORDER BY IdSubcategoria",
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
                        subcategorias.Add(LeerSubcategoria(reader));
                    }
                }
            }

            return subcategorias;
        }

        public async Task<IList<Subcategoria>> ObtenerPorCategoriaAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var subcategorias = new List<Subcategoria>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdSubcategoria, IdCategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Subcategorias "
                + "WHERE IdCategoria = @IdCategoria "
                + "ORDER BY IdSubcategoria",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdCategoria", SqlDbType.Int).Value = idCategoria;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        subcategorias.Add(LeerSubcategoria(reader));
                    }
                }
            }

            return subcategorias;
        }

        public async Task<Subcategoria> ActualizarAsync(
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
                    "UPDATE dbo.CIRI_Subcategorias "
                    + "SET Nombre = @Nombre, Descripcion = @Descripcion "
                    + "WHERE IdSubcategoria = @IdSubcategoria",
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
                        "@IdSubcategoria", SqlDbType.Int).Value =
                        idSubcategoria;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idSubcategoria,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<Subcategoria> CambiarEstadoAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_Subcategorias "
                    + "SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END "
                    + "WHERE IdSubcategoria = @IdSubcategoria",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdSubcategoria", SqlDbType.Int).Value =
                        idSubcategoria;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idSubcategoria,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<Subcategoria> ObtenerPorIdAsync(
            SqlConnection connection,
            int idSubcategoria,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT IdSubcategoria, IdCategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Subcategorias "
                + "WHERE IdSubcategoria = @IdSubcategoria",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSubcategoria", SqlDbType.Int).Value = idSubcategoria;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerSubcategoria(reader)
                        : null;
                }
            }
        }

        private static Subcategoria LeerSubcategoria(
            SqlDataReader reader)
        {
            return new Subcategoria
            {
                IdSubcategoria = Convert.ToInt32(
                    reader["IdSubcategoria"]),
                IdCategoria = Convert.ToInt32(reader["IdCategoria"]),
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
