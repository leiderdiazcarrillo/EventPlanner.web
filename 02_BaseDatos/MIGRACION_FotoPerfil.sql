-- =========================
-- MIGRACION: Agregar columna FotoPerfil a la tabla Usuario
-- Ejecutar este script en SQL Server si la base ya existe
-- =========================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'Usuario') AND name = N'FotoPerfil'
)
BEGIN
    ALTER TABLE Usuario ADD FotoPerfil VARCHAR(500) NULL;
    PRINT 'Columna FotoPerfil agregada correctamente.';
END
ELSE
BEGIN
    PRINT 'La columna FotoPerfil ya existe.';
END
