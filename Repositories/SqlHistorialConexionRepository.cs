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
    public class SqlHistorialConexionRepository : IHistorialConexionRepository
    {
        private readonly string connectionString;

        public SqlHistorialConexionRepository()
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

        public async Task<HistorialConexion> CrearAsync(
            HistorialConexion registro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_HistorialConexiones "
                    + "(Usuario, UsuarioAccion, Estado, Sustento) "
                    + "VALUES (@Usuario, @UsuarioAccion, @Estado, @Sustento); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@Usuario", SqlDbType.VarChar, 100).Value =
                        registro.Usuario;
                    command.Parameters.Add(
                        "@UsuarioAccion", SqlDbType.VarChar, 100).Value =
                        registro.UsuarioAccion;
                    command.Parameters.Add(
                        "@Estado", SqlDbType.Bit).Value = registro.Estado;
                    command.Parameters.Add(
                        "@Sustento", SqlDbType.NVarChar, 500).Value =
                        (object)registro.Sustento ?? DBNull.Value;

                    var idHistorialConexion = Convert.ToInt32(
                        await command.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection,
                        idHistorialConexion,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<HistorialConexion>> ObtenerPorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var registros = new List<HistorialConexion>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdHistorialConexion, Usuario, UsuarioAccion, "
                + "Estado, Sustento, Fecha "
                + "FROM dbo.CIRI_HistorialConexiones "
                + "WHERE Usuario = @Usuario "
                + "ORDER BY Fecha DESC, IdHistorialConexion DESC",
                connection))
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
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        registros.Add(LeerHistorialConexion(reader));
                    }
                }
            }

            return registros;
        }

        private static async Task<HistorialConexion> ObtenerPorIdAsync(
            SqlConnection connection,
            int idHistorialConexion,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT IdHistorialConexion, Usuario, UsuarioAccion, "
                + "Estado, Sustento, Fecha "
                + "FROM dbo.CIRI_HistorialConexiones "
                + "WHERE IdHistorialConexion = @IdHistorialConexion",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdHistorialConexion", SqlDbType.Int).Value =
                    idHistorialConexion;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerHistorialConexion(reader)
                        : null;
                }
            }
        }

        private static HistorialConexion LeerHistorialConexion(
            SqlDataReader reader)
        {
            var sustento = reader["Sustento"];
            var usuarioAccion = reader["UsuarioAccion"];

            return new HistorialConexion
            {
                IdHistorialConexion = Convert.ToInt32(
                    reader["IdHistorialConexion"]),
                Usuario = Convert.ToString(reader["Usuario"]),
                UsuarioAccion = usuarioAccion == DBNull.Value
                    ? null
                    : Convert.ToString(usuarioAccion),
                Estado = Convert.ToBoolean(reader["Estado"]),
                Sustento = sustento == DBNull.Value
                    ? null
                    : Convert.ToString(sustento),
                Fecha = Convert.ToDateTime(reader["Fecha"])
            };
        }
    }
}
