using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence;

/// <summary>
/// Contexto principal de Entity Framework Core para la base de datos del Distrito Scout Cochabamba.
/// </summary>
public class ScoutDbContext(DbContextOptions<ScoutDbContext> options) : DbContext(options)
{
    // 1. Archivos y Almacenamiento
    public DbSet<FileAsset> Files => Set<FileAsset>();

    // 2. Mecanismos de Contacto y Ubicaciones
    public DbSet<ContactMechanismType> ContactMechanismTypes => Set<ContactMechanismType>();
    public DbSet<ContactPurposeType> ContactPurposeTypes => Set<ContactPurposeType>();
    public DbSet<FacilityType> FacilityTypes => Set<FacilityType>();
    public DbSet<Facility> Facilities => Set<Facility>();

    // 3. Estructura Organizacional y Parties
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ScoutGroup> ScoutGroups => Set<ScoutGroup>();
    public DbSet<ScoutGroupContactMechanism> ScoutGroupContactMechanisms =>
        Set<ScoutGroupContactMechanism>();
    public DbSet<ScoutPosition> ScoutPositions => Set<ScoutPosition>();
    public DbSet<PersonStatus> PersonStatuses => Set<PersonStatus>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PersonContactMechanism> PersonContactMechanisms => Set<PersonContactMechanism>();
    public DbSet<ScoutAffiliation> ScoutAffiliations => Set<ScoutAffiliation>();

    // 4. Identidad Digital y Autorización
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // 5. Catálogo Curricular y Plantillas Maestras
    public DbSet<ModuleType> ModuleTypes => Set<ModuleType>();
    public DbSet<TrainingStage> TrainingStages => Set<TrainingStage>();
    public DbSet<TrainingLevel> TrainingLevels => Set<TrainingLevel>();
    public DbSet<TrainingModule> TrainingModules => Set<TrainingModule>();
    public DbSet<TrainingModuleLessonPlanTemplate> TrainingModuleLessonPlanTemplates =>
        Set<TrainingModuleLessonPlanTemplate>();
    public DbSet<TrainingModuleLessonPlanTemplateActivity> TrainingModuleLessonPlanTemplateActivities =>
        Set<TrainingModuleLessonPlanTemplateActivity>();

    // 6. Convocatorias, Sesiones y Trazabilidad de Estados
    public DbSet<TrainingModuleSessionStatus> TrainingModuleSessionStatuses =>
        Set<TrainingModuleSessionStatus>();
    public DbSet<TrainingModuleSession> TrainingModuleSessions => Set<TrainingModuleSession>();
    public DbSet<TrainingModuleSessionStatusHistory> TrainingModuleSessionStatusHistories =>
        Set<TrainingModuleSessionStatusHistory>();
    public DbSet<TrainingModuleSchedule> TrainingModuleSchedules => Set<TrainingModuleSchedule>();
    public DbSet<TrainingModuleInstructor> TrainingModuleInstructors =>
        Set<TrainingModuleInstructor>();

    // 7. Planificación Específica de la Sesión
    public DbSet<TrainingModuleLessonPlan> TrainingModuleLessonPlans =>
        Set<TrainingModuleLessonPlan>();
    public DbSet<TrainingModuleLessonPlanActivity> TrainingModuleLessonPlanActivities =>
        Set<TrainingModuleLessonPlanActivity>();

    // 8. Asistencia y Evaluaciones
    public DbSet<EnrollmentSource> EnrollmentSources => Set<EnrollmentSource>();
    public DbSet<TrainingModuleEnrollment> TrainingModuleEnrollments =>
        Set<TrainingModuleEnrollment>();
    public DbSet<EvaluatorType> EvaluatorTypes => Set<EvaluatorType>();
    public DbSet<EvaluationCriterion> EvaluationCriteria => Set<EvaluationCriterion>();
    public DbSet<TrainingModuleEvaluation> TrainingModuleEvaluations =>
        Set<TrainingModuleEvaluation>();
    public DbSet<TrainingModuleEvaluationDetail> TrainingModuleEvaluationDetails =>
        Set<TrainingModuleEvaluationDetail>();

    // 9. Perfil del Dirigente y Kárdex
    public DbSet<ScoutLeader> ScoutLeaders => Set<ScoutLeader>();
    public DbSet<LeaderTrainingStatus> LeaderTrainingStatuses => Set<LeaderTrainingStatus>();
    public DbSet<ScoutLeaderTrainingLevel> ScoutLeaderTrainingLevels =>
        Set<ScoutLeaderTrainingLevel>();

    // Legacy Support (si existen consultas previas)
    public DbSet<ModuleAttendance> ModuleAttendances => Set<ModuleAttendance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScoutDbContext).Assembly);
    }
}
