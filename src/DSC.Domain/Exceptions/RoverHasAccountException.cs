namespace DSC.Domain.Exceptions;

/// <summary>La modalidad Rover sin cuenta no admite personas con cuenta existente.</summary>
public sealed class RoverHasAccountException() : Exception("La persona ya tiene una cuenta.");
