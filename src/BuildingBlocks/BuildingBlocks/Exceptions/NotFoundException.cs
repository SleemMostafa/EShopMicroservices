namespace BuildingBlocks.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public NotFoundException(string name, object key)
        : this($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
