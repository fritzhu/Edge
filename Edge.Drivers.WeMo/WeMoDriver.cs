using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Edge.Drivers;
using System.ServiceModel;
using NLog;
using System.Threading;

namespace Edge.Drivers.WeMo
{
    public class WeMoDriver : IDeviceDriver
    {
        WeMoServiceReference.BasicServicePortTypeClient service;
        Timer timer;

        public DeviceProperty<bool> State { get; set; }

        [DeviceAction]
        public void Switch(bool state)
        {
            if (State.Value != state)
            {
                service.SetBinaryState(new WeMoServiceReference.SetBinaryState() { BinaryState = state ? "1" : "0" });
                State.Value = state;
            }
        }
        
        int port = 49153;
        string ipAddress = "";

        public WeMoDriver(string name, string IpAddress)
        {
            timer = new Timer(new TimerCallback(timer_Elapsed));
            State = new DeviceProperty<bool>(name, "State");
            this.ipAddress = IpAddress;
            
            service = new WeMoServiceReference.BasicServicePortTypeClient(new BasicHttpBinding(), GetEndpointUrl());
            
            timer.Change(1000, 1000);
        }

        private EndpointAddress GetEndpointUrl()
        {
            var url = string.Format("http://{0}:{1}/upnp/control/basicevent1", ipAddress, port);
            var endpoint = new System.ServiceModel.EndpointAddress(url);
            return endpoint;
        }

        void timer_Elapsed(object o)
        {
            if (Monitor.TryEnter(timer))
            {
                try
                {
                    var binaryState = service.GetBinaryState(new WeMoServiceReference.GetBinaryState());
                    State.Value = (binaryState.BinaryState == "1");
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Warn("Could not get WeMo switch state from " + service.Endpoint.Address.Uri + ". " + e.Message);
                    if (port < 49160) port++;
                    else port = 49153;
                    service.Endpoint.Address = GetEndpointUrl();
                }
                finally
                {
                    Monitor.Exit(timer);
                }
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            service.Close();
        }
    }
}
