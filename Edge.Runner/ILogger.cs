using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner
{
    public interface ILogger
    {
        void Trace(string msg);
        void Trace(string msg, params object[] parms);
        void Info(string msg);
        void Info(string msg, params object[] parms);
        void Warn(string msg);
        void Warn(string msg, Exception e);
        void Error(string msg);
        void Fatal(string msg);
        void Fatal(string msg, Exception e);
    }
}
