using TCG.AuthenticationService.Domain;
using TCG.CatalogService.Application.Keycloak.DTO.Request;

namespace TCG.AuthenticationService.Application.Contracts;

public interface IKeycloakRepository
{
    Task<string> GetAdminAccessTokenAsync();
    Task<string> AuthenticateUserAsync(UserLogin userLogin);
    Task<Guid> GetUserInfoAsync(string accessToken);
    Task<string> GetUserIdAsync(string accessToken, string username);
    Task CreateUserAsync(string accessToken, UserRegistration userRegistration);
    Task DeleteUserAsync(string accessToken, string userId);
}