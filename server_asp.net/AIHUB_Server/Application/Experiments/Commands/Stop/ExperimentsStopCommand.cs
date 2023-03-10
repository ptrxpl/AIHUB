using AIHUB_Server.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AIHUB_Server.Application.Experiments.Commands.Stop
{
    public class ExperimentsStopCommand : IRequest<IActionResult>
    {
        [Required]
        public string name { get; set; }
    }

    public class ModulesStopCommandHandler : IRequestHandler<ExperimentsStopCommand, IActionResult>
    {
        private ISingletonSignalRService _singletonSignalRService;

        public ModulesStopCommandHandler(ISingletonSignalRService singletonSignalRService)
        {
            _singletonSignalRService = singletonSignalRService;
        }

        public async Task<IActionResult> Handle(ExperimentsStopCommand command, CancellationToken cancellationToken)
        {
            var run = _singletonSignalRService.Processes.FirstOrDefault(x => x.ProjectName == command.name);

            var result = new ExperimentsStopCommandResult();

            if (run == null)
            {
                result.Result = "There was no process running.";
                return new BadRequestObjectResult(result);
            }

            if (run != null)
            {
                run.Process.Kill();
                result.Result = "Process killed.";
            }

            return new OkObjectResult(result);
        }
    }
}
