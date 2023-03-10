using AIHUB_Server.Application.Common.Interfaces;
using AIHUB_Server.Application.Interfaces;
using AIHUB_Server.Common.Helpers;
using AIHUB_Server.Domain.Services;
using AIHUB_Server.Infrastructure.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace AIHUB_Server.Controllers
{
    //[SignalRHub] // Can be used for Swagger
    public class SignalRCmd : Hub
    {
        // Services
        public ISingletonSignalRService _singletonSignalRService { get; set; }
        public ServerSettings _serverSettings { get; }
        public IUserService _userService { get; }

        // Variables
        private const string newline = "\n";    // On Windows. In Linux should be (probably) \r\n
        private const string exit = " && exit"; // Very important - without it, process.WaitForExit() never happens, because cmd stays opened forever.
        private static StringBuilder cmdOutput = null;

        public SignalRCmd(
            ISingletonSignalRService singletonSignalRService,
            IOptions<ServerSettings> serverSettings, 
            IUserService userService)
        {
            _singletonSignalRService = singletonSignalRService;
            _serverSettings = serverSettings.Value;
            _userService = userService;
        }

        public override Task OnConnectedAsync()
        {
            // Start with timer
            _singletonSignalRService.Processes.Add(new Content() {
                ConnectionId = Context.ConnectionId,
                StopwatchTimer = new Stopwatch()
            });

            // Start timer
            var run = _singletonSignalRService.Processes.Last();
            run.StopwatchTimer.Start();

            HttpContext httpContext = Context.GetHttpContext();
            bool accessTokenOk = JwtMiddleware.InvokeInSignalR(httpContext, _userService);
            if (accessTokenOk == false)
            {
                Context.Abort(); // Disconnect unauthorized user
            }

            var connectionId = Context.ConnectionId;
            var projectName = Context.GetHttpContext().Request.Query["projectName"];

            Console.WriteLine("OnConnectedAsync before return");
            Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
            // Multiple "users" (e.g. somebody opened client applicaion 2 times) connected to the same group (as projectName) - can listen output.
            return Groups.AddToGroupAsync(connectionId, projectName); 
        }

        public string Stream(string projectName, string command, double howMuchRAM, bool runAgain)
        {
            string endResult = Stream(projectName, command, howMuchRAM, runAgain, o =>
            {
                Clients.Group(projectName).SendAsync("OutputRecieved", o);
            });

            return endResult;
        }

        private string Stream(string projectName, string fileToRun, double howMuchVRAM, bool runAgain, Action<string> outputHandler)
        {
            string projectNameFolder = Path.GetFullPath(Path.Combine(_serverSettings.FolderLocation, projectName));
            var dateTimeFormatted = DateTime.Now.ToString("yyyy-mm-dd-HH-mm-ss");
            string cmdOutputFile = Path.GetFullPath(Path.Combine(projectNameFolder, $"aihub_output_{projectName}.txt")); // This should be returned when everythigs done.
            bool enoughVRAM;
            double freeVRAM;
            string returnMessageStreamEnd = "[INFO] Stream end.";

            // Is there any of processName in list?
            var run = _singletonSignalRService.Processes.FirstOrDefault(x => x.ProjectName == projectName);

            // There is given process already, check if it is still running
            if (run != null)
            {
                run.StopwatchTimer.Start(); // run again
                Console.WriteLine("After first run - logic start");
                Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());

                // Script is done.
                // Even if we run again, we shouldn't be there, because IsCmdDone == false when cmd is running.
                if (run.IsCmdDone == true)
                {
                    // Return logs
                    if (runAgain == false)
                    {
                        using (StreamReader sr = new(run.CmdFileOutputName))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                outputHandler(line);
                            }
                        }

                        Console.WriteLine("After first run - script done, read logs from file");
                        Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
                        Console.WriteLine("============================================");
                        run.StopwatchTimer.Reset();
                        return returnMessageStreamEnd;
                    }

                    // Update some info and run it again!
                    if (runAgain == true)
                    {
                        Console.WriteLine("After first run - run again start logic");
                        Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());

                        // Set again
                        run.IsCmdDone = false;
                        run.RunAgain = false;

                        // But first check if we have enough VRAM
                        (enoughVRAM,  freeVRAM) = EnoughVRAM(howMuchVRAM);
                        if (enoughVRAM == false)
                        {
                            outputHandler($"[ERROR] You tried to set too much VRAM. Actual free is: {freeVRAM}");
                            return returnMessageStreamEnd;
                        }                        

                        // Clear old output
                        using (var sw = new StreamWriter(run.CmdFileOutputName, false))
                        {
                            sw.WriteLine($"AIHUB total \"python.exe -u {fileToRun}\" output in project: {projectName}");
                            sw.WriteLine($"Started at (timestamp): {DateTime.Now}");
                        }

                        // Create process again
                        var processRunAgain = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = _serverSettings.Python310Path,
                                Arguments = "-u " + fileToRun, // -u means unbuffered output, otherwise we had to import sys; sys.stdout.flush() in python, which is not nice
                                RedirectStandardOutput = true,
                                RedirectStandardInput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                WorkingDirectory = projectNameFolder,
                            }
                        };

                        processRunAgain.OutputDataReceived += new DataReceivedEventHandler(
                          (sendingProcess, outLine) =>
                          {
                              outputHandler(outLine.Data);
                              PythonOutputHandler(sendingProcess, outLine, cmdOutputFile);
                          }
                        );

                        processRunAgain.ErrorDataReceived += new DataReceivedEventHandler(
                          (sendingProcess, outLine) =>
                          {
                              outputHandler(outLine.Data);
                              PythonOutputHandler(sendingProcess, outLine, cmdOutputFile);
                          }
                        );

                        run.Process = processRunAgain;

                        run.Process.Start();
                        Console.WriteLine("After first run - run again process started");
                        Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
                        run.Process.BeginOutputReadLine();
                        run.Process.BeginErrorReadLine();

                        run.Process.WaitForExit(); // Thread stays there and waits.

                        // If done, change it to done
                        if (run.Process.HasExited)
                        {
                            run.IsCmdDone = true;
                            run.RunAgain = false;
                            Console.WriteLine("After first run - process has exited");
                            Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
                            Console.WriteLine("============================================");
                            run.StopwatchTimer.Reset();
                        }

                        return returnMessageStreamEnd;
                    }
                }

                // Script still running
                if (run.IsCmdDone == false)
                {
                    // It is already running in different thread. Just wait for exit and keep streaming.
                    // Also prevent from instant return from function and sending fake "Stream end", when there is still process going.
                    // Output will be captured because of SignalR and thread where it initially run, and Groups (in new conection, you are still connected to the same group).
                    run.Process.WaitForExit(); // As mentioned above, tThread stays there and waits. Even if you connect from different thread, then we just manually wait again (but in fact, that's no issue)
                    return returnMessageStreamEnd;
                }
            }

            // If run == null, just make regular process and things below:

            // 1. Check VRAM
            (enoughVRAM, freeVRAM) = EnoughVRAM(howMuchVRAM);
            if (enoughVRAM == false)
            {
                outputHandler($"[ERROR] You tried to set too much VRAM. Actual free is: {freeVRAM}");
                return returnMessageStreamEnd;
            }

            // Create text file if doesn't exist in projectName folder
            using (var sw = new StreamWriter(cmdOutputFile, false))
            {
                sw.WriteLine($"AIHUB total \"python.exe -u {fileToRun}\" output in project: {projectName}");
                sw.WriteLine($"Started at (timestamp): {DateTime.Now}");
            }

            // 2. Create process
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _serverSettings.Python310Path, // TODO: In future keep path to Anaconda - different ennvironments etc.
                    Arguments = "-u " + fileToRun, // -u means unbuffered output, otherwise we had to import sys; sys.stdout.flush() in python, which is not nice
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = projectNameFolder,
                }
            };

            process.OutputDataReceived += new DataReceivedEventHandler(
                (sendingProcess, outLine) =>
                {
                    outputHandler(outLine.Data);
                    PythonOutputHandler(sendingProcess, outLine, cmdOutputFile);
                }
            );

            process.ErrorDataReceived += new DataReceivedEventHandler(
                (sendingProcess, outLine) =>
                {
                    outputHandler(outLine.Data);
                    PythonOutputHandler(sendingProcess, outLine, cmdOutputFile);
                }
            );

            // 3. and 4. Find by connection id and add
            run = _singletonSignalRService.Processes.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            // TODO: database instead of singleton
            run.Process = process;
            run.IsCmdDone = false;
            run.ProjectName = projectName;
            run.CmdFileOutputName = cmdOutputFile;
            run.FileToRun = fileToRun; // Just maybe for the future, now not used (already in arguments)
            run.RunAgain = false; // run again if true, default false means after cmd done, return logs. If RunAgain = true, run all things again

            // 5. Get last added
            run = _singletonSignalRService.Processes.Last(); 

            // 6. And run it
            run.Process.Start();
            Console.WriteLine("First run - process started");
            Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
            run.Process.BeginOutputReadLine();
            run.Process.BeginErrorReadLine();
            
            run.Process.WaitForExit(); // Thread stays there and waits. Even if you connect from different thread, then we just manually wait again (but in fact, that's no issue)

            // If done, change it to done
            if (run.Process.HasExited)
            {
                run.IsCmdDone = true;
                Console.WriteLine("First run - process has exited");
                Console.WriteLine(run.StopwatchTimer.Elapsed.ToString());
                Console.WriteLine("============================================");
                run.StopwatchTimer.Reset();
            }

            return "[INFO] Stream end.";
        }

        private (bool, double) EnoughVRAM(double howMuchVRAM)
        {
            string freeVRAMString = CheckNvidiaSMI();
            string[] freeVRAMLines = freeVRAMString.Replace("\r", "").Split('\n');

            // For safety, just substract some saveVRAM.
            // It is in MiB.
            double freeVRAM = Convert.ToDouble(freeVRAMLines.Last()) - _serverSettings.SaveVRAM;

            if (howMuchVRAM > freeVRAM)
            {
                return (false, freeVRAM);
            }

            return (true, freeVRAM);
        }

        private string CheckNvidiaSMI()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    //WorkingDirectory // is no needed, doesn't matter where we run nvidia-smi
                }
            };

            cmdOutput = new StringBuilder(); // It doesn't create a new string everytime, boost and RAM performance

            process.OutputDataReceived += new DataReceivedEventHandler(
              (sendingProcess, outLine) =>
                  CmdOutputHandler(sendingProcess, outLine, cmdOutput)
            );

            process.ErrorDataReceived += new DataReceivedEventHandler(
              (sendingProcess, outLine) =>
                  CmdOutputHandler(sendingProcess, outLine, cmdOutput)
            );

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.StandardInput.Write(_serverSettings.NvidiaSmiFreeVRAM + exit + newline);
            process.WaitForExit();

            return cmdOutput.ToString();
        }

        private static void CmdOutputHandler(object sendingProcess,
           DataReceivedEventArgs outLine, StringBuilder sortOutput)
        {
            // Collect the cmd.exe output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                sortOutput.Append(Environment.NewLine + $"{outLine.Data}");                                
            }
        }

        private static void PythonOutputHandler(object sendingProcess, DataReceivedEventArgs outLine, string filePath)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                if (File.Exists(filePath))
                {
                    using (var sw = new StreamWriter(filePath, true))
                    {
                        sw.WriteLine(outLine.Data);
                    }
                }
            }
        }
    }
}
