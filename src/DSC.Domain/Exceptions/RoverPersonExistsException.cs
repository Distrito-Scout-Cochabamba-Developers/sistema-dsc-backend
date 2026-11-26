namespace DSC.Domain.Exceptions;

/// <summary>Ya existe una persona con el Carnet de Identidad indicado; el alta de Rover nuevo no la reutiliza ni la duplica.</summary>
public sealed class RoverPersonExistsException() : Exception("Ya existe una persona con el Carnet de Identidad indicado.");
