using Microsoft.AspNetCore.Identity;
using RamsCottons.Data;
using System.Security.Claims;

namespace RamsCottons.Services
{
    public class UserSucursalService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSucursalService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string?> GetUserSucursalAsync(ClaimsPrincipal user)
        {
            if (user == null) return null;

            if (user.IsInRole("SuperAdministrador"))
                return null;

            var appUser = await _userManager.GetUserAsync(user);
            return appUser?.IdSucursal;
        }

        public async Task<bool> UserCanAccessSucursalAsync(ClaimsPrincipal user, string almacen)
        {
            var userSucursal = await GetUserSucursalAsync(user);
            
            if (string.IsNullOrEmpty(userSucursal)) return true;

            return userSucursal == almacen;
        }
    }
}