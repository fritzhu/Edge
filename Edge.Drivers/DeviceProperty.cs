using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge.Drivers
{
    public delegate void PropertyChangeHandler(object sender, dynamic args);

    public class DeviceProperty<T>
    {
        public event EventHandler<DevicePropertyChangedEventArgs<T>> OnChange;

        private Logger log = LogManager.GetCurrentClassLogger();

        private Action<T> setValueCallback;
        private T currentValue = default(T);
        private T temporaryOverride = default(T);
        private bool temporaryOverrideIsActive = false;

        private string deviceName, propertyName;

        public void AddChangeHandler(PropertyChangeHandler handler)
        {
            OnChange += (sender, args) => handler(sender, args);
        }

        public void AddHandler(Action<object, object> handler)
        {
            OnChange += (sender, args) => handler(args.OldValue, args.NewValue);
        }

        public DeviceProperty(string deviceName, string propertyName, Action<T> setter)
        {
            this.deviceName = deviceName;
            this.propertyName = propertyName;
            this.setValueCallback = setter;
        }

        public DeviceProperty(string deviceName, string propertyName, T value, Action<T> setter)
        {
            this.deviceName = deviceName;
            this.propertyName = propertyName;
            this.currentValue = value;
            this.setValueCallback = setter;
        }
                
        public T Value
        {
            get
            {
                return temporaryOverrideIsActive ? temporaryOverride : currentValue;
            }
        }

        private void SetValueAndFireEvent(T newValue)
        {
            var oldValue = currentValue;
            currentValue = newValue;
            if (OnChange != null)
            {
                OnChange(this, new DevicePropertyChangedEventArgs<T>()
                {
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        public void SetValue(T newValue)
        {
            if (!object.Equals(Value, newValue) || temporaryOverrideIsActive)
            {
                temporaryOverride = newValue;
                temporaryOverrideIsActive = true;

                ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                {
                    try
                    {
                        setValueCallback(newValue);
                        SetValueAndFireEvent(newValue);
                    }
                    catch (Exception e)
                    {
                        log.LogException(LogLevel.Error, string.Format("Failed to set {0} on {1} to {2}", propertyName, deviceName, newValue), e);
                        temporaryOverrideIsActive = false;
                        temporaryOverride = default(T);
                    }
                }));
            }
        }
        
        public void ValueWasChangedExternally(T newValue)
        {
            if (!object.Equals(currentValue, newValue))
            {
                temporaryOverrideIsActive = false;
                temporaryOverride = default(T);
                SetValueAndFireEvent(newValue);
            }
        }
    }

    public class DevicePropertyChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }

}
