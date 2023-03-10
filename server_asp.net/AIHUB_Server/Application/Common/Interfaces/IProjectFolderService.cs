using AIHUB_Server.Application.Experiments.Commands.Create;
using AIHUB_Server.Application.Experiments.Commands.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHUB_Server.Application.Common.Interfaces
{
    public interface IProjectFolderService
    {
        bool CreateProjectFolder(ExperimentsInitCommand command);
        bool SaveFilesToFolder(ExperimentsUploadCommand command);
    }
}
