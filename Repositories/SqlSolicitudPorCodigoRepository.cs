using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Repositories
{
    public class SqlSolicitudPorCodigoRepository : ISolicitudPorCodigoRepository
    {
        private readonly string connectionString;

        public SqlSolicitudPorCodigoRepository()
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

        public async Task<SolicitudPorCodigo> BuscarAsync(
            string codigoSolicitud,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_ciri_BuscarSolicitudXCodigoSolicitud", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@PcodigoSolicitud_c", SqlDbType.VarChar, 100).Value =
                    codigoSolicitud;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        return null;
                    }

                    var solicitud = new SolicitudPorCodigo
                    {
                        IdSolicitud = Convert.ToInt32(reader["IdSolicitud"]),
                        CodigoSolicitud = LeerTexto(
                            reader, "CodigoSolicitud") ?? string.Empty,
                        CreadoPor = LeerTexto(reader, "CreadoPor"),
                        SolicitadoPara = LeerTexto(reader, "SolicitadoPara"),
                        Ubicacion = LeerTexto(reader, "Ubicacion"),
                        Area = LeerTexto(reader, "Area"),
                        Telefono = LeerTexto(reader, "Telefono"),
                        ContactoArea = LeerTexto(reader, "ContactoArea"),
                        Correo = LeerTexto(reader, "Correo"),
                        TelefonoContactoArea = LeerTexto(
                            reader, "TelefonoContactoArea"),
                        TipoRegistro = LeerTexto(reader, "TipoRegistro"),
                        Estado = LeerTexto(reader, "Estado"),
                        Categoria = LeerTexto(reader, "Categoria"),
                        Subcategoria = LeerTexto(reader, "Subcategoria"),
                        Ocurrencia = LeerTexto(reader, "Ocurrencia"),
                        CopiaJefatura = LeerTexto(reader, "CopiaJefatura"),
                        FechaSolicitud = Convert.ToDateTime(
                            reader["FechaSolicitud"]),
                        AsignadoA = LeerTexto(reader, "AsignadoA"),
                        FechaInicioAtencion = LeerFechaNullable(
                            reader, "FechaInicioAtencion"),
                        FechaCaducidad = LeerFechaNullable(
                            reader, "FechaCaducidad"),
                        CentroCosto = LeerTexto(reader, "CentroCosto"),
                        NombreCentroCosto = LeerTexto(
                            reader, "NombreCentroCosto"),
                        Lugar = LeerTexto(reader, "Lugar")
                    };

                    if (await reader.NextResultAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync(cancellationToken)
                            .ConfigureAwait(false))
                        {
                            solicitud.Comentarios.Add(
                                new SolicitudComentarioDetalle
                                {
                                    IdComentario = Convert.ToInt32(
                                        reader["IdComentario"]),
                                    Comentario = LeerTexto(
                                        reader, "Comentario"),
                                    Tipo = LeerTexto(reader, "Tipo"),
                                    Usuario = LeerTexto(reader, "Usuario"),
                                    FechaRegistro = Convert.ToDateTime(
                                        reader["FechaRegistro"])
                                });
                        }
                    }

                    return solicitud;
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
    }
}
