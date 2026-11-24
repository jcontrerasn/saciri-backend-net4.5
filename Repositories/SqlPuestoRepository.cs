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
    public class SqlPuestoRepository : IPuestoRepository
    {
        private readonly string connectionString;

        public SqlPuestoRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings[
                "SgtaConnection"];

            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                throw new InvalidOperationException(
                    "No se configurÃ³ la conexiÃ³n SgtaConnection.");
            }

            connectionString = setting.ConnectionString;
        }

        public async Task<Puesto> CrearAsync(
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_Puestos "
                    + "(Nombre, Descripcion, Activo, FechaRegistro) "
                    + "VALUES (@Nombre, @Descripcion, 1, SYSDATETIME()); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarTexto(command, "@Nombre", 100, nombre);
                    AgregarTexto(command, "@Descripcion", 250, descripcion);

                    var idPuesto = Convert.ToInt32(
                        await command.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection,
                        idPuesto,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<Puesto>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var puestos = new List<Puesto>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdPuesto, Nombre, Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Puestos "
                + "ORDER BY IdPuesto", connection))
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
                        puestos.Add(LeerPuesto(reader));
                    }
                }
            }

            return puestos;
        }

        public async Task<Puesto> ActualizarAsync(
            int idPuesto,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_Puestos "
                    + "SET Nombre = @Nombre, Descripcion = @Descripcion "
                    + "WHERE IdPuesto = @IdPuesto",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarTexto(command, "@Nombre", 100, nombre);
                    AgregarTexto(command, "@Descripcion", 250, descripcion);
                    command.Parameters.Add(
                        "@IdPuesto", SqlDbType.Int).Value = idPuesto;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idPuesto,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<Puesto> CambiarEstadoAsync(
            int idPuesto,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_Puestos "
                    + "SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END "
                    + "WHERE IdPuesto = @IdPuesto",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdPuesto", SqlDbType.Int).Value = idPuesto;

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idPuesto,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<Puesto> ObtenerPorIdAsync(
            SqlConnection connection,
            int idPuesto,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT IdPuesto, Nombre, Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Puestos "
                + "WHERE IdPuesto = @IdPuesto",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdPuesto", SqlDbType.Int).Value = idPuesto;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerPuesto(reader)
                        : null;
                }
            }
        }

        private static Puesto LeerPuesto(SqlDataReader reader)
        {
            return new Puesto
            {
                IdPuesto = Convert.ToInt32(reader["IdPuesto"]),
                Nombre = LeerTexto(reader, "Nombre"),
                Descripcion = LeerTexto(reader, "Descripcion"),
                Activo = Convert.ToBoolean(reader["Activo"]),
                FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
            };
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }

        private static void AgregarTexto(
            SqlCommand command,
            string nombre,
            int longitud,
            string valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.NVarChar, longitud)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }
    }
}
