using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Data;
using RamsCottons.Models;

namespace RamsCottons.Services
{
    public class PermisoService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermisoService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        private readonly Dictionary<string, List<string>> PermisosPorRol = new()
        {
            ["Gerente"] = new List<string>
            {
                "Ver Categorías",
                "Exportar Clientes",
                "Ver Clientes",
                "Ver Clientes de Todas las Sucursales",
                "Ver Promociones",
                "Ver Promociones de Todas las Sucursales",
                "Exportar Reportes",
                "Ver Reportes de Clientes",
                "Ver Reportes de Envíos",
                "Ver Reportes de Promociones",
                "Ver Reportes por Sucursal",
                "Ver Sucursales",
                "Ver Usuarios",
                "Ver Envíos",
                "Ver Envíos de Todas las Sucursales",
                "Ver Reportes de Envíos"
            },
            
            ["Vendedor"] = new List<string>
            {
                "Ver Categorías",
                "Asignar Categoría a Cliente",
                "Crear Clientes",
                "Editar Clientes",
                "Generar QR para Cliente",
                "Ver Clientes",
                "Ver Promociones",
                "Crear Envíos",
                "Ver Envíos"
            }
        };

        public async Task AsignarPermisosPorRol(string roleName)
        {
            if (!PermisosPorRol.ContainsKey(roleName)) return;

            var rol = await _roleManager.FindByNameAsync(roleName);
            if (rol == null) return;

            var permisosANombre = PermisosPorRol[roleName];
            
            var permisosIds = await _context.Permisos
                .Where(p => permisosANombre.Contains(p.Nombre))
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var permisoId in permisosIds)
            {
                var existe = await _context.RolesPermisos
                    .AnyAsync(rp => rp.RoleId == rol.Id && rp.PermisoId == permisoId);

                if (!existe)
                {
                    _context.RolesPermisos.Add(new RolPermiso
                    {
                        RoleId = rol.Id,
                        PermisoId = permisoId,
                        FechaAsignacion = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}