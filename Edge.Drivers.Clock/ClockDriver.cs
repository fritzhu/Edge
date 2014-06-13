using SunriseCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge.Drivers.Clock
{
    public class ClockDriver : IDeviceDriver
    {

        [DeviceProperty]
        public DeviceProperty<bool> IsAfterSunset { get; set; }

        [DeviceProperty]
        public DeviceProperty<bool> IsAfterBedtime { get; set; }

        [DeviceProperty]
        public DeviceProperty<bool> IsBeforeSunrise { get; set; }

        Timer timer;

        double latitude, longitude;

        public ClockDriver(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;

            IsAfterSunset = new DeviceProperty<bool>("Clock", "IsAfterSunset", _ => { });
            IsAfterBedtime = new DeviceProperty<bool>("Clock", "IsAfterBedtime", _ => { });
            IsBeforeSunrise = new DeviceProperty<bool>("Clock", "IsBeforeSunrise", _ => { });

            timer = new Timer(new TimerCallback(Tick));
            timer.Change(1000, 1000);
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        private void Tick(object o)
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    var sh = SolarInfo.ForDate(latitude, longitude, DateTime.Now.Date.ToUniversalTime());

                    var n = DateTime.Now.ToLocalTime();
                    var sunset = sh.Sunset.ToLocalTime();
                    var sunrise = sh.Sunrise.ToLocalTime();
                    IsAfterBedtime.ValueWasChangedExternally(n.Hour >= 20);
                    IsAfterSunset.ValueWasChangedExternally(n >= sunset || n < sunrise);
                    IsBeforeSunrise.ValueWasChangedExternally(n.Hour <= 4 && n <= sunrise);
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
    }
}
