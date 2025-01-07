using TCG.AuthenticationService.Domain;
using TCG.Common.Contracts;

namespace TCG.AuthenticationService.Application.Contracts;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetSub(Guid guid, CancellationToken cancellationToken);

    Task<User> GetByUserId(int id);
    Task<bool> CheckUserExist(string mail, Guid sub, string firstname, string lastname, string username);
}