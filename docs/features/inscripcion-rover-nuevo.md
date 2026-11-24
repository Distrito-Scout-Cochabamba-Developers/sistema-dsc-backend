# Inscripción asistida de un Rover nuevo (AB#160)

`POST /api/v1/secretariat/sessions/{sessionId}/rover-enrollments` (rol `DISTRICT_SECRETARY`).

Cuerpo: `ci`, `extension` (opcional), `firstName`, `lastName`, `phone` (opcional). El operador sale del JWT.

- Persona e inscripción se guardan en una sola transacción. Si falla (cupo, ventana cerrada, duplicado, convocatoria inexistente) no queda perfil, contacto ni inscripción.
- Crea solo una persona activa y, si hay celular, un contacto personal. No crea cuenta, dirigente, afiliación, rama ni rol; la inscripción queda con modalidad Rover sin cuenta.
- Si ya existe cualquier persona con ese CI responde 409 `urn:dsc:enrollment:person-exists`; no se reutiliza ni se duplica. Para una persona existente se usa la confirmación de #157/#159 tras la búsqueda.
- Altas simultáneas del mismo CI se serializan con un bloqueo asesor de transacción por CI.
- Otros 409: `session-full`, `enrollment-closed`, `already-enrolled`. Validación: 400 con mensaje fijo sin CI ni celular.
- Los catálogos `person_status ACTIVE`, `contact_mechanism_type CELLPHONE` y `contact_purpose_type PERSONAL` se crean de forma idempotente si faltan, sin migración (para no chocar con #158).
