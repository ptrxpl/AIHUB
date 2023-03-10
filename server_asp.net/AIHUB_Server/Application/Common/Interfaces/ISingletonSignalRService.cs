using AIHUB_Server.Domain.Services;
using System.Diagnostics;

namespace AIHUB_Server.Application.Interfaces
{
    public interface ISingletonSignalRService
    {
        SynchronizedCollection<Content> Processes { get; set; }
    }
}