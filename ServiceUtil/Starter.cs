using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUtils
{
    public static class Starter<T> where T : IService, new()
    {
        public static void Start(string serviceName, string[] args)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Start {0} from console ...", typeof(T));
                var r = new T();
                r.OnStart(args);

                Console.WriteLine("Press 'q' to quit...");
                while (Console.ReadKey().KeyChar != 'q');

                r.OnStop();
            }
            else
                System.ServiceProcess.ServiceBase.Run(new Service<T>(serviceName));
        }
    }
}
