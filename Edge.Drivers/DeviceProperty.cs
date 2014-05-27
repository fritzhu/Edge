using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Drivers
{
    public delegate void PropertyChangeHandler(object sender, dynamic args);

    public class DeviceProperty<T>
    {
        public event EventHandler<DevicePropertyChangedEventArgs<T>> OnChange;
        
        private T propvalue = default(T);
        private Logger log = LogManager.GetCurrentClassLogger();
        private string deviceName, propertyName;

        public void AddChangeHandler(PropertyChangeHandler handler)
        {
            OnChange += (sender, args) => handler(sender, args);
        }

        public void AddHandler(Action<object, object> handler)
        {
            OnChange += (sender, args) => handler(args.OldValue, args.NewValue);
        }

        public DeviceProperty(string deviceName, string propertyName)
        {
            this.deviceName = deviceName;
            this.propertyName = propertyName;
        }

        public DeviceProperty(string deviceName, string propertyName, T value)
        {
            this.deviceName = deviceName;
            this.propertyName = propertyName;
            this.propvalue = value;
        }

        public T Value
        {
            get
            {
                return propvalue;
            }
            set
            {
                    var old = propvalue;
                    propvalue = value;
                    if (!EqualityComparer<T>.Default.Equals(old, value))
                    {
                        log.Trace("Device '{0}' property '{1}' changed from {2} to {3}", deviceName, propertyName, old, value);

                        if (OnChange != null)
                        {
                            try
                            {
                                OnChange(this, new DevicePropertyChangedEventArgs<T>()
                                {
                                    OldValue = old,
                                    NewValue = value
                                });
                            }
                            catch (Exception e)
                            {
                                log.Warn("DeviceProperty set failed.");
                                log.Warn(e);
                            }
                        }
                    }
            }
        }
    }

    public class DevicePropertyChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }

}
