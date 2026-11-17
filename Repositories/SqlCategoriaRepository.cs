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
    public class SqlCategoriaRepository : ICategoriaRepository
    {
        private readonly string connectionString;

        public SqlCategoriaRepository()
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

        public async Task<IList<Categoria>> ObtenerPorTipoRegistroAsync(
            int idTipoRegistro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var categorias = new List<Categoria>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT categoria.IdCategoria, "
                + "categoria.IdTipoRegistro, "
                + "tipoRegistro.Nombre AS TipoRegistro, "
                + "categoria.Nombre, categoria.Descripcion, "
                + "categoria.Activo, categoria.FechaRegistro "
                + "FROM dbo.CIRI_Categorias AS categoria "
                + "INNER JOIN dbo.CIRI_TipoRegistros AS tipoRegistro "
                + "ON tipoRegistro.IdTipoRegistro = categoria.IdTipoRegistro "
                + "WHERE categoria.IdTipoRegistro = @IdTipoRegistro "
                + "ORDER BY categoria.IdCategoria",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdTipoRegistro", SqlDbType.Int).Value = idTipoRegistro;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        categorias.Add(new Categoria
                        {
                            IdCategoria = Convert.ToInt32(
                                reader["IdCategoria"]),
                            IdTipoRegistro = Convert.ToInt32(
                                reader["IdTipoRegistro"]),
                            TipoRegistro = LeerTexto(
                                reader, "TipoRegistro"),
                            Nombre = LeerTexto(reader, "Nombre"),
                            Descripcion = LeerTexto(
                                reader, "Descripcion"),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return categorias;
        }

        private static string LeerTexto(
            SqlDataReader reader,
            string columna)
        {
            var valor = reader[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }
    }
}
