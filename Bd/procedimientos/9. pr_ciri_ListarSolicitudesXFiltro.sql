USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_ListarSolicitudesXFiltro]    Script Date: 25/11/2026 13:12:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--exec [pr_ciri_ListarSolicitudesXFiltro] 'JCONTRERASN', 'MIS_SOLICITUDES'

ALTER PROCEDURE [dbo].[pr_ciri_ListarSolicitudesXFiltro]
    @Pusuario_c VARCHAR(100),
    @Pflag_c VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.CodigoSolicitud,
        tipoRegistro.Nombre AS TipoRegistro,
        categoria.Nombre AS Categoria,
        usuarioCreado.NombreCompleto AS CreadoPor,
        usuarioSolicitado.NombreCompleto AS SolicitadoPara,
        usuarioAsignado.NombreCompleto AS AsignadoA,
        s.FechaSolicitud,
        s.FechaCaducidad AS FechaEstimada,
        s.CopiaJefatura,
        estado.Nombre AS Estado
    FROM dbo.CIRI_Solicitudes AS s
    INNER JOIN dbo.CIRI_TipoRegistros AS tipoRegistro
        ON tipoRegistro.IdTipoRegistro = s.IdTipoRegistro
    INNER JOIN dbo.CIRI_Categorias AS categoria
        ON categoria.IdCategoria = s.IdCategoria
    LEFT JOIN dbo.TS_USUARIO AS usuarioCreado
        ON usuarioCreado.Usuario = s.CreadoPor
    LEFT JOIN dbo.TS_USUARIO AS usuarioSolicitado
        ON usuarioSolicitado.Usuario = s.SolicitadoPara
    LEFT JOIN dbo.TS_USUARIO AS usuarioAsignado
        ON usuarioAsignado.Usuario = s.AsignadoA
    INNER JOIN dbo.CIRI_EstadosSolicitud AS estado
        ON estado.IdEstadoSolicitud = s.IdEstadoSolicitud
    WHERE
      (
          (@Pflag_c IN ('MIS_SOLICITUDES', 'TODAS')
           AND s.IdEstadoSolicitud IN (1, 2, 3, 4, 5, 6, 7))
          OR
          (@Pflag_c IN ('PENDIENTES_ASIGNACION')
           AND s.IdEstadoSolicitud = 2)
          OR
          (@Pflag_c IN ('CONCLUIDAS')
           AND s.IdEstadoSolicitud IN (5, 7))
          OR
          (@Pflag_c IN ('ANULADAS')
           AND s.IdEstadoSolicitud = 6)
          OR
          (@Pflag_c NOT IN (
              'MIS_SOLICITUDES', 'TODAS',
              'PENDIENTES_ASIGNACION', 'CONCLUIDAS', 'ANULADAS')
           AND s.IdEstadoSolicitud IN (1, 2, 3, 4, 5))
      )
      AND
      (
          (@Pflag_c = 'CREADAS' AND
              (s.CreadoPor = LTRIM(RTRIM(@Pusuario_c))
               OR s.SolicitadoPara = LTRIM(RTRIM(@Pusuario_c))))
          OR
          (@Pflag_c = 'ASIGNADAS' AND
              s.AsignadoA = LTRIM(RTRIM(@Pusuario_c)))
          OR
          (@Pflag_c = 'MIS_SOLICITUDES' AND
              (s.CreadoPor = LTRIM(RTRIM(@Pusuario_c))
               OR s.SolicitadoPara = LTRIM(RTRIM(@Pusuario_c)))
              AND (s.AsignadoA IS NULL
                   OR s.AsignadoA <> LTRIM(RTRIM(@Pusuario_c))))
          OR
          (@Pflag_c IN (
              'TODAS', 'PENDIENTES_ASIGNACION', 'CONCLUIDAS', 'ANULADAS'))
      )
    ORDER BY s.FechaSolicitud DESC, s.IdSolicitud DESC;
END
