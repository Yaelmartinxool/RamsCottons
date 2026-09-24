using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Data;
using System.Security.Claims;

namespace RamsCottons.Services
{
    public class PermisoService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermisoService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            RoleManager<IdentityRole> roleManager)
        {
            _contextFactory = contextFactory;
            _roleManager = roleManager;
        }

        // =========================================================
        // VERIFICAR SI EL USUARIO TIENE UN PERMISO
        // =========================================================
        public async Task<bool> TienePermisoAsync(
            ClaimsPrincipal principal,
            string permisoNombre)
        {
            if (principal.Identity?.IsAuthenticated != true)
                return false;

            // SuperAdministrador tiene acceso total
            if (principal.IsInRole("SuperAdministrador"))
                return true;

            var rolesUsuario = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToList();

            if (!rolesUsuario.Any())
                return false;

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var roleIds = await context.Roles
                .Where(r =>
                    r.Name != null &&
                    rolesUsuario.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            if (!roleIds.Any())
                return false;

            return await context.RolesPermisos
                .Include(rp => rp.Permiso)
                .AnyAsync(rp =>
                    roleIds.Contains(rp.RoleId) &&
                    rp.Permiso != null &&
                    rp.Permiso.Activo &&
                    rp.Permiso.Nombre == permisoNombre);
        }


        // =========================================================
        // OBTENER TODOS LOS PERMISOS DEL USUARIO
        // =========================================================
        public async Task<List<string>> ObtenerPermisosAsync(
            ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
                return new List<string>();

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            // SuperAdministrador obtiene todos los permisos
            if (principal.IsInRole("SuperAdministrador"))
            {
                return await context.Permisos
                    .Where(p => p.Activo)
                    .Select(p => p.Nombre)
                    .ToListAsync();
            }

            var rolesUsuario = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToList();

            if (!rolesUsuario.Any())
                return new List<string>();

            var roleIds = await context.Roles
                .Where(r =>
                    r.Name != null &&
                    rolesUsuario.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            if (!roleIds.Any())
                return new List<string>();

            return await context.RolesPermisos
                .Include(rp => rp.Permiso)
                .Where(rp =>
                    roleIds.Contains(rp.RoleId) &&
                    rp.Permiso != null &&
                    rp.Permiso.Activo)
                .Select(rp => rp.Permiso!.Nombre)
                .Distinct()
                .ToListAsync();
        }


        // =========================================================
        // ASIGNAR PERMISOS PREDEFINIDOS A UN ROL
        // =========================================================
        public async Task AsignarPermisosPorRol(string roleName)
        {
            var permisosPorRol = new Dictionary<string, List<string>>
            {
                ["Gerente"] = new()
                {
                    Permissions.Categorias_Ver,
                    Permissions.Clientes_Ver,
                    Permissions.Clientes_VerTodasSucursales,
                    Permissions.Promociones_Ver,
                    Permissions.Promociones_VerTodasSucursales,
                    Permissions.Reportes_Exportar,
                    Permissions.Reportes_VerClientes,
                    Permissions.Reportes_VerEnvios,
                    Permissions.Reportes_VerPromociones,
                    Permissions.Reportes_VerPorSucursal,
                    Permissions.Sucursales_Ver,
                    Permissions.Usuarios_Ver,
                    Permissions.WhatsApp_Ver,
                    Permissions.WhatsApp_VerTodasSucursales
                },

                ["Vendedor"] = new()
                {
                    Permissions.Categorias_Ver,
                    Permissions.Clientes_AsignarCategoria,
                    Permissions.Clientes_Crear,
                    Permissions.Clientes_Editar,
                    Permissions.Clientes_GenerarQR,
                    Permissions.Clientes_Ver,
                    Permissions.Promociones_Ver,
                    Permissions.WhatsApp_Crear,
                    Permissions.WhatsApp_Ver
                }
            };

            if (!permisosPorRol.TryGetValue(roleName, out var permisosNombres))
                return;

            var rol = await _roleManager.FindByNameAsync(roleName);

            if (rol == null)
                return;

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var permisosIds = await context.Permisos
                .Where(p =>
                    p.Activo &&
                    permisosNombres.Contains(p.Nombre))
                .Select(p => p.Id)
                .ToListAsync();

            var permisosExistentes = await context.RolesPermisos
                .Where(rp => rp.RoleId == rol.Id)
                .Select(rp => rp.PermisoId)
                .ToListAsync();

            foreach (var permisoId in permisosIds)
            {
                if (!permisosExistentes.Contains(permisoId))
                {
                    context.RolesPermisos.Add(new RolPermiso
                    {
                        RoleId = rol.Id,
                        PermisoId = permisoId,
                        FechaAsignacion = DateTime.Now
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}