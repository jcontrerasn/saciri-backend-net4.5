USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_EnviarInformacion]    Script Date: 25/11/2026 09:45:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_ciri_EnviarInformacion]
    @IdSolicitud INT,
    @Comentario NVARCHAR(MAX),
    @Usuario VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdEstadoAnterior INT;
    DECLARE @IdComentario INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validar parámetros
        IF NULLIF(LTRIM(RTRIM(@Comentario)), N'') IS NULL
        BEGIN
            ;THROW 50001, 'El comentario es obligatorio.', 1;
        END;

        IF NULLIF(LTRIM(RTRIM(@Usuario)), '') IS NULL
        BEGIN
            ;THROW 50002, 'El usuario es obligatorio.', 1;
        END;

        -- 2. Obtener estado anterior y bloquear solicitud
        SELECT
            @IdEstadoAnterior = IdEstadoSolicitud
        FROM dbo.CIRI_Solicitudes WITH (UPDLOCK, HOLDLOCK)
        WHERE IdSolicitud = @IdSolicitud;

        IF @IdEstadoAnterior IS NULL
        BEGIN
            ;THROW 50003, 'La solicitud no existe.', 1;
        END;

        -- 3. Actualizar estado de la solicitud a 5
        UPDATE dbo.CIRI_Solicitudes
        SET IdEstadoSolicitud = 5
        WHERE IdSolicitud = @IdSolicitud;

        -- 4. Registrar comentario
        INSERT INTO dbo.CIRI_ComentariosSolicitudes (
            IdSolicitud,
            Comentario,
            Tipo,
            Usuario
        )
        VALUES (
            @IdSolicitud,
            @Comentario,
            'Comentarios',
            @Usuario
        );

        SET @IdComentario = CONVERT(INT, SCOPE_IDENTITY());

        -- 5. Registrar historial completo
        INSERT INTO dbo.CIRI_HistorialSolicitudes (
            IdSolicitud,
            SolicitadoPara,
            CreadoPor,
            CodigoSolicitud,
            EstadoSolicitud,
            FechaSolicitud,
            TipoRegistro,
            Categoria,
            Subcategoria,
            Ocurrencia,
            Prioridad,
            AsignadoA,
            FechaInicio,
            FechaCaducidad,
            FechaFinAtencion,
            DuracionAtencion,
            CopiaJefatura,
            JefeAprobador,
            EstadoAprobacion,
            UsuarioAprobacion,
            FechaAprobacion,
            Nivel,
            ProblemaMasivo,
            CCSolicitadoPara,
            Accion,
            UsuarioModificacion,
            Observacion,
            EstadoAnterior,
            EstadoNuevo
        )
        SELECT
            s.IdSolicitud,
            s.SolicitadoPara,
            s.CreadoPor,
            s.CodigoSolicitud,
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud),
            s.FechaSolicitud,
            (SELECT Nombre FROM dbo.CIRI_TipoRegistros
             WHERE IdTipoRegistro = s.IdTipoRegistro),
            (SELECT Nombre FROM dbo.CIRI_Categorias
             WHERE IdCategoria = s.IdCategoria),
            (SELECT Nombre FROM dbo.CIRI_Subcategorias
             WHERE IdSubcategoria = s.IdSubcategoria),
            (SELECT Nombre FROM dbo.CIRI_Ocurrencias
             WHERE IdOcurrencia = s.IdOcurrencia),
            s.Prioridad,
            s.AsignadoA,
            s.FechaInicio,
            s.FechaCaducidad,
            s.FechaFinAtencion,
            s.DuracionAtencion,
            s.CopiaJefatura,
            s.JefeAprobador,
            s.EstadoAprobacion,
            s.UsuarioAprobacion,
            s.FechaAprobacion,
            s.Nivel,
            s.ProblemaMasivo,
            s.CCSolicitadoPara,

            'Envío de Información',   -- Acción realizada
            @Usuario,             -- Usuario que realizó el cambio
            'Se envió información de la solicitud',          -- Observación
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = @IdEstadoAnterior),
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud)

        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        -- 6. Confirmar todas las operaciones
        COMMIT TRANSACTION;

        -- 7. Respuesta
        SELECT
            CAST(1 AS BIT) AS Success,
            'Se envió información correctamente' AS Mensaje,
            @IdComentario AS IdComentario;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
