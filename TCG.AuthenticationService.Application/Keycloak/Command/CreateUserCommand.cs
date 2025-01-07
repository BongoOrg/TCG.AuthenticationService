using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TCG.AuthenticationService.Application.Contracts;
using TCG.AuthenticationService.Domain;
using TCG.AuthenticationService.Domain.Enums;
using TCG.CatalogService.Application.Keycloak.DTO.Request;
using TCG.Common.Middlewares.MiddlewareException;

namespace TCG.AuthenticationService.Application.Keycloak.Command;

public record CreateUserCommand(UserRegistration UserRegistration) : IRequest;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IKeycloakRepository _keycloakService;
    private readonly ILogger _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IKeycloakRepository keycloakService, ILogger<CreateUserCommandHandler> logger, IUserRepository userRepository, IMapper mapper)
    {
        _keycloakService = keycloakService;
        _logger = logger;
        _userRepository = userRepository;
        _mapper = mapper;
    }
    public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await _keycloakService.GetAdminAccessTokenAsync();

            //Verifie si l'utilisateur existe en base
            var userExist = await _userRepository.CheckUserExist(request.UserRegistration.Email, request.UserRegistration.Sub, request.UserRegistration.Firstname, request.UserRegistration.Lastname, request.UserRegistration.Username);
            if (userExist)
            {
                throw new UserAlreadyExistsException($"Error in {nameof(CreateUserCommand)}: User already exist in database");
            }
            
            //Creer l'utilisateur dans Keycloak
            await _keycloakService.CreateUserAsync(accessToken, request.UserRegistration);
            
            //Recupère l'id sub de l'utilisateur cree dans Keycloak
            var userSub = await _keycloakService.GetUserIdAsync(accessToken, request.UserRegistration.Username);
            request.UserRegistration.Sub = Guid.Parse(userSub);

            try
            {
                //Ajoute l'utilisateur en base
                request.UserRegistration.UserStateId = (int)UserStates.Created;
                await _userRepository.AddAsync(_mapper.Map<User>(request.UserRegistration), cancellationToken);
                _logger.LogInformation("User has been created in Keycloak and database ...");
            }
            catch (Exception e)
            {
                //Gère la compensation si l'insertion en base échoue
                await _keycloakService.DeleteUserAsync(accessToken, userSub);
                _logger.LogError("Failed to create user in database. Keycloak user has been rolled back.");
                throw;
            }
        }
        catch (UserAlreadyExistsException e)
        {
            throw;
        }
        catch (Exception e)
        {
            var errorMessage = $"Error in {nameof(CreateUserCommand)}: {e.Message}";
            throw new Exception(errorMessage,e);
        }
        
        return Unit.Value;
    }
}