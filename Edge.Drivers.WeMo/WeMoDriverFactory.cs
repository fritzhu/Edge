using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge.Drivers.WeMo
{
    [DriverConfigProperty("IpAddress", "IP address", true, "The IP address of the WeMo device.", typeof(string))]
    public class WeMoDriverFactory : IDeviceDriverFactory
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public void Initialize()
        {
            log.Trace("Initialized");
        }

        public IDeviceDriver Instantiate(string Identifier, Dictionary<string, string> configuration)
        {
            log.Trace("Instantiating WeMoDriver with IP address " + configuration["IpAddress"]);
            WeMoDriver driver = new WeMoDriver(Identifier, configuration["IpAddress"]);

            return driver;
        }

        public void Dispose()
        {
        }
    }
}
