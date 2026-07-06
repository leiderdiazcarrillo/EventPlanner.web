-- =========================
-- TABLA PROGRAMA FORMACION
-- =========================

CREATE TABLE ProgramaFormacion(
    idProgramaFormacion INT IDENTITY(1,1) PRIMARY KEY,
    NombrePrograma VARCHAR(150) NOT NULL
);

SELECT * FROM ProgramaFormacion;

-- =========================
-- TABLA USUARIO
-- =========================

CREATE TABLE Usuario(
    idUsuario INT IDENTITY(1,1) PRIMARY KEY,

    idProgramaFormacion INT NOT NULL,

    TipoDocumento VARCHAR(20) NOT NULL,
    NumeroDocumento VARCHAR(20) NOT NULL UNIQUE,

    NombreCompleto VARCHAR(100) NOT NULL,

    Correo VARCHAR(100) NOT NULL UNIQUE,

    Ficha VARCHAR(20) NOT NULL,

    Jornada VARCHAR(50) NOT NULL,

    Rol VARCHAR(20) NOT NULL,

    FechaRegistro DATETIME NOT NULL,

    Contrasena VARCHAR(255) NOT NULL,

    CONSTRAINT FK_Usuario_ProgramaFormacion
    FOREIGN KEY(idProgramaFormacion)
    REFERENCES ProgramaFormacion(idProgramaFormacion)
);

SELECT * FROM Usuario;

-- =========================
-- TABLA TIPO EVENTO
-- =========================

CREATE TABLE TipoEvento(
    idTipoEvento INT IDENTITY(1,1) PRIMARY KEY,

    NombreTipoEvento VARCHAR(50) NOT NULL
);

SELECT * FROM TipoEvento;

-- =========================
-- TABLA EVENTO
-- =========================

CREATE TABLE Evento(
    idEvento INT IDENTITY(1,1) PRIMARY KEY,

    idTipoEvento INT NOT NULL,

    NombreEvento VARCHAR(100) NOT NULL,

    Descripcion VARCHAR(300) NOT NULL,

    Modalidad VARCHAR(20) NOT NULL,

    Lugar VARCHAR(100) NOT NULL,

    FechaEvento DATE NOT NULL,

    HoraInicio TIME NOT NULL,

    HoraFin TIME NOT NULL,

    CuposTotales INT NOT NULL,

    FechaInicioInscripcion DATE NOT NULL,

    FechaFinInscripcion DATE NOT NULL,

    Activo BIT NOT NULL,

    FechaCreacion DATETIME NOT NULL,

    CONSTRAINT FK_Evento_TipoEvento
    FOREIGN KEY(idTipoEvento)
    REFERENCES TipoEvento(idTipoEvento)
);

SELECT * FROM Evento;

-- =========================
-- TABLA EQUIPO
-- =========================

CREATE TABLE Equipo(
    idEquipo INT IDENTITY(1,1) PRIMARY KEY,

    idEvento INT NOT NULL,

    NombreEquipo VARCHAR(100) NOT NULL,

    CantidadMinima INT NOT NULL,

    CantidadMaxima INT NOT NULL,

    CONSTRAINT FK_Equipo_Evento
    FOREIGN KEY(idEvento)
    REFERENCES Evento(idEvento)
);

SELECT * FROM Equipo;

-- =========================
-- TABLA INSCRIPCION
-- =========================

CREATE TABLE Inscripcion(
    idInscripcion INT IDENTITY(1,1) PRIMARY KEY,

    idUsuario INT NOT NULL,

    idEvento INT NOT NULL,

    idEquipo INT NULL,

    FechaInscripcion DATETIME NOT NULL,

    CONSTRAINT FK_Inscripcion_Usuario
    FOREIGN KEY(idUsuario)
    REFERENCES Usuario(idUsuario),

    CONSTRAINT FK_Inscripcion_Evento
    FOREIGN KEY(idEvento)
    REFERENCES Evento(idEvento),

    CONSTRAINT FK_Inscripcion_Equipo
    FOREIGN KEY(idEquipo)
    REFERENCES Equipo(idEquipo)
);
SELECT * FROM Inscripcion;

-- =========================
-- AGREGAR FOTOPERFIL A USUARIO
-- =========================
-- Ejecutar si la columna no existe:
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Usuario') AND name = 'FotoPerfil'
)
BEGIN
    ALTER TABLE Usuario ADD FotoPerfil VARCHAR(500) NULL;
END
