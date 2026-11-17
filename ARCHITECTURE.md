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

## Endpoint implementado

`GET /api/usuarios/cuenta/{cuenta}` sigue este flujo:

`UsuariosController -> UsuarioService -> IUsuarioRepository -> SqlUsuarioRepository -> pr_sm_BuscarUsuarioXCuenta`

El repositorio envía el parámetro `Pcuenta_n` y mapea las columnas `Usuario`,
`NombreCompleto` y `Area` a la entidad `Usuario`. El servicio transforma la
entidad en `UsuarioCuentaResponse` y el controlador la devuelve dentro de
`ApiResponse<T>`.
