USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_FinalizarSolicitud]    Script Date: 25/11/2026 09:59:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_ciri_FinalizarSolicitud]
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
            ;THROW 50001, 'El comentario de solucion es obligatorio.', 1;
        END;

        IF NULLIF(LTRIM(RTRIM(@Usuario)), '') IS NULL
        BEGIN
            ;THROW 50002, 'El usuario es obligatorio.', 1;
        END;

        -- 2. Obtener y bloquear la solicitud
        -- Capturar el estado antes de modificarlo
        SELECT
            @IdEstadoAnterior = IdEstadoSolicitud
        FROM dbo.CIRI_Solicitudes WITH (UPDLOCK, HOLDLOCK)
        WHERE IdSolicitud = @IdSolicitud;

        IF @IdEstadoAnterior IS NULL
        BEGIN
            ;THROW 50003, 'La solicitud no existe.', 1;
        END;

        -- 3. Evitar finalizar nuevamente una solicitud finalizada
        IF @IdEstadoAnterior = 7
        BEGIN
            ;THROW 50004, 'La solicitud ya se encuentra finalizada.', 1;
        END;

        -- 4. Evitar finalizar una solicitud anulada
        IF @IdEstadoAnterior = 10
        BEGIN
            ;THROW 50005, 'No se puede finalizar una solicitud anulada.', 1;
        END;

        -- 5. Actualizar el estado a Finalizada
        UPDATE dbo.CIRI_Solicitudes
        SET
            IdEstadoSolicitud = 7,
            FechaFinAtencion = GETDATE()
        WHERE IdSolicitud = @IdSolicitud;

        -- 6. Registrar comentario de solución
        INSERT INTO dbo.CIRI_ComentariosSolicitudes (
            IdSolicitud,
            Comentario,
            Tipo,
            Usuario
        )
        VALUES (
            @IdSolicitud,
            @Comentario,
            'Solucion',
            @Usuario
        );

        SET @IdComentario = CONVERT(INT, SCOPE_IDENTITY());

        -- 7. Registrar historial completo de la solicitud
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

            'Finalización',       -- Acción realizada
            @Usuario,             -- Usuario que finalizó
            'Se finalizó la solicitud',          -- Observación
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = @IdEstadoAnterior),
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud)

        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        -- 8. Confirmar todos los cambios
        COMMIT TRANSACTION;

        -- 9. Respuesta
        SELECT
            CAST(1 AS BIT) AS Success,
            'Solicitud finalizada correctamente' AS Mensaje,
            @IdComentario AS IdComentario;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
