
CREATE TABLE dbo.CIRI_AdjuntosSolicitudes (
    IdAdjunto INT IDENTITY(1,1) NOT NULL,
    IdHistorialSolicitud INT NOT NULL,
    IdComentario INT NULL,
    NombreAdjunto NVARCHAR(255) NOT NULL,
    Formato VARCHAR(100) NOT NULL,
    TamanoAdjunto BIGINT NOT NULL,
    Ruta NVARCHAR(500) NOT NULL,
    AdjuntadoPor VARCHAR(100) NOT NULL,
    Fecha DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    Estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT PK_CIRI_AdjuntosSolicitudes
        PRIMARY KEY (IdAdjunto),

    CONSTRAINT FK_CIRI_AdjuntosSolicitudes_Historial
        FOREIGN KEY (IdHistorialSolicitud)
        REFERENCES dbo.CIRI_HistorialSolicitudes(IdHistorialSolicitud),

    CONSTRAINT FK_CIRI_AdjuntosSolicitudes_CIRI_Comentario
        FOREIGN KEY (IdComentario)
        REFERENCES dbo.CIRI_ComentariosSolicitudes(IdComentario)
);

