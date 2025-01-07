using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TCG.AuthenticationService.Application.Contracts;
using TCG.AuthenticationService.Application.Keycloak.DTO.Response;
using TCG.AuthenticationService.Domain;
using TCG.CatalogService.Application.Keycloak.DTO.Request;
using TCG.Common.Middlewares.MiddlewareException;

namespace TCG.AuthenticationService.Application.Keycloak.Query;

public record GetUserProfileQuery(int idUser) : IRequest<UserProfileDtoResponse>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDtoResponse>
{
    private readonly IKeycloakRepository _keycloakService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IKeycloakRepository keycloakService, ILogger<GetUserProfileQueryHandler> logger, IUserRepository userRepository, IMapper mapper)
    {
        _keycloakService = keycloakService;
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<UserProfileDtoResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            //var userSub = await _keycloakService.GetUserInfoAsync(request.token.Token);
            var userInfo = await _userRepository.GetByIdAsync(request.idUser, cancellationToken);

            // Check if the user exists in the database
            if (userInfo == null)
            {
                // User does not exist in the database
                throw new NotFoundException("User does not exist in the database");
            }
            UserProfileDtoResponse userMaped = _mapper.Map<UserProfileDtoResponse>(userInfo);

            return userMaped;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "User not found in database");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting user");
            throw;
        }
    }
}
