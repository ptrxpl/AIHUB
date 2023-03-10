using AIHUB_Server.Domain.Services;
using AIHUB_Server.Application.Interfaces;
using System.Diagnostics;

namespace AIHUB_Server.Infrastructure.Services
{
    public class SingletonSignalRService : ISingletonSignalRService
    {
        public SynchronizedCollection<Content> Processes { get; set; }


        public SingletonSignalRService()
        {
            Processes = new();
            Processes.Add(new Content()
            {
                Process = new Process(),
                ProjectName = "CTOR",
                IsCmdDone = false,
                CmdFileOutputName = "FILEOUTPUT.txt",
                FileToRun = "ping wp.pl"
            });
        }
    }
}
