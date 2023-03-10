using AIHUB_Server.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AIHUB_Server.Application.Experiments.Commands.Create
{
    public class ExperimentsUploadCommand : IRequest<IActionResult>
    {
        [Required]
        public List<IFormFile> Files { get; set; }
        public string name { get; set; }
        // TODO: add more properties if they'll be created in client.
    }

    public class ModulesCreateCommandHandler : IRequestHandler<ExperimentsUploadCommand, IActionResult>
    {
        private readonly IProjectFolderService _projectFolderService;

        public ModulesCreateCommandHandler(IProjectFolderService projectFolderService)
        {
            _projectFolderService = projectFolderService ?? throw new ArgumentNullException(nameof(projectFolderService));
        }

        public async Task<IActionResult> Handle(ExperimentsUploadCommand command, CancellationToken cancellationToken)
        {
            bool savedFiles = _projectFolderService.SaveFilesToFolder(command);

            var result = new ExperimentsUploadCommandResult();

            if (savedFiles)
            {
                result.Result = "Files correctly saved on the server.";
            }
            else
            {
                result.Result = "One of file is empty. Please fix your files.";
                return new BadRequestObjectResult(result);
            }

            return new OkObjectResult(result);
        }
    }
}
