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

        public async Task<RegistrarSolicitudResponse> RegistrarAsync(
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
                AgregarEntero(command, "@IdTipoRegistro", request.IdTipoRegistro);
                AgregarEntero(command, "@IdCategoria", request.IdCategoria);
                AgregarEntero(command, "@IdSubcategoria", request.IdSubcategoria);
                AgregarEntero(command, "@IdOcurrencia", request.IdOcurrencia);
                AgregarTexto(command, "@CentroCosto", 50, request.CentroCosto);
                AgregarTexto(command, "@Lugar", 200, request.Lugar);
                AgregarTextoLargo(command, "@Descripcion", request.Descripcion);
                AgregarTextoLargo(command, "@Recomendacion", request.Recomendacion);

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                var result = await command.ExecuteScalarAsync(
                    cancellationToken).ConfigureAwait(false);

                var codigoSolicitud = result == null || result == DBNull.Value
                    ? null
                    : Convert.ToString(result);

                if (string.IsNullOrWhiteSpace(codigoSolicitud))
                {
                    return null;
                }

                using (var idCommand = new SqlCommand(
                    "SELECT TOP (1) IdSolicitud "
                    + "FROM dbo.CIRI_Solicitudes "
                    + "WHERE CodigoSolicitud = @CodigoSolicitud",
                    connection))
                {
                    idCommand.CommandType = CommandType.Text;
                    idCommand.CommandTimeout = 30;
                    idCommand.Parameters.Add(
                        "@CodigoSolicitud", SqlDbType.VarChar, 100).Value =
                        codigoSolicitud;

                    var idResult = await idCommand.ExecuteScalarAsync(
                        cancellationToken).ConfigureAwait(false);

                    if (idResult == null || idResult == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "No se encontrÃ³ el IdSolicitud registrado.");
                    }

                    using (var comentarioCommand = new SqlCommand(
                        "SELECT TOP (1) IdComentario "
                        + "FROM dbo.CIRI_ComentariosSolicitudes "
                        + "WHERE IdSolicitud = @IdSolicitud "
                        + "AND Tipo = 'Comentarios' "
                        + "ORDER BY IdComentario ASC",
                        connection))
                    {
                        comentarioCommand.CommandType = CommandType.Text;
                        comentarioCommand.CommandTimeout = 30;
                        comentarioCommand.Parameters.Add(
                            "@IdSolicitud", SqlDbType.Int).Value =
                            Convert.ToInt32(idResult);

                        var comentarioResult =
                            await comentarioCommand.ExecuteScalarAsync(
                                cancellationToken).ConfigureAwait(false);

                        return new RegistrarSolicitudResponse
                        {
                            IdSolicitud = Convert.ToInt32(idResult),
                            IdComentario = comentarioResult == null
                                || comentarioResult == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(comentarioResult),
                            CodigoSolicitud = codigoSolicitud
                        };
                    }
                }
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

        public async Task<SolicitudEnProcesoResponse> PasarAEnProcesoAsync(
            string codigoSolicitud,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_SolicitudEnProceso", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@CodigoSolicitud", SqlDbType.VarChar, 100).Value =
                    codigoSolicitud;
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                SolicitudEnProcesoResponse response;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    response = new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                    return response;
                }
            }
        }

        public async Task<SolicitudEnProcesoResponse> SolicitarInformacionAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SolicitudEnProcesoResponse response;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_SolicitarInformacion", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    response = new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }

                using (var comentarioCommand = new SqlCommand(
                    "SELECT TOP (1) IdComentario "
                    + "FROM dbo.CIRI_ComentariosSolicitudes "
                    + "WHERE IdSolicitud = @IdSolicitud "
                    + "AND Comentario = @Comentario "
                    + "AND Usuario = @Usuario "
                    + "AND Tipo = 'Comentarios' "
                    + "ORDER BY IdComentario DESC",
                    connection))
                {
                    comentarioCommand.CommandType = CommandType.Text;
                    comentarioCommand.CommandTimeout = 30;
                    comentarioCommand.Parameters.Add(
                        "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                    comentarioCommand.Parameters.Add(new SqlParameter(
                        "@Comentario", SqlDbType.NVarChar, -1)
                    {
                        Value = comentario
                    });
                    comentarioCommand.Parameters.Add(
                        "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                    var comentarioResult =
                        await comentarioCommand.ExecuteScalarAsync(
                            cancellationToken).ConfigureAwait(false);

                    response.IdComentario = comentarioResult == null
                        || comentarioResult == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(comentarioResult);
                }

                return response;
            }
        }

        public async Task<SolicitudEnProcesoResponse> EnviarInformacionAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_EnviarInformacion", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    return new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }
            }
        }

        public async Task<SolicitudEnProcesoResponse> AnularSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_AnularSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    return new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }
            }
        }

        public async Task<SolicitudEnProcesoResponse> FinalizarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_FinalizarSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    return new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }
            }
        }

        public async Task<SolicitudEnProcesoResponse> ReasignarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            string asignadoA,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_ReasignarSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                command.Parameters.Add(
                    "@AsignadoA", SqlDbType.VarChar, 100).Value = asignadoA;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    return new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }
            }
        }

        public async Task<SolicitudEnProcesoResponse> DerivarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            string asignadoA,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_DerivarSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                command.Parameters.Add(new SqlParameter(
                    "@Comentario", SqlDbType.NVarChar, -1)
                {
                    Value = comentario
                });
                command.Parameters.Add(
                    "@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                command.Parameters.Add(
                    "@AsignadoA", SqlDbType.VarChar, 100).Value = asignadoA;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió una respuesta.");
                    }

                    return new SolicitudEnProcesoResponse
                    {
                        Success = Convert.ToBoolean(reader["Success"]),
                        Mensaje = Convert.ToString(reader["Mensaje"])
                    };
                }
            }
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
