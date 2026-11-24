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
            + "rol.Nombre AS Rol, usuarioRol.Jefe, usuarioRol.Puestos, "
            + "conexionUsuario.Estado AS EstadoConexion, "
            + "usuarioRol.Activo, "
            + "usuarioRol.FechaRegistro "
            + "FROM dbo.CIRI_UsuariosRoles AS usuarioRol "
            + "INNER JOIN dbo.CIRI_Roles AS rol "
            + "ON rol.IdRol = usuarioRol.IdRol "
            + "OUTER APPLY ("
            + "SELECT TOP (1) conexion.Estado "
            + "FROM dbo.CIRI_ConexionesUsuarios AS conexion "
            + "WHERE conexion.Usuario = usuarioRol.Usuario "
            + "ORDER BY conexion.IdConexion DESC"
            + ") AS conexionUsuario ";

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
            string puestos,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var transaction = connection.BeginTransaction())
                {
                    var idUsuarioRol = 0;

                    try
                    {
                        using (var command = new SqlCommand(
                            "SET XACT_ABORT ON; "
                            + "DECLARE @IdUsuarioRol INT; "
                            + "INSERT INTO dbo.CIRI_UsuariosRoles "
                            + "(Usuario, IdRol, Jefe, Puestos, Activo, FechaRegistro) "
                            + "VALUES (@Usuario, @IdRol, @Jefe, @Puestos, 1, GETDATE()); "
                            + "SET @IdUsuarioRol = CAST(SCOPE_IDENTITY() AS INT); "
                            + "IF NOT EXISTS ("
                            + "SELECT 1 FROM dbo.CIRI_ConexionesUsuarios "
                            + "WITH (UPDLOCK, HOLDLOCK) "
                            + "WHERE Usuario = @Usuario) "
                            + "AND NOT EXISTS ("
                            + "SELECT 1 FROM dbo.CIRI_HistorialConexiones "
                            + "WITH (UPDLOCK, HOLDLOCK) "
                            + "WHERE Usuario = @Usuario) "
                            + "BEGIN "
                            + "INSERT INTO dbo.CIRI_ConexionesUsuarios "
                            + "(Usuario, Fecha, Estado) "
                            + "VALUES (@Usuario, SYSDATETIME(), 0); "
                            + "INSERT INTO dbo.CIRI_HistorialConexiones "
                            + "(Usuario, Estado, Sustento) "
                            + "VALUES (@Usuario, 0, N'Registro'); "
                            + "END; "
                            + "SELECT @IdUsuarioRol;",
                            connection,
                            transaction))
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandTimeout = 30;
                            AgregarTexto(command, "@Usuario", 100, usuario);
                            AgregarEntero(command, "@IdRol", idRol);
                            AgregarTexto(command, "@Jefe", 100, jefe);
                            AgregarTextoLargo(command, "@Puestos", puestos);

                            idUsuarioRol = Convert.ToInt32(
                                await command.ExecuteScalarAsync(
                                    cancellationToken).ConfigureAwait(false));

                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            // La transacciÃ³n puede ya estar confirmada o revertida.
                        }

                        throw;
                    }

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

        public async Task<IList<UsuarioRol>> BuscarPorUsuarioAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var usuariosRoles = new List<UsuarioRol>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                SelectSql
                + "WHERE usuarioRol.Activo = 1 "
                + "AND usuarioRol.Usuario LIKE @Filtro "
                + "ORDER BY usuarioRol.IdUsuarioRol",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@Filtro", SqlDbType.VarChar, 102).Value =
                    "%" + filtro + "%";

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
            string puestos,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_UsuariosRoles "
                    + "SET IdRol = @IdRol, Jefe = @Jefe, Puestos = @Puestos "
                    + "WHERE IdUsuarioRol = @IdUsuarioRol", connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarEntero(command, "@IdRol", idRol);
                    AgregarTexto(command, "@Jefe", 100, jefe);
                    AgregarTextoLargo(command, "@Puestos", puestos);
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
                Puestos = LeerTexto(reader, "Puestos"),
                EstadoConexion = reader["EstadoConexion"] == DBNull.Value
                    ? false
                    : Convert.ToBoolean(reader["EstadoConexion"]),
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

        private static void AgregarTextoLargo(
            SqlCommand command,
            string nombre,
            string valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.VarChar, -1)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }
    }
}
