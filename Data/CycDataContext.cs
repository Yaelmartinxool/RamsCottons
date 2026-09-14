using Microsoft.EntityFrameworkCore;
using RamsCottons.Models.CycData;

namespace RamsCottons.Data
{
    public class CycDataContext : DbContext
    {
        public CycDataContext(DbContextOptions<CycDataContext> options)
            : base(options)
        {
        }

        public DbSet<SucursalCyc> Sucursales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SucursalCyc>(entity =>
            {
                entity.ToTable("sucursales");
                entity.HasKey(e => e.Almacen);
                
                entity.Property(e => e.Almacen).HasColumnName("Almacen");
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.Ubicacion).HasColumnName("Ubicacion");
                entity.Property(e => e.Responsable).HasColumnName("Responsable");
                entity.Property(e => e.Region).HasColumnName("Region");
                entity.Property(e => e.Serie).HasColumnName("Serie");
                entity.Property(e => e.Folio).HasColumnName("Folio");
                entity.Property(e => e.RasonSocial).HasColumnName("Rasonsocial");
                entity.Property(e => e.RFC).HasColumnName("RFC");
                entity.Property(e => e.RegimenFiscal).HasColumnName("RegimenFiscal");
                entity.Property(e => e.DireccionFiscal).HasColumnName("DireccionFiscal");
                entity.Property(e => e.CiudadEstado).HasColumnName("CiudadEstado");
                entity.Property(e => e.DireccionSucursal).HasColumnName("DireccionSucursal");
                entity.Property(e => e.CiudadEstadoSuc).HasColumnName("CiudadEstadoSuc");
                entity.Property(e => e.Telefonos).HasColumnName("Telefonos");
                entity.Property(e => e.DctoGlobal).HasColumnName("Dctoglobal");
                entity.Property(e => e.Folio2).HasColumnName("folio2");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}