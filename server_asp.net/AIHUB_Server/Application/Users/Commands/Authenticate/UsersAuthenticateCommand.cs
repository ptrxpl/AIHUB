using AIHUB_Server.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AIHUB_Server.Application.Users.Commands.Authenticate
{
    public class UsersAuthenticateCommand : IRequest<IActionResult>
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UsersAuthenticateCommandHandler : IRequestHandler<UsersAuthenticateCommand, IActionResult>
    {
        private IUserService _userService;

        public UsersAuthenticateCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Handle(UsersAuthenticateCommand command, CancellationToken cancellationToken)
        {
            UsersAuthenticateCommandResult response = _userService.Authenticate(command);

            if (response == null)
                return new BadRequestObjectResult(new { result = "Username or password is incorrect" });

            return new OkObjectResult(response);
        }
    }
}
