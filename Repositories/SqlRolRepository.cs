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
    public class SqlRolRepository : IRolRepository
    {
        private readonly string connectionString;

        public SqlRolRepository()
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

        public async Task<IList<Rol>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var roles = new List<Rol>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT IdRol, Nombre, Descripcion, Activo, FechaRegistro "
                + "FROM dbo.CIRI_Roles "
                + "ORDER BY IdRol", connection))
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
                        roles.Add(new Rol
                        {
                            IdRol = Convert.ToInt32(reader["IdRol"]),
                            Nombre = LeerTexto(reader, "Nombre"),
                            Descripcion = LeerTexto(reader, "Descripcion"),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaRegistro = Convert.ToDateTime(
                                reader["FechaRegistro"])
                        });
                    }
                }
            }

            return roles;
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
