using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RamsCottons.Data;

namespace RamsCottons.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id_folio")]
        public int Id { get; set; }
        
        [Required]
        [Column("id_usuario")]
        [StringLength(450)]  // <--- AGREGAR ESTO
        public string IdUsuario { get; set; } = string.Empty;
        
        [Required]
        [Column("id_sucursal")]
        public int IdSucursal { get; set; }
        
        [Required]
        [Column("nombre_completo")]
        [StringLength(180)]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Column("telefono")]
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [Column("correo")]
        [StringLength(150)]
        public string? Correo { get; set; }
        
        [Column("regtimestamp")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        [Column("estado")]
        public int Estado { get; set; } = 1;
        
        [Column("id_categoriaProducto")]
        public int? IdCategoriaProducto { get; set; }
        
        [Column("id_generador_qr")]
        public int? IdGeneradorQR { get; set; }
        
        // ✅ PROPIEDADES DE NAVEGACIÓN (BIEN CONFIGURADAS)
        [ForeignKey("IdUsuario")]
        public virtual ApplicationUser? Usuario { get; set; }
        
        [ForeignKey("IdSucursal")]
        public virtual Sucursal? Sucursal { get; set; }
        
        [ForeignKey("IdCategoriaProducto")]
        public virtual CategoriaProducto? Categoria { get; set; }
        
        [ForeignKey("IdGeneradorQR")]
        public virtual GeneradorQR? GeneradorQR { get; set; }
    }
}