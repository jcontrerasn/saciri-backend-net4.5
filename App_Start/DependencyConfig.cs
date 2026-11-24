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
            return new UsuarioRolService(
                new SqlUsuarioRolRepository(),
                new SqlPuestoRepository());
        }

        public static RolService CreateRolService()
        {
            return new RolService(new SqlRolRepository());
        }

        public static ComentarioSolicitudService CreateComentarioSolicitudService()
        {
            return new ComentarioSolicitudService(
                new SqlComentarioSolicitudRepository());
        }

        public static EstadoSolicitudService CreateEstadoSolicitudService()
        {
            return new EstadoSolicitudService(
                new SqlEstadoSolicitudRepository());
        }

        public static DetalleSolicitudService CreateDetalleSolicitudService()
        {
            return new DetalleSolicitudService(
                new SqlDetalleSolicitudRepository());
        }

        public static HistorialSolicitudService CreateHistorialSolicitudService()
        {
            return new HistorialSolicitudService(
                new SqlHistorialSolicitudRepository());
        }

        public static CatSubOcuService CreateCatSubOcuService()
        {
            return new CatSubOcuService(new SqlCatSubOcuRepository());
        }

        public static ConexionUsuarioService CreateConexionUsuarioService()
        {
            return new ConexionUsuarioService(
                new SqlConexionUsuarioRepository());
        }

        public static HistorialConexionService CreateHistorialConexionService()
        {
            return new HistorialConexionService(
                new SqlHistorialConexionRepository());
        }

        public static PuestoService CreatePuestoService()
        {
            return new PuestoService(new SqlPuestoRepository());
        }

        public static AdjuntoSolicitudService CreateAdjuntoSolicitudService()
        {
            return new AdjuntoSolicitudService(
                new SqlAdjuntoSolicitudRepository());
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
