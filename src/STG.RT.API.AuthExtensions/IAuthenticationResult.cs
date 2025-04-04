namespace STG.RT.API.AuthExtensions
{
    public interface IAuthenticationResult
    {
        string RefreshToken { get; }
        string UserName { get; }
        string UserDisplayName { get; }
    }
}
