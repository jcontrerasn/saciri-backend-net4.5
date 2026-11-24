using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class CrearCatSubOcuRequest
    {
        [Range(1, int.MaxValue)]
        public int IdTipoRegistro { get; set; }

        [Range(1, int.MaxValue)]
        public int IdCategoria { get; set; }

        [Range(1, int.MaxValue)]
        public int IdSubcategoria { get; set; }

        [Range(1, int.MaxValue)]
        public int IdOcurrencia { get; set; }

        public bool RequiereAprobacion { get; set; }

        [StringLength(5)]
        public string NivelAprobacion { get; set; }

        [StringLength(10)]
        public string RolAprobacion { get; set; }

        public int? TiempoAtencion { get; set; }
        public bool Estado { get; set; }
        public int? Atencion { get; set; }
        public int? Nivel1 { get; set; }
        public int? Nivel2 { get; set; }
        public int? Nivel3 { get; set; }
        [StringLength(100)]
        public string Copia { get; set; }
        public string DescripcionSubCatOcu { get; set; }
        public bool DatosAdjuntos { get; set; }
        public bool CopiaJefatura { get; set; }

        public CrearCatSubOcuRequest()
        {
            Estado = true;
        }
    }
}
