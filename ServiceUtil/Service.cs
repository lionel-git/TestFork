using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using log4net;
using System.ComponentModel;

namespace ServiceUtils
{
    public interface IService
    {
        void OnStart(string[] args);
        void OnStop();
    }
     [DesignerCategory("Code")]
    public class Service<T> : ServiceBase where T : IService, new()
    {
        private readonly T r = new T();

        private static readonly ILog Logger = LogManager.GetLogger("S_"+typeof(T));

        public Service(string serviceName)
        {
            this.ServiceName = serviceName;
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                r.OnStart(args);
            }
            catch (Exception e)
            {
                Logger.Error("!!!! Exception during OnStart() !!!!", e);
                throw e;
            }
        }

        protected override void OnStop()
        {
            try
            {
                r.OnStop();
            }
            catch (Exception e)
            {
                Logger.Error("!!!! Exception during OnStop() !!!!", e);
                throw e;
            }
        }
    }
}
