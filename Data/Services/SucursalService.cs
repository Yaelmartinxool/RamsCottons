using Microsoft.EntityFrameworkCore;
using RamsCottons.Data;
using RamsCottons.Models.CycData;

namespace RamsCottons.Services
{
    public class SucursalService
    {
        private readonly CycDataContext _cycContext;
        private List<SucursalCyc>? _sucursalesCache;
        private readonly object _lock = new object();

        public SucursalService(CycDataContext cycContext)
        {
            _cycContext = cycContext;
        }

        public List<SucursalCyc> GetAllSucursales()
        {
            if (_sucursalesCache == null)
            {
                lock (_lock)
                {
                    if (_sucursalesCache == null)
                    {
                        _sucursalesCache = _cycContext.Sucursales
                            .AsNoTracking()
                            .Where(s => s.Activo == "S")
                            .OrderBy(s => s.CiudadEstado)
                            .ThenBy(s => s.Nombre)
                            .ToList();
                    }
                }
            }

            return _sucursalesCache;
        }

        public async Task<SucursalCyc?> GetSucursalByAlmacenAsync(string almacen)
        {
            return await _cycContext.Sucursales
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Almacen == almacen &&
                    s.Activo == "S");
        }

        public async Task<List<SucursalCyc>> GetSucursalesByCiudadAsync(string ciudad)
        {
            return await _cycContext.Sucursales
                .AsNoTracking()
                .Where(s =>
                    s.Activo == "S" &&
                    s.CiudadEstado != null &&
                    s.CiudadEstado.Contains(ciudad))
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> GetCiudadesAsync()
        {
            var sucursales = await _cycContext.Sucursales
                .AsNoTracking()
                .Where(s =>
                    s.Activo == "S" &&
                    s.CiudadEstado != null)
                .ToListAsync();

            return sucursales
                .Select(s => s.Ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public void LimpiarCache()
        {
            lock (_lock)
            {
                _sucursalesCache = null;
            }
        }
    }
}
