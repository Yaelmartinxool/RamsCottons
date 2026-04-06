using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;  // ← AGREGAR ESTO PARA IdentityRole

namespace RamsCottons.Data
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
        
        [StringLength(100)]
        public string? Modulo { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
    
    public class RolPermiso
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string RoleId { get; set; } = string.Empty;
        
        [Required]
        public int PermisoId { get; set; }
        
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;
        
        // Propiedades de navegación
        public virtual IdentityRole? Role { get; set; }
        public virtual Permiso? Permiso { get; set; }
    }
}