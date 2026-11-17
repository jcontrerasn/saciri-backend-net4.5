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
    public class SqlUsuarioRepository : IUsuarioRepository
    {
        private readonly string connectionString;

        public SqlUsuarioRepository()
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

        public async Task<Usuario> BuscarPorCuentaAsync(
            string cuenta,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_sm_BuscarUsuarioXCuenta", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "Pcuenta_n", SqlDbType.VarChar, 50).Value = cuenta;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SingleRow, cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        return null;
                    }

                    return new Usuario
                    {
                        Cuenta = LeerTexto(reader, "Usuario"),
                        NombreCompleto = LeerTexto(reader, "NombreCompleto"),
                        Area = LeerTexto(reader, "Area")
                    };
                }
            }
        }

        public async Task<IList<Usuario>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var usuarios = new List<Usuario>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "pr_sm_ListarUsuarioXFiltro", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;
                command.Parameters.Add(
                    "Pfiltro_c", SqlDbType.VarChar, 50).Value = filtro;

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(
                    cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                    {
                        usuarios.Add(new Usuario
                        {
                            Cuenta = LeerTexto(reader, "Usuario"),
                            NombreCompleto = LeerTexto(
                                reader, "NombreCompleto"),
                            Ubicacion = LeerTexto(reader, "Ubicación"),
                            Area = LeerTexto(reader, "Area"),
                            CentroCosto = LeerTexto(
                                reader, "CentroCosto"),
                            Telefono = LeerTexto(reader, "Telefono"),
                            Correo = LeerTexto(reader, "Correo"),
                            ContactoArea = LeerTexto(
                                reader, "ContactoArea"),
                            TelefonoContactoArea = LeerTexto(
                                reader, "TelefonoContactoArea")
                        });
                    }
                }
            }

            return usuarios;
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
