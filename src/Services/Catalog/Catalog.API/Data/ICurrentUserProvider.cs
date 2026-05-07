namespace Catalog.API.Data;

public interface ICurrentUserProvider
{
    string UserId { get; }
}
