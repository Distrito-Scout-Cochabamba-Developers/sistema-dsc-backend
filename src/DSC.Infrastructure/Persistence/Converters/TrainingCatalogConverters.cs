using DSC.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DSC.Infrastructure.Persistence.Converters;

public class TrainingLineIdConverter()
    : ValueConverter<TrainingLineId, short>(id => id.Value, value => new TrainingLineId(value));

public class TrainingLevelIdConverter()
    : ValueConverter<TrainingLevelId, short>(id => id.Value, value => new TrainingLevelId(value));
