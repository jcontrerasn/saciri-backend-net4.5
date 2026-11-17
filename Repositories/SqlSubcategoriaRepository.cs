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
    public class SqlSubcategoriaRepository : ISubcategoriaRepository
    {
        private readonly string connectionString;

        public SqlSubcategoriaRepository()
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

        public async Task<IList<Subcategoria>> ObtenerPorCategoriaAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var subcategorias = new List<Subcategoria>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdSubcategoria, IdCategoria, Nombre, "
                + "Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Subcategorias "
                + "WHERE IdCategoria = @IdCategoria "
                + "ORDER BY IdSubcategoria",
                connection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "@IdCategoria", SqlDbType.Int).Value = idCategoria;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        subcategorias.Add(new Subcategoria
                        {
                            IdSubcategoria = Convert.ToInt32(
                                reader["IdSubcategoria"]),
                            IdCategoria = Convert.ToInt32(
                                reader["IdCategoria"]),
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

            return subcategorias;
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
