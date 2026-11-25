USE [SM_PRUEBA]
GO
/****** Object:  StoredProcedure [dbo].[pr_ciri_BuscarSolicitudXCodigoSolicitud]    Script Date: 25/11/2026 13:08:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_ciri_BuscarSolicitudXCodigoSolicitud]
    @PcodigoSolicitud_c VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- VALIDAR CÓDIGO DE SOLICITUD
    IF NULLIF(LTRIM(RTRIM(@PcodigoSolicitud_c)), '') IS NULL
    BEGIN
        ;THROW 50004, 'El codigo de solicitud es obligatorio.', 1;
    END;

    -- =========================================
    -- SELECT 1: INFORMACIÓN DE LA SOLICITUD
    -- =========================================

    SELECT
        s.IdSolicitud,
        s.CodigoSolicitud,
        COALESCE(usuarioCreado.NombreCompleto, s.CreadoPor) AS CreadoPor,
        COALESCE(usuarioSolicitado.NombreCompleto, s.SolicitadoPara) AS SolicitadoPara,
        usuarioSolicitado.[Ubicación] AS Ubicacion,
        usuarioSolicitado.Area,
        usuarioSolicitado.Telefono,
        usuarioSolicitado.ContactoArea,
        usuarioSolicitado.Correo,
        usuarioSolicitado.TelefonoContactoArea,
        tipoRegistro.Nombre AS TipoRegistro,
        estado.Nombre AS Estado,
        categoria.Nombre AS Categoria,
        subcategoria.Nombre AS Subcategoria,
        ocurrencia.Nombre AS Ocurrencia,
        s.CopiaJefatura,
        s.FechaSolicitud,
        COALESCE(usuarioAsignado.NombreCompleto, s.AsignadoA) AS AsignadoA,
        s.FechaInicio AS FechaInicioAtencion,
        s.FechaCaducidad,
        detalle.CentroCosto,
        centroCosto.Nombre_CCosto AS NombreCentroCosto,
        detalle.Lugar
    FROM dbo.CIRI_Solicitudes AS s

    LEFT JOIN dbo.TS_USUARIO AS usuarioCreado
        ON usuarioCreado.Usuario = s.CreadoPor

    LEFT JOIN dbo.TS_USUARIO AS usuarioSolicitado
        ON usuarioSolicitado.Usuario = s.SolicitadoPara

    LEFT JOIN dbo.TS_USUARIO AS usuarioAsignado
        ON usuarioAsignado.Usuario = s.AsignadoA

    INNER JOIN dbo.CIRI_TipoRegistros AS tipoRegistro
        ON tipoRegistro.IdTipoRegistro = s.IdTipoRegistro

    INNER JOIN dbo.CIRI_EstadosSolicitud AS estado
        ON estado.IdEstadoSolicitud = s.IdEstadoSolicitud

    INNER JOIN dbo.CIRI_Categorias AS categoria
        ON categoria.IdCategoria = s.IdCategoria

    LEFT JOIN dbo.CIRI_Subcategorias AS subcategoria
        ON subcategoria.IdSubcategoria = s.IdSubcategoria

    LEFT JOIN dbo.CIRI_Ocurrencias AS ocurrencia
        ON ocurrencia.IdOcurrencia = s.IdOcurrencia

    LEFT JOIN dbo.CIRI_DetalleSolicitudes AS detalle
        ON detalle.IdSolicitud = s.IdSolicitud

    LEFT JOIN dbo.TS_CENTRO_COSTO AS centroCosto
        ON centroCosto.CCosto = detalle.CentroCosto

    WHERE s.CodigoSolicitud = LTRIM(RTRIM(@PcodigoSolicitud_c));

    SELECT
        comentario.IdComentario,
        comentario.Comentario,
        comentario.Tipo,
        comentario.Usuario,
        comentario.FechaRegistro
    FROM dbo.CIRI_ComentariosSolicitudes AS comentario

    INNER JOIN dbo.CIRI_Solicitudes AS s
        ON s.IdSolicitud = comentario.IdSolicitud

    WHERE s.CodigoSolicitud = LTRIM(RTRIM(@PcodigoSolicitud_c))

    ORDER BY
        comentario.FechaRegistro ASC,
        comentario.IdComentario ASC;

    SELECT
        adjunto.IdAdjunto,
        historial.IdSolicitud,
        adjunto.IdHistorialSolicitud,
        adjunto.IdComentario,
        adjunto.NombreAdjunto,
        adjunto.Formato,
        adjunto.TamanoAdjunto,
        adjunto.Ruta,
        adjunto.AdjuntadoPor,
        adjunto.Fecha,
        adjunto.Estado
    FROM dbo.CIRI_AdjuntosSolicitudes AS adjunto
    INNER JOIN dbo.CIRI_HistorialSolicitudes AS historial
        ON historial.IdHistorialSolicitud = adjunto.IdHistorialSolicitud
    INNER JOIN dbo.CIRI_Solicitudes AS s
        ON s.IdSolicitud = historial.IdSolicitud
    WHERE s.CodigoSolicitud = LTRIM(RTRIM(@PcodigoSolicitud_c))
      AND adjunto.Estado = 1
    ORDER BY
        adjunto.Fecha ASC,
        adjunto.IdAdjunto ASC;

END;
