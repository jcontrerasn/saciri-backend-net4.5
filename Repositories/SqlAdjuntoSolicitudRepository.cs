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
    public class SqlAdjuntoSolicitudRepository : IAdjuntoSolicitudRepository
    {
        private readonly string connectionString;

        public SqlAdjuntoSolicitudRepository()
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

        public async Task<AdjuntoSolicitud> CrearAsync(
            int idSolicitud,
            string nombreAdjunto,
            string formato,
            long tamanoAdjunto,
            string ruta,
            string adjuntadoPor,
            CancellationToken cancellationToken = default(CancellationToken),
            bool primerHistorial = false,
            int? idComentario = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ordenHistorial = primerHistorial
                    ? "ORDER BY IdHistorialSolicitud ASC; "
                    : "ORDER BY FechaRegistro DESC, "
                        + "IdHistorialSolicitud DESC; ";

                using (var command = new SqlCommand(
                    "DECLARE @IdHistorialSolicitud INT; "
                    + "DECLARE @IdComentario INT = @IdComentarioParametro; "
                    + "SELECT TOP (1) @IdHistorialSolicitud = "
                    + "IdHistorialSolicitud "
                    + "FROM dbo.CIRI_HistorialSolicitudes "
                    + "WHERE IdSolicitud = @IdSolicitud "
                    + ordenHistorial
                    + "IF @IdHistorialSolicitud IS NULL "
                    + "BEGIN "
                    + "SELECT CAST(NULL AS INT); "
                    + "RETURN; "
                    + "END; "
                    + "IF @IdComentario IS NULL "
                    + "BEGIN "
                    + "SELECT TOP (1) @IdComentario = c.IdComentario "
                    + "FROM dbo.CIRI_ComentariosSolicitudes AS c "
                    + "WHERE c.IdSolicitud = @IdSolicitud "
                    + "AND c.Tipo = 'Comentarios' "
                    + "ORDER BY c.IdComentario ASC; "
                    + "END; "
                    + "IF @IdComentario IS NOT NULL "
                    + "AND NOT EXISTS (SELECT 1 "
                    + "FROM dbo.CIRI_ComentariosSolicitudes "
                    + "WHERE IdComentario = @IdComentario "
                    + "AND IdSolicitud = @IdSolicitud) "
                    + "BEGIN "
                    + "SELECT CAST(NULL AS INT); "
                    + "RETURN; "
                    + "END; "
                    + "INSERT INTO dbo.CIRI_AdjuntosSolicitudes "
                    + "(IdHistorialSolicitud, IdComentario, NombreAdjunto, Formato, "
                    + "TamanoAdjunto, Ruta, AdjuntadoPor) "
                    + "VALUES (@IdHistorialSolicitud, @IdComentario, "
                    + "@NombreAdjunto, @Formato, "
                    + "@TamanoAdjunto, @Ruta, @AdjuntadoPor); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    command.Parameters.Add(
                        "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                    command.Parameters.Add(new SqlParameter(
                        "@IdComentarioParametro", SqlDbType.Int)
                    {
                        Value = (object)idComentario ?? DBNull.Value
                    });
                    command.Parameters.Add(
                        "@NombreAdjunto", SqlDbType.NVarChar, 255).Value =
                        nombreAdjunto;
                    command.Parameters.Add(
                        "@Formato", SqlDbType.VarChar, 100).Value = formato;
                    command.Parameters.Add(
                        "@TamanoAdjunto", SqlDbType.BigInt).Value =
                        tamanoAdjunto;
                    command.Parameters.Add(
                        "@Ruta", SqlDbType.NVarChar, 500).Value = ruta;
                    command.Parameters.Add(
                        "@AdjuntadoPor", SqlDbType.VarChar, 100).Value =
                        adjuntadoPor;

                    var result = await command.ExecuteScalarAsync(
                        cancellationToken).ConfigureAwait(false);

                    if (result == null || result == DBNull.Value)
                    {
                        return null;
                    }

                    var idAdjunto = Convert.ToInt32(result);
                    return await ObtenerPorIdAsync(
                        connection,
                        idAdjunto,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async Task<AdjuntoSolicitud> ObtenerPorIdAsync(
            SqlConnection connection,
            int idAdjunto,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT adjunto.IdAdjunto, historial.IdSolicitud, "
                + "adjunto.IdHistorialSolicitud, adjunto.IdComentario, "
                + "adjunto.NombreAdjunto, "
                + "adjunto.Formato, adjunto.TamanoAdjunto, adjunto.Ruta, "
                + "adjunto.AdjuntadoPor, adjunto.Fecha, adjunto.Estado "
                + "FROM dbo.CIRI_AdjuntosSolicitudes AS adjunto "
                + "INNER JOIN dbo.CIRI_HistorialSolicitudes AS historial "
                + "ON historial.IdHistorialSolicitud = "
                + "adjunto.IdHistorialSolicitud "
                + "WHERE adjunto.IdAdjunto = @IdAdjunto",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdAdjunto", SqlDbType.Int).Value = idAdjunto;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    return await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ? LeerAdjunto(reader)
                        : null;
                }
            }
        }

        public async Task<IList<AdjuntoSolicitud>> ObtenerPorSolicitudAsync(
            int idSolicitud,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var adjuntos = new List<AdjuntoSolicitud>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT adjunto.IdAdjunto, historial.IdSolicitud, "
                + "adjunto.IdHistorialSolicitud, adjunto.IdComentario, "
                + "adjunto.NombreAdjunto, adjunto.Formato, "
                + "adjunto.TamanoAdjunto, adjunto.Ruta, "
                + "adjunto.AdjuntadoPor, adjunto.Fecha, adjunto.Estado "
                + "FROM dbo.CIRI_AdjuntosSolicitudes AS adjunto "
                + "INNER JOIN dbo.CIRI_HistorialSolicitudes AS historial "
                + "ON historial.IdHistorialSolicitud = "
                + "adjunto.IdHistorialSolicitud "
                + "WHERE historial.IdSolicitud = @IdSolicitud "
                + "AND adjunto.Estado = 1 "
                + "ORDER BY adjunto.Fecha ASC, adjunto.IdAdjunto ASC",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        adjuntos.Add(LeerAdjunto(reader));
                    }
                }
            }

            return adjuntos;
        }

        public async Task<AdjuntoSolicitud> ObtenerPorIdAsync(
            int idAdjunto,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                return await ObtenerPorIdAsync(
                    connection,
                    idAdjunto,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static AdjuntoSolicitud LeerAdjunto(
            SqlDataReader reader)
        {
            return new AdjuntoSolicitud
            {
                IdAdjunto = Convert.ToInt32(reader["IdAdjunto"]),
                IdSolicitud = Convert.ToInt32(reader["IdSolicitud"]),
                IdHistorialSolicitud = Convert.ToInt32(
                    reader["IdHistorialSolicitud"]),
                IdComentario = reader["IdComentario"] == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(reader["IdComentario"]),
                NombreAdjunto = Convert.ToString(reader["NombreAdjunto"]),
                Formato = Convert.ToString(reader["Formato"]),
                TamanoAdjunto = Convert.ToInt64(reader["TamanoAdjunto"]),
                Ruta = Convert.ToString(reader["Ruta"]),
                AdjuntadoPor = Convert.ToString(reader["AdjuntadoPor"]),
                Fecha = Convert.ToDateTime(reader["Fecha"]),
                Estado = Convert.ToBoolean(reader["Estado"])
            };
        }
    }
}
