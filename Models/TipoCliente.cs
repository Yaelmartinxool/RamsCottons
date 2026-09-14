using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models
{
    [Table("TiposCliente")]
    public class TipoCliente
    {
        [Key]
        [Column("id_tipo_cliente")]
        public int IdTipoCliente { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; }
    }
}