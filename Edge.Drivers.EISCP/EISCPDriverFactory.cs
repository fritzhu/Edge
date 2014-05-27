using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Drivers.EISCP
{
    public class EISCPDriverFactory : IDeviceDriverFactory
    {
        public void Dispose()
        {
            
        }
        public void Initialize()
        {
            
        }

        public IDeviceDriver Instantiate(string Identifier, Dictionary<string, string> configuration)
        {
            throw new NotImplementedException();
        }
    }
}
