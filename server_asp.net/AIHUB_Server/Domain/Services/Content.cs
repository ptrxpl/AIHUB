using System.Diagnostics;

namespace AIHUB_Server.Domain.Services
{
    public class Content
    {
        public Process Process { get; set; }
        public string ProjectName { get; set; }
        public bool IsCmdDone { get; set; }
        public string CmdFileOutputName { get; set; }
        public string FileToRun { get; set; }
        public bool RunAgain { get; set; }
        public Stopwatch StopwatchTimer { get; set; }
        public string ConnectionId { get; set; }
    }
}
