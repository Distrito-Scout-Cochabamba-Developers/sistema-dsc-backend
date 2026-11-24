using System.Text.Json.Serialization;

namespace DSC.Application.DTOs;

/// <summary>Bloque de horario de una convocatoria (una fila de schedule).</summary>
public record class WriteTrainingModuleScheduleDto(
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    long? FacilityId = null,
    short PartNumber = 1,
    string? PartTitle = null
);

/// <summary>Alta de convocatoria ligada a un módulo del catálogo.</summary>
public record class WriteTrainingModuleSessionDto(
    IReadOnlyList<WriteTrainingModuleScheduleDto> Schedules,
    string? StatusCode = null,
    bool? IsRegistrationOpen = null
)
{
    private readonly int? capacity;

    public int? Capacity
    {
        get => capacity;
        init
        {
            capacity = value;
            HasCapacity = true;
        }
    }

    [JsonIgnore]
    internal bool HasCapacity { get; private init; }
}

public record class ExpandTrainingModuleSessionCapacityDto([property: JsonRequired] int? Capacity);
