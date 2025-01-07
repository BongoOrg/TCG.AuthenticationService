using Microsoft.EntityFrameworkCore;
using TCG.AuthenticationService.Application.Contracts;
using TCG.AuthenticationService.Domain;
using TCG.Common.MySqlDb;

namespace TCG.AuthenticationService.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    protected readonly ServiceDbContext _dbContext;

    public UserRepository(ServiceDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User> GetSub(Guid guid, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.Where(x => x.Sub == guid).SingleOrDefaultAsync();
    }

    public async Task<User> GetByUserId(int idUser)
    {
        return await _dbContext.Users.Where(x => x.Id == idUser).SingleOrDefaultAsync();
    }

    public async Task<bool> CheckUserExist(string mail, Guid sub, string firstname, string lastname, string username)
    {
        return await _dbContext.Users.Where(user => user.Email == mail || user.Sub == sub || user.Firstname == firstname || user.Lastname == lastname || user.Username == username).FirstOrDefaultAsync() != null;
    }
}
