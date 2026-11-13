namespace DSC.Domain.ValueObjects;

public readonly record struct TrainingLineId(short Value) : IComparable<TrainingLineId>
{
    public static implicit operator TrainingLineId(short value) => new(value);

    public static implicit operator TrainingLineId(int value) => new((short)value);

    public static implicit operator short(TrainingLineId id) => id.Value;

    public int CompareTo(TrainingLineId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
