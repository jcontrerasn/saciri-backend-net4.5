USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_RegistrarSolicitud]    Script Date: 24/11/2026 17:18:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_ciri_RegistrarSolicitud]

    @SolicitadoPara VARCHAR(100) = NULL,
    @CreadoPor VARCHAR(100),
	@IdTipoRegistro INT,
    @IdCategoria INT,
    @IdSubcategoria INT = NULL,
    @IdOcurrencia INT = NULL,
    @CentroCosto VARCHAR(50),
    @Lugar VARCHAR(200),
    @Descripcion NVARCHAR(MAX),
    @Recomendacion NVARCHAR(MAX) = NULL

AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdSolicitud INT;
    DECLARE @CodigoSolicitud VARCHAR(100);
    DECLARE @Prefijo VARCHAR(20);
    DECLARE @Correlativo INT;
    DECLARE @ResultadoBloqueo INT;
    DECLARE @FechaActual DATETIME2(7) = GETDATE();
    DECLARE @UsuarioAsignado VARCHAR(100);

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC @ResultadoBloqueo = sys.sp_getapplock
            @Resource = 'SA-CIRI-CodigoSolicitud',
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 15000;

        IF @ResultadoBloqueo < 0
        BEGIN
            ;THROW 50001,
                'No se pudo obtener el bloqueo para generar el codigo.',
                1;
        END;


        -- Generar código CIRI26-1, CIRI26-2...
        SET @Prefijo =
            'CIRI' +
            RIGHT(
                CONVERT(VARCHAR(4), YEAR(@FechaActual)),
                2
            ) + '-';

        SELECT
            @Correlativo = ISNULL(
                MAX(
                    TRY_CONVERT(
                        INT,
                        SUBSTRING(
                            CodigoSolicitud,
                            LEN(@Prefijo) + 1,
                            100
                        )
                    )
                ),
                0
            ) + 1
        FROM dbo.CIRI_Solicitudes
        WHERE CodigoSolicitud LIKE @Prefijo + '%';

        SET @CodigoSolicitud =
            @Prefijo + CONVERT(VARCHAR(20), @Correlativo);

        -- INSERTAR SOLICITUD EN ESTADO 1, SIN ASIGNADO
        INSERT INTO dbo.CIRI_Solicitudes (
            SolicitadoPara,
            CreadoPor,
            CodigoSolicitud,
            IdEstadoSolicitud,
            FechaSolicitud,
            IdTipoRegistro,
            IdCategoria,
            IdSubcategoria,
            IdOcurrencia,
            AsignadoA,
			CopiaJefatura
        )
        VALUES (
            @SolicitadoPara,
            @CreadoPor,
            @CodigoSolicitud,
            1, -- Estado inicial
            @FechaActual,
            @IdTipoRegistro,
            @IdCategoria,
            @IdSubcategoria,
            @IdOcurrencia,
            NULL, -- Todavía sin asignar
			(select Copia from CIRI_CatSubOcu)
        );

        SET @IdSolicitud = CONVERT(INT, SCOPE_IDENTITY());

        --  INSERTAR DETALLE
        INSERT INTO dbo.CIRI_DetalleSolicitudes (
            IdSolicitud,
            CentroCosto,
            Lugar
        )
        VALUES (
            @IdSolicitud,
            @CentroCosto,
            @Lugar
        );

        --  INSERTAR COMENTARIO INICIAL
        INSERT INTO dbo.CIRI_ComentariosSolicitudes (
            IdSolicitud,
            Comentario,
            Tipo,
            Usuario
        )
        VALUES (
            @IdSolicitud,
            @Descripcion,
            'Comentarios',
            @CreadoPor
        );

        --  INSERTAR RECOMENDACIÓN OPCIONAL
        IF NULLIF(LTRIM(RTRIM(@Recomendacion)), N'') IS NOT NULL
        BEGIN
            INSERT INTO dbo.CIRI_ComentariosSolicitudes (
                IdSolicitud,
                Comentario,
                Tipo,
                Usuario
            )
            VALUES (
                @IdSolicitud,
                @Recomendacion,
                'Recomendacion',
                @CreadoPor
            );
        END;

        --  PRIMER HISTORIAL: CREACIÓN, ESTADO 1
        INSERT INTO dbo.CIRI_HistorialSolicitudes (
            IdSolicitud,
            SolicitadoPara,
            CreadoPor,
            CodigoSolicitud,
            IdEstadoSolicitud,
            FechaSolicitud,
            IdTipoRegistro,
            IdCategoria,
            IdSubcategoria,
            IdOcurrencia,
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
            IdEstadoAnterior,
            IdEstadoNuevo
        )
        SELECT
            s.IdSolicitud,
            s.SolicitadoPara,
            s.CreadoPor,
            s.CodigoSolicitud,
            s.IdEstadoSolicitud,
            s.FechaSolicitud,
            s.IdTipoRegistro,
            s.IdCategoria,
            s.IdSubcategoria,
            s.IdOcurrencia,
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
            'Creacion',
            @CreadoPor,
            N'Solicitud creada',
            NULL,
            1
        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        --  ACTUALIZAR SOLICITUD A ESTADO 2
        --     Y ASIGNAR RESPONSABLE
        EXEC dbo.pr_ciri_AsignacionAutomaticaSolicitud
            @IdSolicitud = @IdSolicitud,
            @UsuarioAsignado = @UsuarioAsignado OUTPUT;

        -- 11. SEGUNDO HISTORIAL: ASIGNACIÓN AUTOMÁTICA
        IF @UsuarioAsignado IS NOT NULL
        BEGIN
            INSERT INTO dbo.CIRI_HistorialSolicitudes (
            IdSolicitud,
            SolicitadoPara,
            CreadoPor,
            CodigoSolicitud,
            IdEstadoSolicitud,
            FechaSolicitud,
            IdTipoRegistro,
            IdCategoria,
            IdSubcategoria,
            IdOcurrencia,
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
            IdEstadoAnterior,
            IdEstadoNuevo
        )
        SELECT
            s.IdSolicitud,
            s.SolicitadoPara,
            s.CreadoPor,
            s.CodigoSolicitud,
            s.IdEstadoSolicitud,
            s.FechaSolicitud,
            s.IdTipoRegistro,
            s.IdCategoria,
            s.IdSubcategoria,
            s.IdOcurrencia,
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
            N'Asignación automática',
            'SACIRI',
            N'El sistema asignó automáticamente la solicitud',
            1,
            2
        FROM dbo.CIRI_Solicitudes s
            WHERE s.IdSolicitud = @IdSolicitud;
        END;

        -- 12. CONFIRMAR TODAS LAS OPERACIONES
        COMMIT TRANSACTION;

        -- 13. DEVOLVER SOLO EL CÓDIGO
        SELECT @CodigoSolicitud AS CodigoSolicitud;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
