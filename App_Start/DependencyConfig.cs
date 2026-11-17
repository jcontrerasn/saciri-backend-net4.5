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

        public static UsuarioRolService CreateUsuarioRolService()
        {
            return new UsuarioRolService(new SqlUsuarioRolRepository());
        }

        public static RolService CreateRolService()
        {
            return new RolService(new SqlRolRepository());
        }

        public static SolicitudService CreateSolicitudService()
        {
            return new SolicitudService(
                new SqlSolicitudRepository(),
                new SqlSolicitudPorCodigoRepository());
        }

        public static TipoRegistroService CreateTipoRegistroService()
        {
            return new TipoRegistroService(new SqlTipoRegistroRepository());
        }

        public static CategoriaService CreateCategoriaService()
        {
            return new CategoriaService(new SqlCategoriaRepository());
        }

        public static SubcategoriaService CreateSubcategoriaService()
        {
            return new SubcategoriaService(new SqlSubcategoriaRepository());
        }

        public static OcurrenciaService CreateOcurrenciaService()
        {
            return new OcurrenciaService(new SqlOcurrenciaRepository());
        }

        public static CentroCostoService CreateCentroCostoService()
        {
            return new CentroCostoService(new SqlCentroCostoRepository());
        }
    }
}
