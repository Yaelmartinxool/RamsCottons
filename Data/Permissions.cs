namespace RamsCottons.Data
{
    public static class Permissions
    {
        // ==================== Módulo Usuarios (7) ====================
        public const string Usuarios_Ver = "Usuarios.Ver";
        public const string Usuarios_Crear = "Usuarios.Crear";
        public const string Usuarios_Editar = "Usuarios.Editar";
        public const string Usuarios_Eliminar = "Usuarios.Eliminar";
        public const string Usuarios_ActivarDesactivar = "Usuarios.ActivarDesactivar";
        public const string Usuarios_CambiarRol = "Usuarios.CambiarRol";
        public const string Usuarios_AsignarSucursal = "Usuarios.AsignarSucursal";

        // ==================== Módulo Roles (5) ====================
        public const string Roles_Ver = "Roles.Ver";
        public const string Roles_Crear = "Roles.Crear";
        public const string Roles_Editar = "Roles.Editar";
        public const string Roles_Eliminar = "Roles.Eliminar";
        public const string Roles_AsignarPermisos = "Roles.AsignarPermisos";

        // ==================== Módulo Clientes (8) ====================
        public const string Clientes_Ver = "Clientes.Ver";
        public const string Clientes_Crear = "Clientes.Crear";
        public const string Clientes_Editar = "Clientes.Editar";
        public const string Clientes_Eliminar = "Clientes.Eliminar";
        public const string Clientes_Importar = "Clientes.Importar";
        public const string Clientes_Exportar = "Clientes.Exportar";
        public const string Clientes_VerTodasSucursales = "Clientes.VerTodasSucursales";
        public const string Clientes_AsignarCategoria = "Clientes.AsignarCategoria";
        public const string Clientes_GenerarQR = "Clientes.GenerarQR";

        // ==================== Módulo Promociones (6) ====================
        public const string Promociones_Ver = "Promociones.Ver";
        public const string Promociones_Crear = "Promociones.Crear";
        public const string Promociones_Editar = "Promociones.Editar";
        public const string Promociones_Eliminar = "Promociones.Eliminar";
        public const string Promociones_ActivarDesactivar = "Promociones.ActivarDesactivar";
        public const string Promociones_VerTodasSucursales = "Promociones.VerTodasSucursales";

        // ==================== Módulo WhatsApp (4) ====================
        public const string WhatsApp_Ver = "WhatsApp.Ver";
        public const string WhatsApp_Crear = "WhatsApp.Crear";
        public const string WhatsApp_VerTodasSucursales = "WhatsApp.VerTodasSucursales";
        public const string WhatsApp_VerReportes = "WhatsApp.VerReportes";

        // ==================== Módulo Sucursales (5) ====================
        public const string Sucursales_Ver = "Sucursales.Ver";
        public const string Sucursales_Crear = "Sucursales.Crear";
        public const string Sucursales_Editar = "Sucursales.Editar";
        public const string Sucursales_Eliminar = "Sucursales.Eliminar";
        public const string Sucursales_ActivarDesactivar = "Sucursales.ActivarDesactivar";

        // ==================== Módulo Categorías (4) ====================
        public const string Categorias_Ver = "Categorias.Ver";
        public const string Categorias_Crear = "Categorias.Crear";
        public const string Categorias_Editar = "Categorias.Editar";
        public const string Categorias_Eliminar = "Categorias.Eliminar";

        // ==================== Módulo Reportes (5) ====================
        public const string Reportes_VerClientes = "Reportes.VerClientes";
        public const string Reportes_VerPromociones = "Reportes.VerPromociones";
        public const string Reportes_VerEnvios = "Reportes.VerEnvios";
        public const string Reportes_VerPorSucursal = "Reportes.VerPorSucursal";
        public const string Reportes_Exportar = "Reportes.Exportar";

        // ==================== Módulo Configuración (4) ====================
        public const string Configuracion_Ver = "Configuracion.Ver";
        public const string Configuracion_Editar = "Configuracion.Editar";
        public const string Configuracion_VerLogs = "Configuracion.VerLogs";
        public const string Configuracion_Respaldar = "Configuracion.Respaldar";

        // ==================== Diccionario por módulo ====================
        public static Dictionary<string, List<string>> ObtenerPermisosPorModulo()
        {
            return new Dictionary<string, List<string>>
            {
                ["Usuarios"] = new List<string>
                {
                    Usuarios_Ver, Usuarios_Crear, Usuarios_Editar, Usuarios_Eliminar,
                    Usuarios_ActivarDesactivar, Usuarios_CambiarRol, Usuarios_AsignarSucursal
                },
                ["Roles"] = new List<string>
                {
                    Roles_Ver, Roles_Crear, Roles_Editar, Roles_Eliminar, Roles_AsignarPermisos
                },
                ["Clientes"] = new List<string>
                {
                    Clientes_Ver, Clientes_Crear, Clientes_Editar, Clientes_Eliminar,
                    Clientes_Importar, Clientes_Exportar, Clientes_VerTodasSucursales,
                    Clientes_AsignarCategoria, Clientes_GenerarQR
                },
                ["Promociones"] = new List<string>
                {
                    Promociones_Ver, Promociones_Crear, Promociones_Editar, Promociones_Eliminar,
                    Promociones_ActivarDesactivar, Promociones_VerTodasSucursales
                },
                ["WhatsApp"] = new List<string>
                {
                    WhatsApp_Ver, WhatsApp_Crear, WhatsApp_VerTodasSucursales, WhatsApp_VerReportes
                },
                ["Sucursales"] = new List<string>
                {
                    Sucursales_Ver, Sucursales_Crear, Sucursales_Editar, Sucursales_Eliminar,
                    Sucursales_ActivarDesactivar
                },
                ["Categorías"] = new List<string>
                {
                    Categorias_Ver, Categorias_Crear, Categorias_Editar, Categorias_Eliminar
                },
                ["Reportes"] = new List<string>
                {
                    Reportes_VerClientes, Reportes_VerPromociones, Reportes_VerEnvios,
                    Reportes_VerPorSucursal, Reportes_Exportar
                },
                ["Configuración"] = new List<string>
                {
                    Configuracion_Ver, Configuracion_Editar, Configuracion_VerLogs, Configuracion_Respaldar
                }
            };
        }

        // ==================== Descripciones ====================
        public static string GetPermissionDescription(string permission)
        {
            return permission switch
            {
                // Usuarios
                Usuarios_Ver => "Ver lista de usuarios",
                Usuarios_Crear => "Crear nuevos usuarios",
                Usuarios_Editar => "Editar usuarios existentes",
                Usuarios_Eliminar => "Eliminar usuarios",
                Usuarios_ActivarDesactivar => "Activar/Desactivar usuarios",
                Usuarios_CambiarRol => "Cambiar rol de usuarios",
                Usuarios_AsignarSucursal => "Asignar sucursal a usuario",

                // Roles
                Roles_Ver => "Ver lista de roles",
                Roles_Crear => "Crear nuevos roles",
                Roles_Editar => "Editar roles existentes",
                Roles_Eliminar => "Eliminar roles",
                Roles_AsignarPermisos => "Asignar permisos a roles",

                // Clientes
                Clientes_Ver => "Ver lista de clientes",
                Clientes_Crear => "Crear nuevos clientes",
                Clientes_Editar => "Editar clientes existentes",
                Clientes_Eliminar => "Eliminar clientes",
                Clientes_Importar => "Importar clientes desde archivo",
                Clientes_Exportar => "Exportar clientes a archivo",
                Clientes_VerTodasSucursales => "Ver clientes de todas las sucursales",
                Clientes_AsignarCategoria => "Asignar categoría a cliente",
                Clientes_GenerarQR => "Generar código QR para cliente",

                // Promociones
                Promociones_Ver => "Ver lista de promociones",
                Promociones_Crear => "Crear nuevas promociones",
                Promociones_Editar => "Editar promociones existentes",
                Promociones_Eliminar => "Eliminar promociones",
                Promociones_ActivarDesactivar => "Activar/Desactivar promociones",
                Promociones_VerTodasSucursales => "Ver promociones de todas las sucursales",

                // WhatsApp
                WhatsApp_Ver => "Ver envíos de WhatsApp",
                WhatsApp_Crear => "Crear envíos de WhatsApp",
                WhatsApp_VerTodasSucursales => "Ver envíos de todas las sucursales",
                WhatsApp_VerReportes => "Ver reportes de envíos",

                // Sucursales
                Sucursales_Ver => "Ver lista de sucursales",
                Sucursales_Crear => "Crear nuevas sucursales",
                Sucursales_Editar => "Editar sucursales existentes",
                Sucursales_Eliminar => "Eliminar sucursales",
                Sucursales_ActivarDesactivar => "Activar/Desactivar sucursales",

                // Categorías
                Categorias_Ver => "Ver lista de categorías",
                Categorias_Crear => "Crear nuevas categorías",
                Categorias_Editar => "Editar categorías existentes",
                Categorias_Eliminar => "Eliminar categorías",

                // Reportes
                Reportes_VerClientes => "Ver reportes de clientes",
                Reportes_VerPromociones => "Ver reportes de promociones",
                Reportes_VerEnvios => "Ver reportes de envíos",
                Reportes_VerPorSucursal => "Ver reportes por sucursal",
                Reportes_Exportar => "Exportar reportes",

                // Configuración
                Configuracion_Ver => "Ver configuración del sistema",
                Configuracion_Editar => "Editar configuración del sistema",
                Configuracion_VerLogs => "Ver logs del sistema",
                Configuracion_Respaldar => "Respaldar base de datos",

                _ => permission
            };
        }
    }
}