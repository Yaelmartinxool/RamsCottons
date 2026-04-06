using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models
{
    [Table("sucursales")]
    public class Sucursal
    {
        [Key]
        [Column("id_sucursal")]
        public int IdSucursal { get; set; }
        
        [Column("nombre_sucursal")]
        public string NombreSucursal { get; set; } = string.Empty;
        
        [Column("direccion")]
        public string? Direccion { get; set; }
        
        [Column("telefono")]
        public string? Telefono { get; set; }
        
        [Column("estado")]
        public int Estado { get; set; } = 1;
        
        [Column("regtimestamp")]
        public DateTime RegTimestamp { get; set; } = DateTime.Now;
    }
}