using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Entities;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Repositories
{
    public class SqlSolicitudRepository : ISolicitudRepository
    {
        private readonly string connectionString;

        public SqlSolicitudRepository()
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

        public async Task<string> RegistrarAsync(
            RegistrarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_RegistrarSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                AgregarTexto(command, "@SolicitadoPara", 100, request.SolicitadoPara);
                AgregarTexto(command, "@CreadoPor", 100, request.CreadoPor);
                AgregarEntero(command, "@IdEstadoSolicitud", request.IdEstadoSolicitud);
                AgregarEntero(command, "@IdTipoRegistro", request.IdTipoRegistro);
                AgregarEntero(command, "@IdCategoria", request.IdCategoria);
                AgregarEntero(command, "@IdSubcategoria", request.IdSubcategoria);
                AgregarEntero(command, "@IdOcurrencia", request.IdOcurrencia);
                AgregarTexto(command, "@Prioridad", 50, request.Prioridad);
                AgregarTexto(command, "@AsignadoA", 100, request.AsignadoA);
                AgregarFecha(command, "@FechaSolicitud", request.FechaSolicitud);
                AgregarFecha(command, "@FechaInicio", request.FechaInicio);
                AgregarFecha(command, "@FechaCaducidad", request.FechaCaducidad);
                AgregarFecha(command, "@FechaFinAtencion", request.FechaFinAtencion);
                AgregarEntero(command, "@DuracionAtencion", request.DuracionAtencion);
                AgregarTexto(command, "@CopiaJefatura", 100, request.CopiaJefatura);
                AgregarTexto(command, "@JefeAprobador", 100, request.JefeAprobador);
                AgregarTexto(command, "@EstadoAprobacion", 100, request.EstadoAprobacion);
                AgregarTexto(command, "@UsuarioAprobacion", 100, request.UsuarioAprobacion);
                AgregarFecha(command, "@FechaAprobacion", request.FechaAprobacion);
                AgregarEntero(command, "@Nivel", request.Nivel);
                AgregarTexto(command, "@ProblemaMasivo", 2, request.ProblemaMasivo);
                AgregarTexto(command, "@CCSolicitadoPara", 100, request.CCSolicitadoPara);
                AgregarTexto(command, "@CentroCosto", 50, request.CentroCosto);
                AgregarTexto(command, "@Lugar", 200, request.Lugar);
                AgregarTextoLargo(command, "@Descripcion", request.Descripcion);
                AgregarTextoLargo(command, "@Recomendacion", request.Recomendacion);

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                var result = await command.ExecuteScalarAsync(
                    cancellationToken).ConfigureAwait(false);

                return result == null || result == DBNull.Value
                    ? null
                    : Convert.ToString(result);
            }
        }

        public async Task<IList<SolicitudFlag>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var solicitudes = new List<SolicitudFlag>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_ListarSolicitudesXFiltro", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                command.Parameters.Add(
                    "@Pusuario_c", SqlDbType.VarChar, 100).Value = usuario;
                command.Parameters.Add(
                    "@Pflag_c", SqlDbType.VarChar, 5).Value = flag;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        solicitudes.Add(new SolicitudFlag
                        {
                            CodigoSolicitud = LeerTexto(
                                reader, "CodigoSolicitud"),
                            TipoRegistro = LeerTexto(reader, "TipoRegistro"),
                            Categoria = LeerTexto(reader, "Categoria"),
                            CreadoPor = LeerTexto(reader, "CreadoPor"),
                            SolicitadoPara = LeerTexto(
                                reader, "SolicitadoPara"),
                            AsignadoA = LeerTexto(reader, "AsignadoA"),
                            FechaSolicitud = Convert.ToDateTime(
                                reader["FechaSolicitud"]),
                            FechaEstimada = LeerFechaNullable(
                                reader, "FechaEstimada"),
                            CopiaJefatura = LeerTexto(
                                reader, "CopiaJefatura"),
                            Estado = LeerTexto(reader, "Estado")
                        });
                    }
                }
            }

            return solicitudes;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
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

        private static void AgregarTextoLargo(
            SqlCommand command,
            string nombre,
            string valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.NVarChar, -1)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }

        private static void AgregarEntero(
            SqlCommand command,
            string nombre,
            int? valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.Int)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }

        private static void AgregarFecha(
            SqlCommand command,
            string nombre,
            DateTime? valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.DateTime2)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }
    }
}
