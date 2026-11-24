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
    public class SqlConexionUsuarioRepository : IConexionUsuarioRepository
    {
        private readonly string connectionString;

        public SqlConexionUsuarioRepository()
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

        public async Task<ConexionUsuario> CrearAsync(
            string usuario,
            string usuarioAccion,
            string sustento,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var transaction = connection.BeginTransaction())
                {
                    var idConexion = 0;

                    try
                    {
                        using (var command = new SqlCommand(
                            "SET NOCOUNT ON; "
                            + "SET XACT_ABORT ON; "
                            + "DECLARE @IdConexion INT; "
                            + "DECLARE @Estado BIT; "
                            + "DECLARE @EsPrimerHistorial BIT; "
                            + "SELECT TOP (1) @IdConexion = IdConexion, "
                            + "@Estado = Estado "
                            + "FROM dbo.CIRI_ConexionesUsuarios WITH (UPDLOCK, HOLDLOCK) "
                            + "WHERE Usuario = @Usuario "
                            + "ORDER BY IdConexion; "
                            + "IF @IdConexion IS NULL "
                            + "BEGIN "
                            + "SET @ResultadoId = NULL; "
                            + "RETURN; "
                            + "END "
                            + "IF EXISTS ("
                            + "SELECT 1 FROM dbo.CIRI_HistorialConexiones "
                            + "WITH (UPDLOCK, HOLDLOCK) "
                            + "WHERE Usuario = @Usuario) "
                            + "SET @EsPrimerHistorial = 0; "
                            + "ELSE "
                            + "SET @EsPrimerHistorial = 1; "
                            + "IF @EsPrimerHistorial = 1 "
                            + "BEGIN "
                            + "SET @Estado = 0; "
                            + "UPDATE dbo.CIRI_ConexionesUsuarios "
                            + "SET Fecha = SYSDATETIME(), Estado = 0 "
                            + "WHERE IdConexion = @IdConexion; "
                            + "END "
                            + "ELSE "
                            + "BEGIN "
                            + "SET @Estado = CASE WHEN @Estado = 1 THEN 0 ELSE 1 END; "
                            + "UPDATE dbo.CIRI_ConexionesUsuarios "
                            + "SET Fecha = SYSDATETIME(), Estado = @Estado "
                            + "WHERE IdConexion = @IdConexion; "
                            + "END; "
                            + "IF @Estado = 1 "
                            + "BEGIN "
                            + "EXEC dbo.pr_ciri_AsignacionAutomaticaSolicitud "
                            + "@Usuario = @Usuario; "
                            + "END; "
                            + "IF @EsPrimerHistorial = 1 "
                            + "SET @Sustento = N'Registro'; "
                            + "INSERT INTO dbo.CIRI_HistorialConexiones "
                            + "(Usuario, UsuarioAccion, Estado, Sustento) "
                            + "VALUES (@Usuario, @UsuarioAccion, @Estado, @Sustento); "
                            + "SET @ResultadoId = @IdConexion;",
                            connection,
                            transaction))
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandTimeout = 30;
                            command.Parameters.Add(
                                "@Usuario", SqlDbType.VarChar, 100).Value =
                                usuario;
                            command.Parameters.Add(
                                "@UsuarioAccion", SqlDbType.VarChar, 100).Value =
                                usuarioAccion;
                            command.Parameters.Add(
                                "@Sustento", SqlDbType.NVarChar, 500).Value =
                                (object)sustento ?? DBNull.Value;

                            var resultadoId = new SqlParameter(
                                "@ResultadoId", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            command.Parameters.Add(resultadoId);

                            await command.ExecuteNonQueryAsync(
                                cancellationToken).ConfigureAwait(false);

                            if (resultadoId.Value == null
                                || resultadoId.Value == DBNull.Value)
                            {
                                transaction.Commit();
                                return null;
                            }

                            idConexion = Convert.ToInt32(resultadoId.Value);

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
                        idConexion,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<ConexionUsuario> ObtenerPorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT TOP (1) IdConexion, Usuario, Fecha, Estado "
                + "FROM dbo.CIRI_ConexionesUsuarios "
                + "WHERE Usuario = @Usuario "
                + "ORDER BY IdConexion", connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerConexionUsuario(reader)
                        : null;
                }
            }
        }

        public async Task<IList<ConexionUsuario>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var conexiones = new List<ConexionUsuario>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdConexion, Usuario, Fecha, Estado "
                + "FROM dbo.CIRI_ConexionesUsuarios "
                + "ORDER BY IdConexion", connection))
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
                        conexiones.Add(LeerConexionUsuario(reader));
                    }
                }
            }

            return conexiones;
        }

        private static async Task<ConexionUsuario> ObtenerPorIdAsync(
            SqlConnection connection,
            int idConexion,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT IdConexion, Usuario, Fecha, Estado "
                + "FROM dbo.CIRI_ConexionesUsuarios "
                + "WHERE IdConexion = @IdConexion",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdConexion", SqlDbType.Int).Value = idConexion;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerConexionUsuario(reader)
                        : null;
                }
            }
        }

        private static ConexionUsuario LeerConexionUsuario(
            SqlDataReader reader)
        {
            return new ConexionUsuario
            {
                IdConexion = Convert.ToInt32(reader["IdConexion"]),
                Usuario = Convert.ToString(reader["Usuario"]),
                Fecha = Convert.ToDateTime(reader["Fecha"]),
                Estado = Convert.ToBoolean(reader["Estado"])
            };
        }
    }
}
