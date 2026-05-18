namespace BuildingBlocks.Authentication;

public interface IInternalServiceTokenProvider
{
    string CreateToken(IEnumerable<string> scopes);
}
