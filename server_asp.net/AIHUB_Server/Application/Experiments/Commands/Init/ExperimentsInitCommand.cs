using AIHUB_Server.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AIHUB_Server.Application.Experiments.Commands.Init
{
    public class ExperimentsInitCommand : IRequest<IActionResult>
    {
        public string Name { get; set; }
    }

    public class ExperimentsInitQueryHandler : IRequestHandler<ExperimentsInitCommand, IActionResult>
    {
        private readonly IProjectFolderService _projectFolderService;

        public ExperimentsInitQueryHandler(IProjectFolderService projectFolderService)
        {
            _projectFolderService = projectFolderService ?? throw new ArgumentNullException(nameof(projectFolderService));
        }

        public async Task<IActionResult> Handle(ExperimentsInitCommand command, CancellationToken cancellationToken)
        {
            bool createdFolder = _projectFolderService.CreateProjectFolder(command);

            var result = new ExperimentsInitCommandResult();

            if (createdFolder == true)
            {
                result.Result = "Initial folder created on the server.";
            }
            if (createdFolder == false)
            {
                result.Result = "Folder with given name actually exists on the server.";
                return new BadRequestObjectResult(result);
            }

            return new OkObjectResult(result);
        }
    }
}
