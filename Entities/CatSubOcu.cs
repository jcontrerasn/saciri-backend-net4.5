namespace SACIRI_backend.Entities
{
    public class CatSubOcu
    {
        public int Id { get; set; }
        public int IdTipoRegistro { get; set; }
        public int IdCategoria { get; set; }
        public int IdSubcategoria { get; set; }
        public int IdOcurrencia { get; set; }
        public string TipoRegistro { get; set; }
        public string Categoria { get; set; }
        public string Subcategoria { get; set; }
        public string Ocurrencia { get; set; }
        public bool RequiereAprobacion { get; set; }
        public string NivelAprobacion { get; set; }
        public string RolAprobacion { get; set; }
        public int? TiempoAtencion { get; set; }
        public bool Estado { get; set; }
        public int? Atencion { get; set; }
        public int? Nivel1 { get; set; }
        public int? Nivel2 { get; set; }
        public int? Nivel3 { get; set; }
        public string Copia { get; set; }
        public string DescripcionSubCatOcu { get; set; }
        public bool DatosAdjuntos { get; set; }
        public bool CopiaJefatura { get; set; }
    }
}
