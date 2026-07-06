-- =========================
-- DATOS INICIALES
-- PROGRAMA FORMACION
-- =========================

INSERT INTO ProgramaFormacion(NombrePrograma)
VALUES('ANALISIS Y DESARROLLO DE SOFTWARE'),
('CONTABILIDAD'),
('LOGISTICA EMPRESARIAL'),
('GESTION ADMINISTRATIVA'),
('MANTENIMIENTO INDUSTRIAL');

INSERT INTO ProgramaFormacion(NombrePrograma)
VALUES ('ADMINISTRACION SISTEMA');

SELECT * FROM ProgramaFormacion;

-- =========================
-- TIPO EVENTO
-- =========================

INSERT INTO TipoEvento(NombreTipoEvento)
VALUES ('ACADEMICO');

INSERT INTO TipoEvento(NombreTipoEvento)
VALUES ('DEPORTIVO');

INSERT INTO TipoEvento(NombreTipoEvento)
VALUES ('CULTURAL');

SELECT * FROM TipoEvento;

-- =========================
-- ADMINISTRADORES DEL SISTEMA
-- =========================

INSERT INTO Usuario
(idProgramaFormacion, TipoDocumento, NumeroDocumento, NombreCompleto, Correo, Ficha, Jornada, Rol, FechaRegistro, Contrasena)
VALUES
(6, 'CC', '1048442056', 'Leider Maza Taboada', 'leider.maza@soy.sena.edu.co', 'ADMIN', 'ADMINISTRATIVA', 'ADMIN', GETDATE(), '123456');

INSERT INTO Usuario
(idProgramaFormacion, TipoDocumento, NumeroDocumento, NombreCompleto, Correo, Ficha, Jornada, Rol, FechaRegistro, Contrasena)
VALUES
(6, 'CC', '1043456782', 'Alejandro Vera', 'alejandro.vera@soy.sena.edu.co', 'ADMIN', 'ADMINISTRATIVA', 'ADMIN', GETDATE(), '123456');

INSERT INTO Usuario
(idProgramaFormacion, TipoDocumento, NumeroDocumento, NombreCompleto, Correo, Ficha, Jornada, Rol, FechaRegistro, Contrasena)
VALUES
(6, 'CC', '1048554459', 'Leider Manuel Diaz', 'leider.diaz@soy.sena.edu.co', 'ADMIN', 'ADMINISTRATIVA', 'ADMIN', GETDATE(), '123456');

SELECT * FROM Usuario;