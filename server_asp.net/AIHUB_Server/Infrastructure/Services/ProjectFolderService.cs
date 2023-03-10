using AIHUB_Server.Application.Common.Interfaces;
using AIHUB_Server.Application.Experiments.Commands.Create;
using AIHUB_Server.Application.Experiments.Commands.Init;
using AIHUB_Server.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AIHUB_Server.Infrastructure.Services
{
    public class ProjectFolderService : IProjectFolderService
    {
        private readonly ServerSettings _serverSettings;

        public ProjectFolderService(IOptions<ServerSettings> serverSettings)
        {
            _serverSettings = serverSettings.Value;
        }

        public bool CreateProjectFolder(ExperimentsInitCommand query)
        {
            string path = _serverSettings.FolderLocation + query.Name;

            bool exists = Directory.Exists(path);

            if (!exists)
            {
                Directory.CreateDirectory(path);
                return true;
            }

            return false;
        }

        private string GetSafeFileName(string name, char replace = '_')
        {
            char[] invalids = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalids.Contains(c) ? replace : c).ToArray());
        }

        public bool SaveFilesToFolder(ExperimentsUploadCommand command)
        {
            if (command.Files == null)
            {
                return false;
            }

            foreach (IFormFile file in command.Files)
            {
                if (file.Length > 0)
                {
                    string fileName = "";
                    string folderPath = Path.Combine(_serverSettings.FolderLocation, command.name);

                    if (file.FileName.StartsWith("./"))
                    {
                        fileName = file.FileName.Remove(0, 2);
                    }

                    // File can be in folder, e.g. somedata/picture.jpg
                    // So create that folder
                    string folderName = Path.GetDirectoryName(fileName); 
                    if (folderName != null)
                    {
                        if (!Directory.Exists(Path.Combine(folderPath, folderName)))
                        {
                            Directory.CreateDirectory(Path.Combine(folderPath, folderName));
                        }
                    }

                    fileName = Path.GetFileName(fileName);

                    // TODO: compare given and new file name, if something changed - return it to client and inform
                    // It may happens that user will be not able to run project (e.g. wrong file names inside scripts).
                    string fileNameSafe = GetSafeFileName(fileName);

                    string filePath = Path.GetFullPath(Path.Combine(folderPath, folderName, fileNameSafe));

                    if(!Directory.Exists(Path.Combine(_serverSettings.FolderLocation, command.name)))
                    {
                        return false;
                    }

                    using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        file.CopyTo(fileStream);
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
