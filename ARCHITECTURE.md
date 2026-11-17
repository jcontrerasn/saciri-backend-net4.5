# Arquitectura

La aplicación Web API está organizada por capas:

- `Controllers`: recibe solicitudes HTTP y coordina la respuesta.
- `Services`: contiene los casos de uso y la lógica de negocio.
- `Interfaces`: define los contratos de servicios y repositorios.
- `Dtos`: define los modelos de entrada y salida de la API.
- `Entities`: contiene las entidades del dominio.
- `Repositories`: abstrae el acceso a datos.

## Flujo de una solicitud

`Controller -> Service -> Repository -> Entity`

Los controladores deben depender de interfaces de servicios, y los servicios
deben depender de interfaces de repositorios. Las implementaciones concretas
de persistencia se mantienen dentro de `Repositories`, evitando acoplar la
lógica de negocio a una tecnología de datos específica.

Los DTOs mantienen separado el contrato HTTP de las entidades del dominio.
`ApiResponse<T>` proporciona un envoltorio común para respuestas que lo
requieran.

## Endpoint implementado: solicitudes por flag

POST /api/solicitudes/flag recibe Usuario y Flag (CS, A o TMS) y sigue este
flujo:

SolicitudesController -> SolicitudService -> ISolicitudRepository ->
SqlSolicitudRepository -> pr_ciri_ListarSolicitudesXFiltro

La respuesta contiene una lista de SolicitudFlagResponse dentro de
ApiResponse<T>.

## Endpoint implementado: categorías por tipo de registro

GET /api/categorias/tipo-registro/{idTipoRegistro} valida el identificador y
consulta CIRI_Categorias junto con CIRI_TipoRegistros mediante
CategoriasController -> CategoriaService -> ICategoriaRepository ->
SqlCategoriaRepository.

POST /api/categorias crea una categoría con IdTipoRegistro, Nombre y
Descripcion y devuelve el registro creado con estado HTTP 201.

## Endpoint implementado: subcategorías por categoría

GET /api/subcategorias/categoria/{idCategoria} valida el identificador y
consulta dbo.CIRI_Subcategorias mediante
SubcategoriasController -> SubcategoriaService -> ISubcategoriaRepository ->
SqlSubcategoriaRepository.

## Endpoint implementado: ocurrencias por subcategoría

GET /api/ocurrencias/subcategoria/{idSubcategoria} valida el identificador y
consulta dbo.CIRI_Ocurrencias mediante
OcurrenciasController -> OcurrenciaService -> IOcurrenciaRepository ->
SqlOcurrenciaRepository.

## Endpoint implementado: centros de costo

POST /api/centros-costo recibe Pfiltro_c y consulta
pr_sm_ListarCentroCostoXFiltro mediante
CentrosCostoController -> CentroCostoService -> ICentroCostoRepository ->
SqlCentroCostoRepository.

## Endpoint implementado: búsqueda de usuarios

POST /api/usuarios/filtro recibe Pfiltro_c y consulta
pr_sm_ListarUsuarioXFiltro mediante UsuariosController -> UsuarioService ->
IUsuarioRepository -> SqlUsuarioRepository.

## Endpoint implementado: tipos de registro

GET /api/tipos-registros consulta dbo.CIRI_TipoRegistros mediante
TipoRegistrosController -> TipoRegistroService -> ITipoRegistroRepository ->
SqlTipoRegistroRepository y devuelve TipoRegistroResponse dentro de
ApiResponse<T>.

## Endpoint implementado

`GET /api/usuarios/cuenta/{cuenta}` sigue este flujo:

`UsuariosController -> UsuarioService -> IUsuarioRepository -> SqlUsuarioRepository -> pr_sm_BuscarUsuarioXCuenta`

El repositorio envía el parámetro `Pcuenta_n` y mapea las columnas `Usuario`,
`NombreCompleto` y `Area` a la entidad `Usuario`. El servicio transforma la
entidad en `UsuarioCuentaResponse` y el controlador la devuelve dentro de
`ApiResponse<T>`.
