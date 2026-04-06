using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Models;
using RamsCottons.Data;

namespace RamsCottons.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
      
        // TABLAS EXISTENTES
        public DbSet<Promocion> Promociones => Set<Promocion>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<HistorialEnvio> HistorialEnvios => Set<HistorialEnvio>();
        
        // TABLAS DE PERMISOS
        public DbSet<Permiso> Permisos => Set<Permiso>();
        public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
        
        // ✅ NUEVA TABLA: CATEGORÍAS DE PRODUCTOS
        public DbSet<CategoriaProducto> CategoriaProductos => Set<CategoriaProducto>();
        
        // ✅ NUEVA TABLA: SUCURSALES
        public DbSet<Sucursal> Sucursales => Set<Sucursal>();
        
        // ✅ NUEVA TABLA: GENERADOR QR
        public DbSet<GeneradorQR> GeneradorQR => Set<GeneradorQR>();
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // --- CONFIGURACIONES EXISTENTES ---
            builder.Entity<Cliente>()
                .HasIndex(c => c.Telefono)
                .IsUnique();
            
            builder.Entity<Promocion>()
                .HasIndex(p => p.TokenUnico)
                .IsUnique()
                .HasFilter("[TokenUnico] IS NOT NULL");
            
            builder.Entity<Promocion>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);
            
            builder.Entity<Cliente>()
                .Property(c => c.Telefono)
                .HasMaxLength(20);
            
            // --- CONFIGURACIONES DE PERMISOS ---
            builder.Entity<Permiso>()
                .HasIndex(p => p.Nombre)
                .IsUnique();
            
            builder.Entity<Permiso>()
                .Property(p => p.Nombre)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Entity<RolPermiso>()
                .HasKey(rp => rp.Id);
            
            builder.Entity<RolPermiso>()
                .HasIndex(rp => new { rp.RoleId, rp.PermisoId })
                .IsUnique();
            
            builder.Entity<RolPermiso>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany()
                .HasForeignKey(rp => rp.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // --- ✅ CONFIGURACIONES DE CATEGORÍAS ---
            builder.Entity<CategoriaProducto>()
                .ToTable("categoria_productos");
            
            builder.Entity<CategoriaProducto>()
                .HasKey(c => c.IdCategoria);
            
            builder.Entity<CategoriaProducto>()
                .Property(c => c.IdCategoria)
                .HasColumnName("id_categoria")
                .ValueGeneratedOnAdd();
            
            builder.Entity<CategoriaProducto>()
                .Property(c => c.NombreCat)
                .HasColumnName("nombreCat")
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Entity<CategoriaProducto>()
                .Property(c => c.Estado)
                .HasColumnName("estado")
                .HasDefaultValue(1);
            
            // --- ✅ CONFIGURACIONES DE SUCURSALES ---
            builder.Entity<Sucursal>()
                .ToTable("sucursales");
            
            builder.Entity<Sucursal>()
                .HasKey(s => s.IdSucursal);
            
            builder.Entity<Sucursal>()
                .Property(s => s.IdSucursal)
                .HasColumnName("id_sucursal")
                .ValueGeneratedOnAdd();
            
            builder.Entity<Sucursal>()
                .Property(s => s.NombreSucursal)
                .HasColumnName("nombre_sucursal")
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Entity<Sucursal>()
                .Property(s => s.Direccion)
                .HasColumnName("direccion")
                .HasMaxLength(255);
            
            builder.Entity<Sucursal>()
                .Property(s => s.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(20);
            
            builder.Entity<Sucursal>()
                .Property(s => s.Estado)
                .HasColumnName("estado")
                .HasDefaultValue(1);
            
            builder.Entity<Sucursal>()
                .Property(s => s.RegTimestamp)
                .HasColumnName("regtimestamp")
                .HasDefaultValueSql("GETDATE()");
            
            // --- ✅ CONFIGURACIONES DE GENERADOR QR ---
            builder.Entity<GeneradorQR>()
                .ToTable("generador_qr");
            
            builder.Entity<GeneradorQR>()
                .HasKey(g => g.IdGeneradorQR);
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.IdGeneradorQR)
                .HasColumnName("id_generador_qr")
                .ValueGeneratedOnAdd();
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.ClienteId)
                .HasColumnName("cliente_id")
                .IsRequired();
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.PromocionId)
                .HasColumnName("promocion_id");
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.CodigoQR)
                .HasColumnName("codigo_qr")
                .HasColumnType("TEXT");
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.ImagenQRUrl)
                .HasColumnName("imagen_qr_url")
                .HasMaxLength(500);
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.FolioCliente)
                .HasColumnName("folio_cliente")
                .HasMaxLength(50);
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.DatosQR)
                .HasColumnName("datos_qr")
                .HasColumnType("TEXT");
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.FechaGeneracion)
                .HasColumnName("fecha_generacion")
                .HasDefaultValueSql("GETDATE()");
            
            builder.Entity<GeneradorQR>()
                .Property(g => g.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(1);
            
            // --- RELACIONES ---
            builder.Entity<GeneradorQR>()
                .HasOne(g => g.Cliente)
                .WithMany()
                .HasForeignKey(g => g.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<GeneradorQR>()
                .HasOne(g => g.Promocion)
                .WithMany()
                .HasForeignKey(g => g.PromocionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Cliente>()
                .HasOne(c => c.Sucursal)
                .WithMany()
                .HasForeignKey(c => c.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Cliente>()
                .HasOne(c => c.Categoria)
                .WithMany()
                .HasForeignKey(c => c.IdCategoriaProducto)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}