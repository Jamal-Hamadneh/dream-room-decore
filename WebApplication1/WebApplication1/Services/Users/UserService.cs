using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Users;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IUserService : ICrudService<UserRequest, UserResponse>;

public class UserService(IUserRepository repository, ICrudMapper<User, UserRequest, UserResponse> mapper, ApplicationDbContext context)
    : CrudService<User, UserRequest, UserResponse>(repository, mapper), IUserService
{
    public override async Task<UserResponse> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        await ThrowIfEmailExistsAsync(request.Email, null, cancellationToken);
        return await base.CreateAsync(request, cancellationToken);
    }

    public override async Task<UserResponse?> UpdateAsync(int id, UserRequest request, CancellationToken cancellationToken = default)
    {
        await ThrowIfEmailExistsAsync(request.Email, id, cancellationToken);
        return await base.UpdateAsync(id, request, cancellationToken);
    }

    private async Task ThrowIfEmailExistsAsync(string email, int? currentUserId, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await context.Users.AnyAsync(user => user.Email == normalizedEmail && user.Id != currentUserId, cancellationToken);
        if (exists)
        {
            throw new UserEmailAlreadyExistsException(normalizedEmail);
        }
    }
}
