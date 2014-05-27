using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Drivers
{
    public interface IDeviceDriverFactory : IDisposable
    {
        void Initialize();

        IDeviceDriver Instantiate(string Identifier, Dictionary<string, string> configuration);
    }
}
