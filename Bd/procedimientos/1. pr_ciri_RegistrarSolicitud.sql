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
			(
				SELECT c.Copia FROM dbo.CIRI_CatSubOcu AS c
				WHERE c.IdOcurrencia = @IdOcurrencia AND c.Estado = 1
			)
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
            (SELECT Nombre FROM CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud),
            s.FechaSolicitud,
			(SELECT Nombre FROM CIRI_TipoRegistros
             WHERE IdTipoRegistro = s.IdTipoRegistro),
			(SELECT Nombre FROM CIRI_Categorias
             WHERE IdCategoria = s.IdCategoria),
			(SELECT Nombre FROM CIRI_Subcategorias
             WHERE IdSubcategoria = s.IdSubcategoria),
			(SELECT Nombre FROM CIRI_Ocurrencias
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
            'Creacion',
            @CreadoPor,
            N'Solicitud creada',
            NULL,
            (SELECT Nombre FROM CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = 1)
        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        --  ASIGNAR RESPONSABLE AUTOMÁTICAMENTE
        EXEC dbo.pr_ciri_AsignacionAutomaticaSolicitud
            @IdSolicitud = @IdSolicitud,
            @UsuarioAsignado = @UsuarioAsignado OUTPUT;

        -- 11. SEGUNDO HISTORIAL: ASIGNACIÓN AUTOMÁTICA
        IF @UsuarioAsignado IS NULL
        BEGIN
            UPDATE dbo.CIRI_Solicitudes
            SET IdEstadoSolicitud = 3
            WHERE IdSolicitud = @IdSolicitud
              AND IdEstadoSolicitud = 1
              AND AsignadoA IS NULL;
        END;

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
            (SELECT Nombre FROM CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = s.IdEstadoSolicitud),
            s.FechaSolicitud,
			(SELECT Nombre FROM CIRI_TipoRegistros
             WHERE IdTipoRegistro = s.IdTipoRegistro),
			(SELECT Nombre FROM CIRI_Categorias
             WHERE IdCategoria = s.IdCategoria),
			(SELECT Nombre FROM CIRI_Subcategorias
             WHERE IdSubcategoria = s.IdSubcategoria),
			(SELECT Nombre FROM CIRI_Ocurrencias
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
            CASE
                WHEN @UsuarioAsignado IS NULL
                    THEN 'Pendiente de Asignacion'
                ELSE 'Asignacion automatica'
            END,
            'SACIRI',
            CASE
                WHEN @UsuarioAsignado IS NULL
                    THEN 'No se encontro un usuario disponible para asignar'
                ELSE 'El sistema asigno automaticamente la solicitud'
            END,
			(SELECT Nombre FROM CIRI_EstadosSolicitud
             WHERE IdEstadoSolicitud = 1),
            CASE
                WHEN @UsuarioAsignado IS NULL THEN
                    (SELECT Nombre FROM CIRI_EstadosSolicitud
                     WHERE IdEstadoSolicitud = 3)
                ELSE
                    (SELECT Nombre FROM CIRI_EstadosSolicitud
                     WHERE IdEstadoSolicitud = 4)
            END
        FROM dbo.CIRI_Solicitudes s
        WHERE s.IdSolicitud = @IdSolicitud;

        UPDATE h
        SET
            Accion = CASE
                WHEN @UsuarioAsignado IS NULL
                    THEN 'Pendiente de Asignacion'
                ELSE 'Asignacion automatica'
            END,
            Observacion = CASE
                WHEN @UsuarioAsignado IS NULL
                    THEN 'No se encontro un usuario disponible para asignar'
                ELSE 'El sistema asigno automaticamente la solicitud'
            END
        FROM dbo.CIRI_HistorialSolicitudes h
        WHERE h.IdSolicitud = @IdSolicitud
          AND h.EstadoAnterior = (SELECT Nombre
                                  FROM CIRI_EstadosSolicitud
                                  WHERE IdEstadoSolicitud = 1)
          AND h.EstadoNuevo = CASE
              WHEN @UsuarioAsignado IS NULL THEN
                  (SELECT Nombre FROM CIRI_EstadosSolicitud
                   WHERE IdEstadoSolicitud = 3)
              ELSE
                  (SELECT Nombre FROM CIRI_EstadosSolicitud
                   WHERE IdEstadoSolicitud = 4)
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
