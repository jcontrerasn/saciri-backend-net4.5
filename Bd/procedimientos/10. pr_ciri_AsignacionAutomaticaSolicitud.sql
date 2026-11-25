USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_AsignacionAutomaticaSolicitud]    Script Date: 25/11/2026 14:46:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_ciri_AsignacionAutomaticaSolicitud]
    @IdSolicitud INT = NULL,
    @Usuario VARCHAR(100) = NULL,
    @UsuarioAsignado VARCHAR(100) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Nivel1 VARCHAR(100);
    DECLARE @Nivel2 VARCHAR(100);
    DECLARE @EstadoSolicitud INT;
    DECLARE @AsignadoActual VARCHAR(100);
    DECLARE @EsAsignacionPorConexion BIT =
        CASE WHEN @IdSolicitud IS NULL THEN 1 ELSE 0 END;
    DECLARE @TransaccionPropia BIT = 0;

    SET @UsuarioAsignado = NULL;

    IF @EsAsignacionPorConexion = 1
       AND NULLIF(LTRIM(RTRIM(@Usuario)), '') IS NULL
    BEGIN
        ;THROW 50007, 'El usuario es obligatorio para buscar solicitudes.', 1;
    END;

    BEGIN TRY
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRANSACTION;
            SET @TransaccionPropia = 1;
        END;

        IF @EsAsignacionPorConexion = 1
        BEGIN
            -- Buscar la primera solicitud pendiente compatible con el usuario conectado.
            -- UPDLOCK y READPAST evitan que dos usuarios tomen la misma solicitud.
            SELECT TOP (1)
                @IdSolicitud = s.IdSolicitud
            FROM dbo.CIRI_Solicitudes s WITH (
                UPDLOCK,
                READPAST,
                ROWLOCK,
                READCOMMITTEDLOCK
            )
            INNER JOIN dbo.CIRI_CatSubOcu c
                ON c.IdOcurrencia = s.IdOcurrencia
            INNER JOIN CIRI_UsuariosRoles ur
                ON ur.Usuario = @Usuario
               AND ur.Activo = 1
            INNER JOIN dbo.CIRI_ConexionesUsuarios cu
                ON cu.Usuario = ur.Usuario
               AND cu.Estado = 1
            WHERE s.IdEstadoSolicitud = 1
              AND s.AsignadoA IS NULL
              AND (
                    CHARINDEX(
                        ';' + NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(100), c.Nivel1))), '') + ';',
                        ';' + REPLACE(ISNULL(ur.Puestos, ''), ' ', '') + ';'
                    ) > 0
                    OR CHARINDEX(
                        ';' + NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(100), c.Nivel2))), '') + ';',
                        ';' + REPLACE(ISNULL(ur.Puestos, ''), ' ', '') + ';'
                    ) > 0
              )
            ORDER BY
                CASE
                    WHEN CHARINDEX(
                        ';' + NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(100), c.Nivel1))), '') + ';',
                        ';' + REPLACE(ISNULL(ur.Puestos, ''), ' ', '') + ';'
                    ) > 0 THEN 1
                    ELSE 2
                END,
                s.FechaSolicitud,
                s.IdSolicitud;

            SET @UsuarioAsignado = @Usuario;
        END;

        IF @EsAsignacionPorConexion = 0
        BEGIN
            -- Modo usado al registrar una solicitud: buscar un usuario conectado.
            SELECT
                @EstadoSolicitud = s.IdEstadoSolicitud,
                @AsignadoActual = s.AsignadoA,
                @Nivel1 = NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(100), c.Nivel1))), ''),
                @Nivel2 = NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(100), c.Nivel2))), '')
            FROM dbo.CIRI_Solicitudes s WITH (UPDLOCK, HOLDLOCK)
            LEFT JOIN dbo.CIRI_CatSubOcu c
                ON c.IdOcurrencia = s.IdOcurrencia
            WHERE s.IdSolicitud = @IdSolicitud;

            IF @EstadoSolicitud IS NULL
            BEGIN
                ;THROW 50006, 'La solicitud no existe.', 1;
            END;

            IF @EstadoSolicitud <> 1 OR @AsignadoActual IS NOT NULL
            BEGIN
                IF @TransaccionPropia = 1
                    COMMIT TRANSACTION;
                RETURN;
            END;

            IF @Nivel1 IS NOT NULL
            BEGIN
                SELECT TOP (1)
                    @UsuarioAsignado = ur.Usuario
                FROM CIRI_UsuariosRoles ur
                WHERE ur.Activo = 1
                  AND EXISTS (
                      SELECT 1
                      FROM dbo.CIRI_ConexionesUsuarios cu
                      WHERE cu.Usuario = ur.Usuario
                        AND cu.Estado = 1
                  )
                  AND CHARINDEX(
                      ';' + @Nivel1 + ';',
                      ';' + REPLACE(ISNULL(ur.Puestos, ''), ' ', '') + ';'
                  ) > 0
                ORDER BY ur.IdUsuarioRol;
            END;

            IF @UsuarioAsignado IS NULL AND @Nivel2 IS NOT NULL
            BEGIN
                SELECT TOP (1)
                    @UsuarioAsignado = ur.Usuario
                FROM CIRI_UsuariosRoles ur
                WHERE ur.Activo = 1
                  AND EXISTS (
                      SELECT 1
                      FROM dbo.CIRI_ConexionesUsuarios cu
                      WHERE cu.Usuario = ur.Usuario
                        AND cu.Estado = 1
                  )
                  AND CHARINDEX(
                      ';' + @Nivel2 + ';',
                      ';' + REPLACE(ISNULL(ur.Puestos, ''), ' ', '') + ';'
                  ) > 0
                ORDER BY ur.IdUsuarioRol;
            END;
        END;

        IF @IdSolicitud IS NULL OR @UsuarioAsignado IS NULL
        BEGIN
            IF @TransaccionPropia = 1
                COMMIT TRANSACTION;

            IF @EsAsignacionPorConexion = 1
            BEGIN
                SELECT
                    CAST(NULL AS INT) AS IdSolicitud,
                    CAST(NULL AS VARCHAR(100)) AS CodigoSolicitud,
                    CAST(NULL AS VARCHAR(100)) AS AsignadoA,
                    CAST(NULL AS INT) AS IdEstadoSolicitud
                WHERE 1 = 0;
            END;

            RETURN;
        END;

        UPDATE dbo.CIRI_Solicitudes
        SET
            IdEstadoSolicitud = 2,
            AsignadoA = @UsuarioAsignado
        WHERE IdSolicitud = @IdSolicitud
          AND IdEstadoSolicitud = 1
          AND AsignadoA IS NULL;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @UsuarioAsignado = NULL;

            IF @TransaccionPropia = 1
                COMMIT TRANSACTION;

            RETURN;
        END;

        IF @EsAsignacionPorConexion = 1
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
                'AsignacionAutomatica',
                @Usuario,
                N'Solicitud asignada al conectarse el usuario',
                1,
                2
            FROM dbo.CIRI_Solicitudes s
            WHERE s.IdSolicitud = @IdSolicitud;

            IF @TransaccionPropia = 1
                COMMIT TRANSACTION;

            SELECT
                s.IdSolicitud,
                s.CodigoSolicitud,
                s.AsignadoA,
                s.IdEstadoSolicitud
            FROM dbo.CIRI_Solicitudes s
            WHERE s.IdSolicitud = @IdSolicitud;

            RETURN;
        END;

        IF @TransaccionPropia = 1
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @TransaccionPropia = 1 AND XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
