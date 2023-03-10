using AIHUB_Server.Application.Experiments.Commands.Create;
using AIHUB_Server.Application.Experiments.Commands.Init;
using AIHUB_Server.Application.Experiments.Commands.Stop;
using AIHUB_Server.Common.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AIHUB_Server.Controllers
{
    public class ExperimentsController : ApiControllerBase
    {
        [Authorize]
        [HttpPost("Init")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExperimentsInitCommandResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ExperimentsInitCommandResult))]
        public async Task<IActionResult> ExperimentsInit(ExperimentsInitCommand query)
        {
            return await Mediator.Send(query);
        }

        [Authorize]
        [HttpPost("Upload")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExperimentsUploadCommandResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ExperimentsUploadCommandResult))]
        public async Task<IActionResult> ModulesCreate([FromForm] ExperimentsUploadCommand command)
        {
            return await Mediator.Send(command);
        }

        [Authorize]
        [HttpPost("Stop")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExperimentsStopCommandResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ExperimentsStopCommandResult))]
        public async Task<IActionResult> ModulesStop(ExperimentsStopCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
