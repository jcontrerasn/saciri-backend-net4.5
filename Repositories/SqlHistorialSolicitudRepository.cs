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
    public class SqlHistorialSolicitudRepository : IHistorialSolicitudRepository
    {
        private readonly string connectionString;

        public SqlHistorialSolicitudRepository()
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

        public async Task<IList<HistorialSolicitud>> ObtenerPorSolicitudAsync(
            int idSolicitud,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var historial = new List<HistorialSolicitud>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdHistorialSolicitud, IdSolicitud, SolicitadoPara, "
                + "CreadoPor, CodigoSolicitud, IdEstadoSolicitud, "
                + "FechaSolicitud, IdTipoRegistro, IdCategoria, "
                + "IdSubcategoria, IdOcurrencia, Prioridad, AsignadoA, "
                + "FechaInicio, FechaCaducidad, FechaFinAtencion, "
                + "DuracionAtencion, CopiaJefatura, JefeAprobador, "
                + "EstadoAprobacion, UsuarioAprobacion, FechaAprobacion, "
                + "Nivel, ProblemaMasivo, CCSolicitadoPara, Accion, "
                + "UsuarioModificacion, Observacion, IdEstadoAnterior, "
                + "IdEstadoNuevo, FechaRegistro "
                + "FROM dbo.CIRI_HistorialSolicitudes "
                + "WHERE IdSolicitud = @IdSolicitud "
                + "ORDER BY FechaRegistro, IdHistorialSolicitud", connection))
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
                        historial.Add(new HistorialSolicitud
                        {
                            IdHistorialSolicitud = Convert.ToInt32(
                                reader["IdHistorialSolicitud"]),
                            IdSolicitud = Convert.ToInt32(
                                reader["IdSolicitud"]),
                            SolicitadoPara = LeerTexto(
                                reader, "SolicitadoPara"),
                            CreadoPor = LeerTexto(reader, "CreadoPor"),
                            CodigoSolicitud = LeerTexto(
                                reader, "CodigoSolicitud"),
                            IdEstadoSolicitud = Convert.ToInt32(
                                reader["IdEstadoSolicitud"]),
                            FechaSolicitud = Convert.ToDateTime(
                                reader["FechaSolicitud"]),
                            IdTipoRegistro = Convert.ToInt32(
                                reader["IdTipoRegistro"]),
                            IdCategoria = Convert.ToInt32(
                                reader["IdCategoria"]),
                            IdSubcategoria = LeerEnteroNullable(
                                reader, "IdSubcategoria"),
                            IdOcurrencia = LeerEnteroNullable(
                                reader, "IdOcurrencia"),
                            Prioridad = LeerTexto(reader, "Prioridad"),
                            AsignadoA = LeerTexto(reader, "AsignadoA"),
                            FechaInicio = LeerFechaNullable(
                                reader, "FechaInicio"),
                            FechaCaducidad = LeerFechaNullable(
                                reader, "FechaCaducidad"),
                            FechaFinAtencion = LeerFechaNullable(
                                reader, "FechaFinAtencion"),
                            DuracionAtencion = LeerEnteroNullable(
                                reader, "DuracionAtencion"),
                            CopiaJefatura = LeerTexto(
                                reader, "CopiaJefatura"),
                            JefeAprobador = LeerTexto(
                                reader, "JefeAprobador"),
                            EstadoAprobacion = LeerTexto(
                                reader, "EstadoAprobacion"),
                            UsuarioAprobacion = LeerTexto(
                                reader, "UsuarioAprobacion"),
                            FechaAprobacion = LeerFechaNullable(
                                reader, "FechaAprobacion"),
                            Nivel = LeerEnteroNullable(reader, "Nivel"),
                            ProblemaMasivo = LeerTexto(
                                reader, "ProblemaMasivo"),
                            CCSolicitadoPara = LeerTexto(
                                reader, "CCSolicitadoPara"),
                            Accion = LeerTexto(reader, "Accion"),
                            UsuarioModificacion = LeerTexto(
                                reader, "UsuarioModificacion"),
                            Observacion = LeerTexto(reader, "Observacion"),
                            IdEstadoAnterior = LeerEnteroNullable(
                                reader, "IdEstadoAnterior"),
                            IdEstadoNuevo = Convert.ToInt32(
                                reader["IdEstadoNuevo"]),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return historial;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }

        private static int? LeerEnteroNullable(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(valor);
        }

        private static DateTime? LeerFechaNullable(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(valor);
        }
    }
}
