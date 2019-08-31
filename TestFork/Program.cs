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

namespace TestFork
{
    class Program
    {
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
            File.AppendAllText(log, $"Parent pid = {pProcess.Id}\n");

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
            Thread.Sleep(100_000);
        }

        static void Main(string[] args)
        {
            try
            {               
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
