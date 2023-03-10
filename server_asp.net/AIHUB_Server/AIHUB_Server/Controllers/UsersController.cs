using AIHUB_Server.Application.Experiments.Commands.Stop;
using AIHUB_Server.Application.Users.Commands.Authenticate;
using Microsoft.AspNetCore.Mvc;

namespace AIHUB_Server.Controllers
{
    public class UsersController : ApiControllerBase
    {
        [HttpPost("Authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsersAuthenticateCommandResult))]
        public async Task<IActionResult> Authenticate(UsersAuthenticateCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
