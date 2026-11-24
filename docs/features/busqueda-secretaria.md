# Búsqueda por CI para Secretaría

`POST /api/v1/secretariat/participants/lookup` con `Content-Type: application/json`:

```json
{ "ci": "5556667", "extension": "CB" }
```

Requiere JWT con rol `DISTRICT_SECRETARY`. Es una consulta de solo lectura: no crea perfiles ni inscripciones. Busca personas, incluidos Rovers sin cuenta.
Se usa POST para no enviar datos personales en la URL. No existe variante GET. La convocatoria seleccionada es contexto de la UI: esta consulta de personas no toma `sessionId` ni lee o cambia inscripciones.

Todas las respuestas de la ruta, incluidos errores, se marcan `Cache-Control: no-store`. No habilitar captura de cuerpos de solicitud/respuesta ni parámetros SQL en logs, proxy o telemetría de este flujo. No registrar CI, celular ni perfiles; para fallos inesperados la API registra solo el tipo de excepción. Verificar también la configuración externa antes de usar datos reales.

- `ci`: obligatorio, de 4 a 10 dígitos. `extension`: opcional, código de departamento válido.
- `200`: devuelve `personId`, `firstName`, `lastName`, `ciNumber`, `extension`, `phone`, `hasAccount`, `isScoutLeader` e `isProfileComplete`.
- `404` con `type: urn:dsc:secretariat:participant-not-found`: no hay coincidencia con los criterios enviados. No habilita un alta.
- `409`: existen varias coincidencias y se necesita la extensión. Si la extensión sigue sin identificar una persona, no se debe crear ni asociar un perfil automáticamente.
- `400`: CI o extensión inválidos. `401`/`403`: falta autenticación o el rol de Secretaría. `500`: fallo inesperado, sin exponer información de la excepción.

Si se omite la extensión y hay una sola coincidencia, se devuelve esa persona. Si se proporciona, debe coincidir, sin distinguir mayúsculas; nunca se selecciona otra extensión. Se devuelve el celular vigente primario o, en su defecto, el vigente más reciente; si no hay, se devuelve vacío. `hasAccount` indica si existe una cuenta, no si está activa.

La inscripción, la creación persistente de perfiles y las invitaciones **no están implementadas** en este flujo: dependen de la operación compartida de inscripción con cupo atómico y del flujo de acceso.
