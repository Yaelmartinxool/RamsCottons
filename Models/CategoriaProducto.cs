using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models
{
    [Table("categoria_productos")]
    public class CategoriaProducto
    {
        [Key]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }
        
        [Column("nombreCat")]
        public string NombreCat { get; set; } = string.Empty;
        
        [Column("estado")]
        public int Estado { get; set; } = 1;
    }
}