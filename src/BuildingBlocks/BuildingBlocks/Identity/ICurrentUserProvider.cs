namespace BuildingBlocks.Identity;

public interface ICurrentUserProvider
{
    string UserId { get; }
}
