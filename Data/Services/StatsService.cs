using Microsoft.EntityFrameworkCore;
using RamsCottons.Data;

namespace RamsCottons.Services
{
    public class StatsService
    {
        private readonly ApplicationDbContext _context;
        private static StatsCache _cache = new();
        private static DateTime _lastUpdate = DateTime.MinValue;
        private static readonly object _lock = new();

        public StatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StatsCache> GetStats()
        {
            // Actualizar caché cada 5 minutos
            if ((DateTime.Now - _lastUpdate).TotalMinutes < 5)
            {
                return _cache;
            }

            lock (_lock)
            {
                if ((DateTime.Now - _lastUpdate).TotalMinutes < 5)
                {
                    return _cache;
                }

                _cache = new StatsCache
                {
                    TotalClientes = _context.Clientes.Count(),
                    TotalEnvios = _context.HistorialEnvios.Count(),
                    ClientesHoy = _context.Clientes.Count(c => c.FechaRegistro.Date == DateTime.Today),
                    EnviosHoy = _context.HistorialEnvios.Count(e => e.FechaEnvio.Date == DateTime.Today),
                    TotalUsuarios = _context.Users.Count(),
                    PromocionesActivas = _context.Promociones.Count(p => p.Activo)
                };
                _lastUpdate = DateTime.Now;
            }

            return _cache;
        }
    }

    public class StatsCache
    {
        public int TotalClientes { get; set; }
        public int TotalEnvios { get; set; }
        public int ClientesHoy { get; set; }
        public int EnviosHoy { get; set; }
        public int TotalUsuarios { get; set; }
        public int PromocionesActivas { get; set; }
    }
}