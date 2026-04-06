IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'00000000000000_CreateIdentitySchema', N'8.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [Activo] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [AspNetUsers] ADD [FechaRegistro] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [AspNetUsers] ADD [NombreCompleto] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [Telefono] nvarchar(max) NULL;
GO

CREATE TABLE [HistorialEnvios] (
    [Id] int NOT NULL IDENTITY,
    [PromocionId] int NOT NULL,
    [ClienteId] int NOT NULL,
    [UsuarioEnvioId] nvarchar(450) NOT NULL,
    [Mensaje] nvarchar(max) NULL,
    [EnlacePromocion] nvarchar(max) NOT NULL,
    [EnlaceWhatsApp] nvarchar(max) NULL,
    [FechaEnvio] datetime2 NOT NULL,
    [Estatus] nvarchar(50) NOT NULL,
    [Observaciones] nvarchar(max) NULL,
    CONSTRAINT [PK_HistorialEnvios] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Permisos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(500) NULL,
    [Modulo] nvarchar(100) NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Permisos] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Permisos_Nombre] ON [Permisos] ([Nombre]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260130211920_CrearTablasNegocio', N'8.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Promociones] (
    [Id] int NOT NULL IDENTITY,
    [Titulo] nvarchar(200) NOT NULL,
    [Descripcion] nvarchar(max) NULL,
    [Precio] decimal(18,2) NOT NULL,
    [DescuentoPorcentaje] decimal(5,2) NULL,
    [DescuentoMonto] decimal(18,2) NULL,
    [ImagenUrl] nvarchar(max) NULL,
    [ImagenNombre] nvarchar(max) NULL,
    [Fechalnicio] datetime2 NOT NULL,
    [FechaFin] datetime2 NOT NULL,
    [TokenUnico] nvarchar(450) NULL,
    [Activo] bit NOT NULL,
    [UsuarioId] nvarchar(max) NULL,
    [ApplicationUserId] nvarchar(450) NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaActualizacion] datetime2 NULL,
    [pdf_nombre] nvarchar(max) NULL,
    [pdf_ruta] nvarchar(max) NULL,
    [pdf_url] nvarchar(max) NULL,
    CONSTRAINT [PK_Promociones] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Promociones_TokenUnico] ON [Promociones] ([TokenUnico]) WHERE [TokenUnico] IS NOT NULL;
GO

CREATE INDEX [IX_Promociones_ApplicationUserId] ON [Promociones] ([ApplicationUserId]);
GO

CREATE TABLE [RolesPermisos] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [PermisoId] int NOT NULL,
    [FechaAsignacion] datetime2 NOT NULL,
    CONSTRAINT [PK_RolesPermisos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RolesPermisos_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolesPermisos_Permisos_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [Permisos] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RolesPermisos_PermisoId] ON [RolesPermisos] ([PermisoId]);
GO

CREATE UNIQUE INDEX [IX_RolesPermisos_RoleId_PermisoId] ON [RolesPermisos] ([RoleId], [PermisoId]);
GO

CREATE TABLE [clientes] (
    [id_folio] int NOT NULL IDENTITY,
    [ApplicationUserId] nvarchar(450) NULL,
    [correo] nvarchar(150) NULL,
    [estado] int NOT NULL DEFAULT 0,
    [id_categoriaProducto] int NULL,
    [id_generador_qr] int NULL,
    [id_sucursal] int NOT NULL DEFAULT 0,
    [id_usuario] nvarchar(max) NOT NULL DEFAULT N'',
    [nombre_completo] nvarchar(180) NOT NULL DEFAULT N'',
    [regtimestamp] datetime2 NOT NULL,
    [telefono] nvarchar(20) NULL,
    CONSTRAINT [PK_clientes] PRIMARY KEY ([id_folio])
);
GO

CREATE INDEX [IX_clientes_ApplicationUserId] ON [clientes] ([ApplicationUserId]);
GO

CREATE UNIQUE INDEX [IX_clientes_telefono] ON [clientes] ([telefono]) WHERE [telefono] IS NOT NULL;
GO

ALTER TABLE [Promociones] ADD CONSTRAINT [FK_Promociones_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]);
GO

ALTER TABLE [clientes] ADD CONSTRAINT [FK_clientes_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260320010200_Inicial', N'8.0.16');
GO

COMMIT;
GO