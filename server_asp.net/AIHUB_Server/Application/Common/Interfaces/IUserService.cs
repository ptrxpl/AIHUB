using AIHUB_Server.Application.Users.Commands.Authenticate;
using AIHUB_Server.Domain.Entities.Users;
using Microsoft.AspNetCore.Http;

namespace AIHUB_Server.Application.Common.Interfaces
{
    public interface IUserService
    {
        UsersAuthenticateCommandResult Authenticate(UsersAuthenticateCommand model);
        User GetById(int id);
        bool AttachUserToContext(HttpContext context, IUserService userService, string token);
    }
}
