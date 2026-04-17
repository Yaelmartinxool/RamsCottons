using Microsoft.AspNetCore.Identity;
using RamsCottons.Models;

namespace RamsCottons.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;
        public int? IdSucursal { get; set; }
        // Relaciones
        public virtual ICollection<Cliente>? ClientesAsignados { get; set; }
        public virtual ICollection<Promocion>? PromocionesCreadas { get; set; }
    }
}