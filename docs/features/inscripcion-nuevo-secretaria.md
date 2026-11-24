# Alta e inscripción asistida

La creación del perfil mínimo y la inscripción comparten una transacción PostgreSQL READ COMMITTED. El cupo se reserva mediante la operación común de inscripción. No se crea cuenta ni se envía correo: la activación corresponde a AB#161.

Las escrituras asistidas serializan por número de CI con un bloqueo advisory transaccional y consultan nuevamente la identidad. El alta rechaza cualquier coincidencia, aun con otro complemento; la inscripción existente requiere una coincidencia única para el CI y complemento enviados y verifica el identificador. El bloqueo solo protege escritores que adopten el mismo protocolo: asistencia digital y otros escritores históricos aún no lo utilizan. No se modifica la unicidad global ni se corrigen datos históricos ambiguos; coordinar esos escritores antes de afirmar unicidad global.
