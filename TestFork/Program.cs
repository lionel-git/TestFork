using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ServiceUtils;
using System.Collections.Concurrent;
using System.ServiceProcess;
using log4net;

namespace TestFork
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("Program");

        static void Server()
        {
            Console.WriteLine($"From Server pid = {Process.GetCurrentProcess().Id}");
            var processes = new List<Process>();
            for (int i = 0; i < 3; i++)
            {
                var p = new ProcessStartInfo();
                p.FileName = "TestFork.exe";
                p.Arguments = "client";
                p.UseShellExecute = false;                
                var process = Process.Start(p);
                processes.Add(process);
                Console.WriteLine($"Started pid={process.Id}");
                Thread.Sleep(500);
            }

            Console.WriteLine("Test Exit");
            Environment.Exit(0);

            int count = 0;
            do
            {
                Thread.Sleep(500);
                count += 500;
                foreach (var process in processes)
                {
                    Console.WriteLine($" {process.Id} => {  process.HasExited}");
                }
            }
            while (count < 15000);
        }

        static void Client()
        {
            int pid = Process.GetCurrentProcess().Id;
            string log = $@"c:\tmp\child_{pid}.log";
            File.AppendAllText(log, $"From Client pid = {pid}\n");

            Console.WriteLine($"From Client with pid={pid}");

            var procs = Process.GetProcesses();
            foreach (var proc in procs)
            {
                try
                {
                    //File.AppendAllText(log, $"proc= {proc.Id} {proc.ProcessName}\n");
                    var parentP = ParentProcessUtilities.GetParentProcess(proc.Handle);
                    //File.AppendAllText(log, $"zParent = {parentP.Id} {parentP.ProcessName}\n");
                    if (parentP.Id == pid)
                    {
                        File.AppendAllText(log, $"Child pid = {proc.Id} {proc.ProcessName}\n");
                      //  proc.Kill();
                    }
                }
                catch (Exception)
                {
                    // File.AppendAllText(log, $"{proc.Id} => {e.Message}\n");
                }
            }

            int count = 0;
            do
            {
                Thread.Sleep(500);
                count += 500;
                File.AppendAllText(log, $"Env.UserInter = {Environment.UserInteractive}\n");

            }
            while (count < 10000);

            // var process = Process.GetCurrentProcess();



            var pProcess = ParentProcessUtilities.GetParentProcess(Process.GetCurrentProcess().Handle);
            if (pProcess!=null)
                File.AppendAllText(log, $"Parent pid = {pProcess.Id}\n");
            else
                File.AppendAllText(log, $"Parent pid NOT FOUND\n");

            File.AppendAllText(log, $"Exiting pid = {pid}\n");
        }

        static void WaitConnection(TcpListener t, string msg)
        {
            var k = t.AcceptTcpClient();
            Console.WriteLine($"{msg} {k.Client.RemoteEndPoint} {k.Client.LocalEndPoint}");
        }


        static void TestLocalAny()
        {
            var server = new TcpListener(IPAddress.IPv6Any, 3000);
            var serverLocal = new TcpListener(IPAddress.IPv6Loopback, 3000);

            server.Start();
            serverLocal.Start();


            var t1 = Task.Run(() => WaitConnection(server, "FromAny"));
            var t2 = Task.Run(() => WaitConnection(serverLocal, "FromLoopback"));


            var c = new TcpClient(AddressFamily.InterNetworkV6);
            c.Connect(IPAddress.IPv6Loopback, 3000);
            Console.WriteLine($"connected to {c.Client.RemoteEndPoint}");
            Thread.Sleep(30000);
        }


        static int CheckFreePort(int portMin, int n)
        {
            var server0 = new TcpListener(IPAddress.Loopback, portMin + 1);
            server0.Start();

            var address = IPAddress.Any;
            address = IPAddress.Loopback;
            for (int port = portMin; port <= portMin + n; port++)
            {
                var server = new TcpListener(address, port);
                try
                {
                    server.Start();
                    // Thread.Sleep(15000);
                    server.Stop();
                    return port;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Port {port} is used: {e.Message}");
                }
            }
            throw new Exception("No port available!");
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        static void ClientNull()
        {
            //Console.WriteLine($"From client null pid={Process.GetCurrentProcess().Id}");
            Thread.Sleep(100_000);
        }


        static void TestAdd(BlockingCollection<int> c)
        {
            Thread.Sleep(2000);
            c.Add(17);
            Thread.Sleep(2000);
            c.CompleteAdding();
        }

        static void TestBlockingColl()
        {
            var c = new BlockingCollection<int>();
            try
            {
                c.Add(1);
                c.Add(2);

                var v1 = c.Take();
                Console.WriteLine(v1);
                var v2 = c.Take();
                Console.WriteLine(v2);

                Task.Run(() => TestAdd(c));

                var v3 = c.Take();
                Console.WriteLine(v3);

                var v4 = c.Take();
                Console.WriteLine(v4);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Queue completed");
            }
        }


        static void TestStartService()
        {
            
            string serviceName = "CheckDisk";
            var service = new ServiceController(serviceName);

            service.Refresh();
            Console.WriteLine(service.Status);
            Console.WriteLine(service.MachineName);
            Console.WriteLine(service.ServiceType);
            Console.WriteLine(service.Site);
            Console.WriteLine(service.StartType);
            
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
            service.Refresh();
            Console.WriteLine(service.Status);

            Thread.Sleep(5000);

            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
            service.Refresh();
            Console.WriteLine(service.Status);
        }

        static void TestLogPerf()
        {
            var sw = new Stopwatch();
            double elapsed;

            Logger.Info($"Init");

            double total = 0.0;
            int N = 100;
            for (int i = 0; i < N; i++)
            {
                sw.Restart();
                Logger.Info($"Test_{i}");
                elapsed = sw.ElapsedTicks * 0.1;
                total += elapsed;
                Console.WriteLine($"{elapsed} us");
            }
            Console.WriteLine($"Moy: {total/N}");
        }





        static void Main(string[] args)
        {
            try
            {
                TestStartService(); return;
//                TestLogPerf(); return;

               // TestBlockingColl(); return;
                //TestLocalAny(); return;

                /* int port = FreeTcpPort();
                 Console.WriteLine($"port={port}");

                 var server0 = new TcpListener(IPAddress.Loopback, port);
                 server0.Start();
                 server0.Stop();
                 return;
                 */

                if (args.Length >= 1)
                {
                    if (args[0] == "client")
                        Client();
                    else if (args[0] == "server")
                        Server();
                    else if (args[0] == "service")
                        Starter<Server>.Start("TestFork", args);
                    else if (args[0] == "client_null")
                        ClientNull();
                    else
                        Console.WriteLine($"Invalid arg: {args[0]}");
                }
                else
                    Console.WriteLine("Specify server or client");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
