using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models
{
    public class HistorialEnvio
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PromocionId { get; set; }
        
        [Required]
        public int ClienteId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UsuarioEnvioId { get; set; } = string.Empty;
        
        public string? Mensaje { get; set; }
        public string EnlacePromocion { get; set; } = string.Empty;
        public string? EnlaceWhatsApp { get; set; }
        
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Estatus { get; set; } = "Enviado";
        
        public string? Observaciones { get; set; }
    }
}