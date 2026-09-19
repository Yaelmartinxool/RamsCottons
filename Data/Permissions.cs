namespace RamsCottons.Data
{
    public static class Permissions
    {
        // INICIO
        public const string Inicio_Ver = "Ver Inicio";

        // USUARIOS
        public const string Usuarios_Ver = "Ver Usuarios";
        public const string Usuarios_Crear = "Crear Usuarios";
        public const string Usuarios_Editar = "Editar Usuarios";
        public const string Usuarios_Eliminar = "Eliminar Usuarios";
        public const string Usuarios_ActivarDesactivar = "Activar/Desactivar Usuarios";
        public const string Usuarios_CambiarRol = "Cambiar Rol de Usuarios";
        public const string Usuarios_AsignarSucursal = "Asignar Sucursal a Usuario";

        // ROLES
        public const string Roles_Ver = "Ver Roles";
        public const string Roles_Crear = "Crear Roles";
        public const string Roles_Editar = "Editar Roles";
        public const string Roles_Eliminar = "Eliminar Roles";
        public const string Roles_AsignarPermisos = "Asignar Permisos a Roles";

        // CLIENTES
        public const string Clientes_Ver = "Ver Clientes";
        public const string Clientes_Crear = "Crear Clientes";
        public const string Clientes_Editar = "Editar Clientes";
        public const string Clientes_Eliminar = "Eliminar Clientes";
        public const string Clientes_Exportar = "Exportar Clientes";
        public const string Clientes_VerTodasSucursales = "Ver Clientes de Todas las Sucursales";
        public const string Clientes_AsignarCategoria = "Asignar Categoría a Cliente";
        public const string Clientes_GenerarQR = "Generar QR para Cliente";

        // PROMOCIONES
        public const string Promociones_Ver = "Ver Promociones";
        public const string Promociones_Crear = "Crear Promociones";
        public const string Promociones_Editar = "Editar Promociones";
        public const string Promociones_Eliminar = "Eliminar Promociones";
        public const string Promociones_ActivarDesactivar = "Activar/Desactivar Promociones";
        public const string Promociones_VerTodasSucursales = "Ver Promociones de Todas las Sucursales";

        // WHATSAPP
        public const string WhatsApp_Ver = "Ver Envíos";
        public const string WhatsApp_Crear = "Crear Envíos";
        public const string WhatsApp_VerTodasSucursales = "Ver Envíos de Todas las Sucursales";
        public const string WhatsApp_VerReportes = "Ver Reportes de Envíos";

        // SUCURSALES
        public const string Sucursales_Ver = "Ver Sucursales";
        public const string Sucursales_Crear = "Crear Sucursales";
        public const string Sucursales_Editar = "Editar Sucursales";
        public const string Sucursales_Eliminar = "Eliminar Sucursales";
        public const string Sucursales_ActivarDesactivar = "Activar/Desactivar Sucursales";

        // CATEGORÍAS
        public const string Categorias_Ver = "Ver Categorías";
        public const string Categorias_Crear = "Crear Categorías";
        public const string Categorias_Editar = "Editar Categorías";
        public const string Categorias_Eliminar = "Eliminar Categorías";

        // REPORTES
        public const string Reportes_VerClientes = "Ver Reportes de Clientes";
        public const string Reportes_VerPromociones = "Ver Reportes de Promociones";
        public const string Reportes_VerEnvios = "Ver Reportes de Envíos";
        public const string Reportes_VerPorSucursal = "Ver Reportes por Sucursal";
        public const string Reportes_Exportar = "Exportar Reportes";

        // CONFIGURACIÓN
        public const string Configuracion_Ver = "Ver Configuración";
        public const string Configuracion_Editar = "Editar Configuración";
        public const string Configuracion_VerLogs = "Ver Logs del Sistema";
        public const string Configuracion_Respaldar = "Respaldar Base de Datos";

        public static Dictionary<string, List<string>> ObtenerPermisosPorModulo()
        {
            return new Dictionary<string, List<string>>
            {
                ["Inicio"] = new() { Inicio_Ver },

                ["Usuarios"] = new()
                {
                    Usuarios_Ver, Usuarios_Crear, Usuarios_Editar,
                    Usuarios_Eliminar, Usuarios_ActivarDesactivar,
                    Usuarios_CambiarRol, Usuarios_AsignarSucursal
                },

                ["Roles"] = new()
                {
                    Roles_Ver, Roles_Crear, Roles_Editar,
                    Roles_Eliminar, Roles_AsignarPermisos
                },

                ["Clientes"] = new()
                {
                    Clientes_Ver, Clientes_Crear, Clientes_Editar,
                    Clientes_Eliminar,Clientes_Exportar,
                    Clientes_VerTodasSucursales, Clientes_AsignarCategoria,
                    Clientes_GenerarQR
                },

                ["Promociones"] = new()
                {
                    Promociones_Ver, Promociones_Crear, Promociones_Editar,
                    Promociones_Eliminar, Promociones_ActivarDesactivar,
                    Promociones_VerTodasSucursales
                },

                ["WhatsApp"] = new()
                {
                    WhatsApp_Ver, WhatsApp_Crear,
                    WhatsApp_VerTodasSucursales, WhatsApp_VerReportes
                },

                ["Sucursales"] = new()
                {
                    Sucursales_Ver, Sucursales_Crear, Sucursales_Editar,
                    Sucursales_Eliminar, Sucursales_ActivarDesactivar
                },

                ["Categorías"] = new()
                {
                    Categorias_Ver, Categorias_Crear,
                    Categorias_Editar, Categorias_Eliminar
                },

                ["Reportes"] = new()
                {
                    Reportes_VerClientes, Reportes_VerPromociones,
                    Reportes_VerEnvios, Reportes_VerPorSucursal,
                    Reportes_Exportar
                },

                ["Configuración"] = new()
                {
                    Configuracion_Ver, Configuracion_Editar,
                    Configuracion_VerLogs, Configuracion_Respaldar
                }
            };
        }

        public static string GetPermissionDescription(string permission)
        {
            return permission switch
            {
                Inicio_Ver => "Ver página de inicio",

                Usuarios_Ver => "Ver lista de usuarios",
                Usuarios_Crear => "Crear nuevos usuarios",
                Usuarios_Editar => "Editar usuarios existentes",
                Usuarios_Eliminar => "Eliminar usuarios",
                Usuarios_ActivarDesactivar => "Activar o desactivar usuarios",
                Usuarios_CambiarRol => "Cambiar rol de usuarios",
                Usuarios_AsignarSucursal => "Asignar sucursal a usuario",

                Roles_Ver => "Ver lista de roles",
                Roles_Crear => "Crear nuevos roles",
                Roles_Editar => "Editar roles existentes",
                Roles_Eliminar => "Eliminar roles",
                Roles_AsignarPermisos => "Asignar permisos a roles",

                Clientes_Ver => "Ver lista de clientes",
                Clientes_Crear => "Crear nuevos clientes",
                Clientes_Editar => "Editar clientes existentes",
                Clientes_Eliminar => "Eliminar clientes",
                Clientes_Exportar => "Exportar clientes a archivo",
                Clientes_VerTodasSucursales => "Ver clientes de todas las sucursales",
                Clientes_AsignarCategoria => "Asignar categoría a cliente",
                Clientes_GenerarQR => "Generar código QR para cliente",

                Promociones_Ver => "Ver lista de promociones",
                Promociones_Crear => "Crear nuevas promociones",
                Promociones_Editar => "Editar promociones existentes",
                Promociones_Eliminar => "Eliminar promociones",
                Promociones_ActivarDesactivar => "Activar o desactivar promociones",
                Promociones_VerTodasSucursales => "Ver promociones de todas las sucursales",

                WhatsApp_Ver => "Ver envíos de WhatsApp",
                WhatsApp_Crear => "Crear envíos de WhatsApp",
                WhatsApp_VerTodasSucursales => "Ver envíos de todas las sucursales",

                // También aplica a Reportes_VerEnvios porque ambos tienen
                // exactamente el valor "Ver Reportes de Envíos".
                WhatsApp_VerReportes => "Ver reportes de envíos",

                Sucursales_Ver => "Ver lista de sucursales",
                Sucursales_Crear => "Crear nuevas sucursales",
                Sucursales_Editar => "Editar sucursales existentes",
                Sucursales_Eliminar => "Eliminar sucursales",
                Sucursales_ActivarDesactivar => "Activar o desactivar sucursales",

                Categorias_Ver => "Ver lista de categorías",
                Categorias_Crear => "Crear nuevas categorías",
                Categorias_Editar => "Editar categorías existentes",
                Categorias_Eliminar => "Eliminar categorías",

                Reportes_VerClientes => "Ver reportes de clientes",
                Reportes_VerPromociones => "Ver reportes de promociones",
                Reportes_VerPorSucursal => "Ver reportes por sucursal",
                Reportes_Exportar => "Exportar reportes",

                Configuracion_Ver => "Ver configuración del sistema",
                Configuracion_Editar => "Editar configuración del sistema",
                Configuracion_VerLogs => "Ver logs del sistema",
                Configuracion_Respaldar => "Respaldar base de datos",

                _ => permission
            };
        }
    }
}