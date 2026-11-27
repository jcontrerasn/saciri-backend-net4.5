USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_SolicitudEnProceso]    Script Date: 25/11/2026 14:56:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_ciri_SolicitudEnProceso]
    @CodigoSolicitud VARCHAR(100),
    @Usuario VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdSolicitud INT;
    DECLARE @IdEstadoAnterior INT;
    DECLARE @TiempoAtencion INT;

    DECLARE @FechaInicio DATETIME2(7) = GETDATE();

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validar parámetros
        IF NULLIF(LTRIM(RTRIM(@CodigoSolicitud)), '') IS NULL
        BEGIN
            ;THROW 50001, 'El código de solicitud es obligatorio.', 1;
        END;

        IF NULLIF(LTRIM(RTRIM(@Usuario)), '') IS NULL
        BEGIN
            ;THROW 50002, 'El usuario es obligatorio.', 1;
        END;

        -- 2. Obtener solicitud y estado anterior
        SELECT
            @IdSolicitud = s.IdSolicitud,
            @IdEstadoAnterior = s.IdEstadoSolicitud,
            @TiempoAtencion = c.TiempoAtencion
        FROM dbo.CIRI_Solicitudes s WITH (UPDLOCK, HOLDLOCK)
        LEFT JOIN dbo.CIRI_CatSubOcu c
            ON c.IdOcurrencia = s.IdOcurrencia
        WHERE s.CodigoSolicitud = @CodigoSolicitud;

        IF @IdSolicitud IS NULL
        BEGIN
            ;THROW 50003, 'La solicitud no existe.', 1;
        END;

        IF @TiempoAtencion IS NULL OR @TiempoAtencion < 0
        BEGIN
            ;THROW 50006, 'La solicitud no tiene un TiempoAtencion valido.', 1;
        END;

        -- 3. Evitar registrar dos veces el mismo cambio
        IF @IdEstadoAnterior = 5
        BEGIN
            ;THROW 50004, 'La solicitud ya se encuentra En Atención.', 1;
        END;

		-- 4. Evitar que otro usuario actualice el estado
        IF @Usuario <> (SELECT AsignadoA FROM CIRI_Solicitudes where CodigoSolicitud = @CodigoSolicitud)
        BEGIN
            ;THROW 50005, 'Esta solicitud no se encuentra Asignada a usted.', 1;
        END;

        -- 4. Actualizar solicitud
        UPDATE dbo.CIRI_Solicitudes
        SET
            IdEstadoSolicitud = 5,
            FechaInicio = @FechaInicio,
            FechaCaducidad = DATEADD(MINUTE, @TiempoAtencion, @FechaInicio)
        WHERE IdSolicitud = @IdSolicitud;

        -- 5. INSERTAR HISTORIAL
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

            'En Atención',                         -- Accion
            @Usuario,                           -- UsuarioModificacion
            N'Se actualizó el estado de la solicitud a En Atención debido al acceso del usuario asignado',    -- Observacion
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = @IdEstadoAnterior),
            (SELECT Nombre FROM dbo.CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud)

        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        -- 6. Confirmar cambios
        COMMIT TRANSACTION;

        -- 7. Respuesta
        SELECT
            CAST(1 AS BIT) AS Success,
            'La solicitud pasó a estado En Atención correctamente' AS Mensaje;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
