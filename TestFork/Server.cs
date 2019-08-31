using ServiceUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFork
{
    public class Server : IService
    {
        private Process _process;

        public void OnStart(string[] args)
        {
            File.AppendAllText(@"c:\tmp\start.log", "Start\n");
            try
            {
                var p = new ProcessStartInfo();
                p.FileName = @"G:\my_projects\TestFork\TestFork\bin\Debug\TestFork.exe";
                p.Arguments = "client_null";
                p.UseShellExecute = false;
                p.RedirectStandardOutput = true;
                p.RedirectStandardError = true;
                p.RedirectStandardInput = true;
                _process = Process.Start(p);

                File.AppendAllText(@"c:\tmp\start.log", $"OnStart child pid={_process.Id}\n");                
            }
            catch (Exception e)
            {
                File.WriteAllText(@"c:\tmp\error.log", e.Message);
            }
        }

        public void OnStop()
        {
            if (!_process.HasExited)
                _process.Kill();

        }
    }
}
