using MapsterMapper;
using MediatR;
using TCG.AuthenticationService.Application.Contracts;

namespace TCG.AuthenticationService.Application.Keycloak.Query;

public record CheckUserExistQuery(string mailUser, Guid Sub, string firstname, string lastname, string username) : IRequest<bool>;

public class CheckUserExistQueryHandler : IRequestHandler<CheckUserExistQuery, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CheckUserExistQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<bool> Handle(CheckUserExistQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _userRepository.CheckUserExist(request.mailUser, request.Sub, request.firstname, request.lastname, request.username);
        }
        catch (Exception e)
        {
            var errorMessage = $"Error in {nameof(CheckUserExistQueryHandler)}: {e.Message}";
            throw new Exception(errorMessage,e);
        }
    }
}