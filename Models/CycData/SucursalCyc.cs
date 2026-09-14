using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RamsCottons.Models.CycData
{
    [Table("sucursales")]
    public class SucursalCyc
    {
        [Key]
        [Column("Almacen")]
        public string Almacen { get; set; } = string.Empty;

        [Column("Nombre")]
        public string? Nombre { get; set; }

        [Column("Ubicacion")]
        public string? Ubicacion { get; set; }

        [Column("Responsable")]
        public string? Responsable { get; set; }

        [Column("Region")]
        public string? Region { get; set; }

        [Column("Serie")]
        public string? Serie { get; set; }

        [Column("Folio")]
        public double? Folio { get; set; }

        [Column("Rasonsocial")]
        public string? RasonSocial { get; set; }

        [Column("RFC")]
        public string? RFC { get; set; }

        [Column("RegimenFiscal")]
        public string? RegimenFiscal { get; set; }

        [Column("DireccionFiscal")]
        public string? DireccionFiscal { get; set; }

        [Column("CiudadEstado")]
        public string? CiudadEstado { get; set; }

        [Column("DireccionSucursal")]
        public string? DireccionSucursal { get; set; }

        [Column("CiudadEstadoSuc")]
        public string? CiudadEstadoSuc { get; set; }

        [Column("Telefonos")]
        public string? Telefonos { get; set; }

        [Column("Dctoglobal")]
        public double? DctoGlobal { get; set; }

        [Column("folio2")]
        public long? Folio2 { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================
        
        [NotMapped]
        public int IdSucursal 
        { 
            get 
            {
                if (int.TryParse(Almacen, out int id))
                    return id;
                return 0;
            }
        }

        [NotMapped]
        public string Ciudad 
        { 
            get 
            {
                // PRIMERO: intentar con CiudadEstadoSuc (tiene la ciudad real)
                if (!string.IsNullOrEmpty(CiudadEstadoSuc))
                {
                    var partes = CiudadEstadoSuc.Split(',');
                    return partes.Length > 0 ? partes[0].Trim() : "Sin ciudad";
                }
                
                // SEGUNDO: si no tiene, usar CiudadEstado
                if (!string.IsNullOrEmpty(CiudadEstado))
                {
                    var partes = CiudadEstado.Split(',');
                    return partes.Length > 0 ? partes[0].Trim() : "Sin ciudad";
                }
                
                return "Sin ciudad";
            }
        }

        [NotMapped]
        public string Estado 
        { 
            get 
            {
                // PRIMERO: intentar con CiudadEstadoSuc
                if (!string.IsNullOrEmpty(CiudadEstadoSuc))
                {
                    var partes = CiudadEstadoSuc.Split(',');
                    return partes.Length > 1 ? partes[1].Trim() : "Sin estado";
                }
                
                // SEGUNDO: usar CiudadEstado
                if (!string.IsNullOrEmpty(CiudadEstado))
                {
                    var partes = CiudadEstado.Split(',');
                    return partes.Length > 1 ? partes[1].Trim() : "Sin estado";
                }
                
                return "Sin estado";
            }
        }

        [NotMapped]
        public string NombreCompleto => $"{Almacen} - {Nombre} ({CiudadEstadoSuc})";

        [NotMapped]
        public string NombreCompletoConCiudad => $"{Ciudad} - {Nombre}";
    }
}