using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner
{
    public class Logger : ILogger
    {
        private NLog.Logger log = NLog.LogManager.GetLogger("");

        public void Trace(string msg)
        {
            log.Trace(msg);
        }

        public void Trace(string msg, params object[] parms)
        {
            log.Trace(string.Format(msg, parms));
        }

        public void Info(string msg)
        {
            log.Info(msg);
        }

        public void Info(string msg, params object[] parms)
        {
            log.Info(string.Format(msg, parms));
        }

        public void Warn(string msg)
        {
            log.Warn(msg);
        }

        public void Warn(string msg, Exception e)
        {
            log.WarnException(msg, e);
        }

        public void Error(string msg)
        {
            log.Error(msg);
        }

        public void Fatal(string msg)
        {
            log.Fatal(msg);
        }

        public void Fatal(string msg, Exception e)
        {
            log.FatalException(msg, e);
        }
    }
}
