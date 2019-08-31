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
            File.AppendAllText(@"c:\tmp\start.log", "Start");
            try
            {
                var p = new ProcessStartInfo();
                p.FileName = @"G:\my_projects\TestFork\TestFork\bin\Debug\TestFork.exe";
                p.Arguments = "client_null";
                p.UseShellExecute = false;
                p.CreateNoWindow = true;
                _process = Process.Start(p);

                File.AppendAllText(@"c:\tmp\start.log", $"OnStart child pid={_process.Id}");                
            }
            catch (Exception e)
            {
                File.WriteAllText(@"c:\tmp\error.log", e.Message);
            }
        }

        public void OnStop()
        {
            _process.Kill();
        }
    }
}
