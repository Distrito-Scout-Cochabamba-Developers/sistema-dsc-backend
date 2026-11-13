namespace DSC.Domain.ValueObjects;

public readonly record struct TrainingLevelId(short Value) : IComparable<TrainingLevelId>
{
    public static implicit operator TrainingLevelId(short value) => new(value);
    public static implicit operator TrainingLevelId(int value) => new((short)value);
    public static implicit operator short(TrainingLevelId id) => id.Value;
    public int CompareTo(TrainingLevelId other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString();
}
