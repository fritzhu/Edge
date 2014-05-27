using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Drivers.Clock
{
    public class ClockDriverFactory : IDeviceDriverFactory
    {
        public void Initialize()
        {
        }

        public IDeviceDriver Instantiate(string Identifier, Dictionary<string, string> configuration)
        {
            double lat = double.Parse(configuration["Latitude"]);
            double lng = double.Parse(configuration["Longitude"]);
            return new ClockDriver(lat, lng);
        }

        public void Dispose()
        {
        }
    }
}
