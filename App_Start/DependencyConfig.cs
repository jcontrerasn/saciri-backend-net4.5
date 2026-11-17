using SACIRI_backend.Repositories;
using SACIRI_backend.Services;

namespace SACIRI_backend
{
    public static class DependencyConfig
    {
        public static UsuarioService CreateUsuarioService()
        {
            return new UsuarioService(new SqlUsuarioRepository());
        }

        public static SolicitudService CreateSolicitudService()
        {
            return new SolicitudService(new SqlSolicitudRepository());
        }
    }
}
