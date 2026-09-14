using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RamsCottons.Data;

namespace RamsCottons.Models
{
    [Table("Promociones")]
    public class Promocion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Precio { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DescuentoPorcentaje { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DescuentoMonto { get; set; }

        [Required]
        [StringLength(30)]
        public string TipoPromocion { get; set; } = "PrecioUnico";

        public string? ImagenUrl { get; set; }

        public string? ImagenNombre { get; set; }

        [Required]
        [Column("Fechalnicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaFin { get; set; } = DateTime.Now.AddDays(30);

        public string? TokenUnico { get; set; }

        public bool Activo { get; set; } = true;

        [Column("UsuarioId")]
        public string? UsuarioId { get; set; }

        public string? ApplicationUserId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }

        public string? pdf_nombre { get; set; }

        public string? pdf_ruta { get; set; }

        public string? pdf_url { get; set; }

        public string? IdSucursal { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}