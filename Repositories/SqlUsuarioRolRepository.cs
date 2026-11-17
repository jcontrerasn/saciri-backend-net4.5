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
    public class SqlUsuarioRolRepository : IUsuarioRolRepository
    {
        private readonly string connectionString;

        private const string SelectSql =
            "SELECT usuarioRol.IdUsuarioRol, "
            + "usuarioRol.Usuario, usuarioRol.IdRol, "
            + "rol.Nombre AS Rol, usuarioRol.Jefe, usuarioRol.Activo, "
            + "usuarioRol.FechaRegistro "
            + "FROM dbo.CIRI_UsuariosRoles AS usuarioRol "
            + "INNER JOIN dbo.CIRI_Roles AS rol "
            + "ON rol.IdRol = usuarioRol.IdRol ";

        public SqlUsuarioRolRepository()
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

        public async Task<bool> ExistePorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT COUNT(1) FROM dbo.CIRI_UsuariosRoles "
                + "WHERE Usuario = @Usuario", connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                var result = await command.ExecuteScalarAsync(
                    cancellationToken).ConfigureAwait(false);

                return Convert.ToInt32(result) > 0;
            }
        }

        public async Task<UsuarioRol> CrearAsync(
            string usuario,
            int idRol,
            string jefe,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_UsuariosRoles "
                    + "(Usuario, IdRol, Jefe, Activo, FechaRegistro) "
                    + "VALUES (@Usuario, @IdRol, @Jefe, 1, GETDATE()); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarTexto(command, "@Usuario", 100, usuario);
                    AgregarEntero(command, "@IdRol", idRol);
                    AgregarTexto(command, "@Jefe", 100, jefe);

                    var idUsuarioRol = Convert.ToInt32(
                        await command.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection,
                        idUsuarioRol,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<UsuarioRol>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var usuariosRoles = new List<UsuarioRol>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                SelectSql + "ORDER BY usuarioRol.IdUsuarioRol", connection))
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
                        usuariosRoles.Add(LeerUsuarioRol(reader));
                    }
                }
            }

            return usuariosRoles;
        }

        public async Task<UsuarioRol> ActualizarAsync(
            int idUsuarioRol,
            int idRol,
            string jefe,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_UsuariosRoles "
                    + "SET IdRol = @IdRol, Jefe = @Jefe "
                    + "WHERE IdUsuarioRol = @IdUsuarioRol", connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarEntero(command, "@IdRol", idRol);
                    AgregarTexto(command, "@Jefe", 100, jefe);
                    AgregarEntero(command, "@IdUsuarioRol", idUsuarioRol);

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idUsuarioRol,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<UsuarioRol> CambiarEstadoAsync(
            int idUsuarioRol,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_UsuariosRoles "
                    + "SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END "
                    + "WHERE IdUsuarioRol = @IdUsuarioRol", connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarEntero(command, "@IdUsuarioRol", idUsuarioRol);

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }
                }

                return await ObtenerPorIdAsync(
                    connection,
                    idUsuarioRol,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<UsuarioRol> ObtenerPorIdAsync(
            SqlConnection connection,
            int idUsuarioRol,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                SelectSql + "WHERE usuarioRol.IdUsuarioRol = @IdUsuarioRol",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                AgregarEntero(command, "@IdUsuarioRol", idUsuarioRol);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerUsuarioRol(reader)
                        : null;
                }
            }
        }

        private static UsuarioRol LeerUsuarioRol(SqlDataReader reader)
        {
            return new UsuarioRol
            {
                IdUsuarioRol = Convert.ToInt32(reader["IdUsuarioRol"]),
                Usuario = LeerTexto(reader, "Usuario"),
                IdRol = Convert.ToInt32(reader["IdRol"]),
                Rol = new Rol
                {
                    IdRol = Convert.ToInt32(reader["IdRol"]),
                    Nombre = LeerTexto(reader, "Rol")
                },
                Jefe = LeerTexto(reader, "Jefe"),
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
                nombre, SqlDbType.VarChar, longitud)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }

        private static void AgregarEntero(
            SqlCommand command,
            string nombre,
            int valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.Int)
            {
                Value = valor
            });
        }
    }
}
