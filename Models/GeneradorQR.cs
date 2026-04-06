using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models
{
    [Table("generador_qr")]
    public class GeneradorQR
    {
        [Key]
        [Column("id_generador_qr")]
        public int IdGeneradorQR { get; set; }
        
        [Column("cliente_id")]
        public int ClienteId { get; set; }
        
        [Column("promocion_id")]
        public int? PromocionId { get; set; }
        
        [Column("codigo_qr")]
        public string CodigoQR { get; set; } = string.Empty;
        
        [Column("imagen_qr_url")]
        public string? ImagenQRUrl { get; set; }
        
        [Column("folio_cliente")]
        public string? FolioCliente { get; set; }
        
        [Column("datos_qr")]
        public string? DatosQR { get; set; }
        
        [Column("fecha_generacion")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        
        [Column("activo")]
        public int Activo { get; set; } = 1;
        
        // Propiedades de navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }
        
        [ForeignKey("PromocionId")]
        public virtual Promocion? Promocion { get; set; }
    }
}