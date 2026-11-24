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
    public class SqlCatSubOcuRepository : ICatSubOcuRepository
    {
        private readonly string connectionString;

        public SqlCatSubOcuRepository()
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

        public async Task<CatSubOcu> CrearAsync(
            CatSubOcu registro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "INSERT INTO dbo.CIRI_CatSubOcu ("
                    + "IdTipoRegistro, IdCategoria, IdSubcategoria, "
                    + "IdOcurrencia, RequiereAprobacion, NivelAprobacion, "
                    + "RolAprobacion, TiempoAtencion, Estado, Atencion, Nivel1, "
                    + "Nivel2, Nivel3, Copia, DescripcionSubCatOcu, "
                    + "DatosAdjuntos, CopiaJefatura) "
                    + "VALUES (@IdTipoRegistro, @IdCategoria, "
                    + "@IdSubcategoria, @IdOcurrencia, @RequiereAprobacion, "
                    + "@NivelAprobacion, @RolAprobacion, @TiempoAtencion, "
                    + "@Estado, @Atencion, @Nivel1, @Nivel2, @Nivel3, @Copia, "
                    + "@DescripcionSubCatOcu, @DatosAdjuntos, "
                    + "@CopiaJefatura); "
                    + "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarEntero(command, "@IdTipoRegistro", registro.IdTipoRegistro);
                    AgregarEntero(command, "@IdCategoria", registro.IdCategoria);
                    AgregarEntero(command, "@IdSubcategoria", registro.IdSubcategoria);
                    AgregarEntero(command, "@IdOcurrencia", registro.IdOcurrencia);
                    AgregarBooleano(
                        command, "@RequiereAprobacion",
                        registro.RequiereAprobacion);
                    AgregarTexto(
                        command, "@NivelAprobacion", 5,
                        registro.NivelAprobacion);
                    AgregarTexto(
                        command, "@RolAprobacion", 10,
                        registro.RolAprobacion);
                    AgregarEnteroNullable(
                        command, "@TiempoAtencion", registro.TiempoAtencion);
                    AgregarBooleano(command, "@Estado", registro.Estado);
                    AgregarEnteroNullable(
                        command, "@Atencion", registro.Atencion);
                    AgregarEnteroNullable(command, "@Nivel1", registro.Nivel1);
                    AgregarEnteroNullable(command, "@Nivel2", registro.Nivel2);
                    AgregarEnteroNullable(command, "@Nivel3", registro.Nivel3);
                    AgregarTexto(command, "@Copia", 100, registro.Copia);
                    AgregarTextoLargo(
                        command, "@DescripcionSubCatOcu",
                        registro.DescripcionSubCatOcu);
                    AgregarBooleano(
                        command, "@DatosAdjuntos", registro.DatosAdjuntos);
                    AgregarBooleano(
                        command, "@CopiaJefatura", registro.CopiaJefatura);

                    var id = Convert.ToInt32(await command.ExecuteScalarAsync(
                        cancellationToken).ConfigureAwait(false));

                    return await ObtenerPorIdAsync(
                        connection, id, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<CatSubOcu> ActualizarAsync(
            int id,
            CatSubOcu registro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var command = new SqlCommand(
                    "UPDATE dbo.CIRI_CatSubOcu SET "
                    + "IdTipoRegistro = @IdTipoRegistro, "
                    + "IdCategoria = @IdCategoria, "
                    + "IdSubcategoria = @IdSubcategoria, "
                    + "IdOcurrencia = @IdOcurrencia, "
                    + "RequiereAprobacion = @RequiereAprobacion, "
                    + "NivelAprobacion = @NivelAprobacion, "
                    + "RolAprobacion = @RolAprobacion, "
                    + "TiempoAtencion = @TiempoAtencion, "
                    + "Estado = @Estado, Atencion = @Atencion, "
                    + "Nivel1 = @Nivel1, Nivel2 = @Nivel2, Nivel3 = @Nivel3, "
                    + "Copia = @Copia, "
                    + "DescripcionSubCatOcu = @DescripcionSubCatOcu, "
                    + "DatosAdjuntos = @DatosAdjuntos, "
                    + "CopiaJefatura = @CopiaJefatura "
                    + "WHERE Id = @Id", connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30;
                    AgregarEntero(command, "@Id", id);
                    AgregarEntero(command, "@IdTipoRegistro", registro.IdTipoRegistro);
                    AgregarEntero(command, "@IdCategoria", registro.IdCategoria);
                    AgregarEntero(command, "@IdSubcategoria", registro.IdSubcategoria);
                    AgregarEntero(command, "@IdOcurrencia", registro.IdOcurrencia);
                    AgregarBooleano(
                        command, "@RequiereAprobacion",
                        registro.RequiereAprobacion);
                    AgregarTexto(
                        command, "@NivelAprobacion", 5,
                        registro.NivelAprobacion);
                    AgregarTexto(
                        command, "@RolAprobacion", 10,
                        registro.RolAprobacion);
                    AgregarEnteroNullable(
                        command, "@TiempoAtencion", registro.TiempoAtencion);
                    AgregarBooleano(command, "@Estado", registro.Estado);
                    AgregarEnteroNullable(
                        command, "@Atencion", registro.Atencion);
                    AgregarEnteroNullable(command, "@Nivel1", registro.Nivel1);
                    AgregarEnteroNullable(command, "@Nivel2", registro.Nivel2);
                    AgregarEnteroNullable(command, "@Nivel3", registro.Nivel3);
                    AgregarTexto(command, "@Copia", 100, registro.Copia);
                    AgregarTextoLargo(
                        command, "@DescripcionSubCatOcu",
                        registro.DescripcionSubCatOcu);
                    AgregarBooleano(
                        command, "@DatosAdjuntos", registro.DatosAdjuntos);
                    AgregarBooleano(
                        command, "@CopiaJefatura", registro.CopiaJefatura);

                    if (await command.ExecuteNonQueryAsync(
                        cancellationToken).ConfigureAwait(false) == 0)
                    {
                        return null;
                    }

                    return await ObtenerPorIdAsync(
                        connection, id, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<IList<CatSubOcu>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var registros = new List<CatSubOcu>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT catSubOcu.Id, catSubOcu.IdTipoRegistro, "
                + "catSubOcu.IdCategoria, catSubOcu.IdSubcategoria, "
                + "catSubOcu.IdOcurrencia, tipoRegistro.Nombre AS TipoRegistro, "
                + "categoria.Nombre AS Categoria, "
                + "subcategoria.Nombre AS Subcategoria, "
                + "ocurrencia.Nombre AS Ocurrencia, "
                + "catSubOcu.RequiereAprobacion, catSubOcu.NivelAprobacion, "
                + "catSubOcu.RolAprobacion, catSubOcu.TiempoAtencion, "
                + "catSubOcu.Estado, catSubOcu.Atencion, catSubOcu.Nivel1, "
                + "catSubOcu.Nivel2, catSubOcu.Nivel3, catSubOcu.Copia, "
                + "catSubOcu.DescripcionSubCatOcu, catSubOcu.DatosAdjuntos, "
                + "catSubOcu.CopiaJefatura "
                + "FROM dbo.CIRI_CatSubOcu AS catSubOcu "
                + "INNER JOIN dbo.CIRI_TipoRegistros AS tipoRegistro "
                + "ON tipoRegistro.IdTipoRegistro = catSubOcu.IdTipoRegistro "
                + "INNER JOIN dbo.CIRI_Categorias AS categoria "
                + "ON categoria.IdCategoria = catSubOcu.IdCategoria "
                + "INNER JOIN dbo.CIRI_Subcategorias AS subcategoria "
                + "ON subcategoria.IdSubcategoria = catSubOcu.IdSubcategoria "
                + "INNER JOIN dbo.CIRI_Ocurrencias AS ocurrencia "
                + "ON ocurrencia.IdOcurrencia = catSubOcu.IdOcurrencia "
                + "ORDER BY catSubOcu.Id", connection))
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
                        registros.Add(new CatSubOcu
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IdTipoRegistro = Convert.ToInt32(
                                reader["IdTipoRegistro"]),
                            IdCategoria = Convert.ToInt32(
                                reader["IdCategoria"]),
                            IdSubcategoria = Convert.ToInt32(
                                reader["IdSubcategoria"]),
                            IdOcurrencia = Convert.ToInt32(
                                reader["IdOcurrencia"]),
                            TipoRegistro = LeerTexto(reader, "TipoRegistro"),
                            Categoria = LeerTexto(reader, "Categoria"),
                            Subcategoria = LeerTexto(reader, "Subcategoria"),
                            Ocurrencia = LeerTexto(reader, "Ocurrencia"),
                            RequiereAprobacion = Convert.ToBoolean(
                                reader["RequiereAprobacion"]),
                            NivelAprobacion = LeerTexto(
                                reader, "NivelAprobacion"),
                            RolAprobacion = LeerTexto(
                                reader, "RolAprobacion"),
                            TiempoAtencion = LeerEnteroNullable(
                                reader, "TiempoAtencion"),
                            Estado = Convert.ToBoolean(reader["Estado"]),
                            Atencion = LeerEnteroNullable(
                                reader, "Atencion"),
                            Nivel1 = LeerEnteroNullable(reader, "Nivel1"),
                            Nivel2 = LeerEnteroNullable(reader, "Nivel2"),
                            Nivel3 = LeerEnteroNullable(reader, "Nivel3"),
                            Copia = LeerTexto(reader, "Copia"),
                            DescripcionSubCatOcu = LeerTexto(
                                reader, "DescripcionSubCatOcu"),
                            DatosAdjuntos = Convert.ToBoolean(
                                reader["DatosAdjuntos"]),
                            CopiaJefatura = Convert.ToBoolean(
                                reader["CopiaJefatura"])
                        });
                    }
                }
            }

            return registros;
        }

        public async Task<CatSubOcu> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                return await ObtenerPorIdAsync(
                    connection, id, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IList<CatSubOcu>> ObtenerPorOcurrenciaAsync(
            int idOcurrencia,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var registros = new List<CatSubOcu>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT Copia, DatosAdjuntos, CopiaJefatura, "
                + "DescripcionSubCatOcu "
                + "FROM dbo.CIRI_CatSubOcu "
                + "WHERE IdOcurrencia = @IdOcurrencia "
                + "ORDER BY Id", connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdOcurrencia", SqlDbType.Int).Value = idOcurrencia;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        registros.Add(new CatSubOcu
                        {
                            Copia = LeerTexto(reader, "Copia"),
                            DatosAdjuntos = Convert.ToBoolean(
                                reader["DatosAdjuntos"]),
                            CopiaJefatura = Convert.ToBoolean(
                                reader["CopiaJefatura"]),
                            DescripcionSubCatOcu = LeerTexto(
                                reader, "DescripcionSubCatOcu")
                        });
                    }
                }
            }

            return registros;
        }

        private static async Task<CatSubOcu> ObtenerPorIdAsync(
            SqlConnection connection,
            int id,
            CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(
                "SELECT catSubOcu.Id, catSubOcu.IdTipoRegistro, "
                + "catSubOcu.IdCategoria, catSubOcu.IdSubcategoria, "
                + "catSubOcu.IdOcurrencia, tipoRegistro.Nombre AS TipoRegistro, "
                + "categoria.Nombre AS Categoria, "
                + "subcategoria.Nombre AS Subcategoria, "
                + "ocurrencia.Nombre AS Ocurrencia, "
                + "catSubOcu.RequiereAprobacion, catSubOcu.NivelAprobacion, "
                + "catSubOcu.RolAprobacion, catSubOcu.TiempoAtencion, "
                + "catSubOcu.Estado, catSubOcu.Atencion, catSubOcu.Nivel1, "
                + "catSubOcu.Nivel2, catSubOcu.Nivel3, catSubOcu.Copia, "
                + "catSubOcu.DescripcionSubCatOcu, catSubOcu.DatosAdjuntos, "
                + "catSubOcu.CopiaJefatura "
                + "FROM dbo.CIRI_CatSubOcu AS catSubOcu "
                + "INNER JOIN dbo.CIRI_TipoRegistros AS tipoRegistro "
                + "ON tipoRegistro.IdTipoRegistro = catSubOcu.IdTipoRegistro "
                + "INNER JOIN dbo.CIRI_Categorias AS categoria "
                + "ON categoria.IdCategoria = catSubOcu.IdCategoria "
                + "INNER JOIN dbo.CIRI_Subcategorias AS subcategoria "
                + "ON subcategoria.IdSubcategoria = catSubOcu.IdSubcategoria "
                + "INNER JOIN dbo.CIRI_Ocurrencias AS ocurrencia "
                + "ON ocurrencia.IdOcurrencia = catSubOcu.IdOcurrencia "
                + "WHERE catSubOcu.Id = @Id", connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        return null;
                    }

                    return LeerCatSubOcu(reader);
                }
            }
        }

        private static CatSubOcu LeerCatSubOcu(SqlDataReader reader)
        {
            return new CatSubOcu
            {
                Id = Convert.ToInt32(reader["Id"]),
                IdTipoRegistro = Convert.ToInt32(reader["IdTipoRegistro"]),
                IdCategoria = Convert.ToInt32(reader["IdCategoria"]),
                IdSubcategoria = Convert.ToInt32(reader["IdSubcategoria"]),
                IdOcurrencia = Convert.ToInt32(reader["IdOcurrencia"]),
                TipoRegistro = LeerTexto(reader, "TipoRegistro"),
                Categoria = LeerTexto(reader, "Categoria"),
                Subcategoria = LeerTexto(reader, "Subcategoria"),
                Ocurrencia = LeerTexto(reader, "Ocurrencia"),
                RequiereAprobacion = Convert.ToBoolean(
                    reader["RequiereAprobacion"]),
                NivelAprobacion = LeerTexto(reader, "NivelAprobacion"),
                RolAprobacion = LeerTexto(reader, "RolAprobacion"),
                TiempoAtencion = LeerEnteroNullable(
                    reader, "TiempoAtencion"),
                Estado = Convert.ToBoolean(reader["Estado"]),
                Atencion = LeerEnteroNullable(reader, "Atencion"),
                Nivel1 = LeerEnteroNullable(reader, "Nivel1"),
                Nivel2 = LeerEnteroNullable(reader, "Nivel2"),
                Nivel3 = LeerEnteroNullable(reader, "Nivel3"),
                Copia = LeerTexto(reader, "Copia"),
                DescripcionSubCatOcu = LeerTexto(
                    reader, "DescripcionSubCatOcu"),
                DatosAdjuntos = Convert.ToBoolean(reader["DatosAdjuntos"]),
                CopiaJefatura = Convert.ToBoolean(reader["CopiaJefatura"])
            };
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

        private static void AgregarEnteroNullable(
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

        private static void AgregarBooleano(
            SqlCommand command,
            string nombre,
            bool valor)
        {
            command.Parameters.Add(new SqlParameter(
                nombre, SqlDbType.Bit)
            {
                Value = valor
            });
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
                nombre, SqlDbType.VarChar, -1)
            {
                Value = (object)valor ?? DBNull.Value
            });
        }
    }
}
