namespace DSC.Domain.Exceptions;

public sealed class CapacityPermissionException : Exception
{
    public CapacityPermissionException()
        : base("Solo ADMIN puede ampliar el cupo o establecerlo sin límite.") { }
}
