using AluminumLua;
using Edge.Data;
using Edge.Drivers;
using Edge.Drivers.WeMo;
using Edge.Runner.Scripting;
using Nancy.Hosting.Self;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner
{
    public class Program
    {
        private static Dictionary<string, IDeviceDriverFactory> DriverFactories = new Dictionary<string, IDeviceDriverFactory>();
        private static Dictionary<string, IDeviceDriver> Devices = new Dictionary<string, IDeviceDriver>();
        public static Dictionary<int, IDeviceDriver> DevicesById = new Dictionary<int, IDeviceDriver>();

        static ILogger log;

        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            log = kernel.Get<ILogger>();
            log.Info("Main()");

            try
            {
                foreach (var driver in EdgeDB.AllDriverFactories)
                {
                    log.Trace("Loading driver factory class {0}", driver.TypeName);
                    try
                    {
                        var type = Type.GetType(driver.TypeName);
                        if (type == null)
                        {
                            throw new Exception(string.Format("Driver factory class {0} not found!", driver.TypeName));
                        }

                        if (!typeof(IDeviceDriverFactory).IsAssignableFrom(type))
                        {
                            throw new Exception(string.Format("Driver factory class {0} does not implement {1}!", driver.TypeName, typeof(IDeviceDriverFactory).FullName));
                        }

                        var inst = (IDeviceDriverFactory)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                        DriverFactories.Add(driver.TypeName, inst);
                    }
                    catch (Exception e)
                    {
                        log.Warn(string.Format("Failed to load driver factory class {0}", driver.TypeName), e);
                    }
                }

                foreach (var df in DriverFactories)
                {
                    log.Trace("Initializing driver factory class " + df.Key);
                    try
                    {
                        df.Value.Initialize();
                    }
                    catch (Exception e)
                    {
                        log.Warn(string.Format("Failed to initialize driver factory class {0}", df.Key), e);
                    }
                }

                foreach (var device in EdgeDB.AllDevices)
                {
                    try
                    {
                        var driverFactory = EdgeDB.GetDriverFactory(device.DriverFactoryId);
                        if (driverFactory == null)
                        {
                            throw new Exception(string.Format("Driver factory with ID {0} is not available.", device.DriverFactoryId));
                        }

                        if (!DriverFactories.ContainsKey(driverFactory.TypeName))
                        {
                            throw new Exception(string.Format("Driver {0} is not available.", driverFactory.TypeName));
                        }

                        var config = new Dictionary<string, string>();
                        foreach (var cfg in EdgeDB.GetConfigsForDevice(device.Id))
                        {
                            config.Add(cfg.PropertyName, cfg.PropertyValue);
                        }

                        log.Trace("Instantiating device {0} using driver {1}", device.Name, driverFactory.TypeName);
                        var driver = DriverFactories[driverFactory.TypeName].Instantiate(device.Name, config);

                        Devices.Add(device.Name, driver);
                        DevicesById.Add(device.Id, driver);
                    }
                    catch (Exception e)
                    {
                        log.Warn(string.Format("Failed to setup device {0}!", device.Name), e);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Failed to setup devices!", e);
            }

            log.Info("Config loaded");
            log.Info("Starting scripting engine");
            log.Trace("Starting Lua engine");
            IScriptEngine engine = null;
            try
            {
                engine = kernel.Get<IScriptEngine>();

                foreach (var device in Devices)
                {
                    engine.ExposeDevice(device.Key, device.Value);
                }

                foreach (var zone in EdgeDB.AllZones)
                {
                    var scenes = EdgeDB.GetScenesForZone(zone.Id).Select(x => x.Name).ToArray();
                    engine.ExposeZone(zone.Id, zone.Name, scenes);
                }

                engine.ExecuteSystemScripts();
            }
            catch (Exception e)
            {
                log.Fatal("Failed to start scripting engine!", e);
            }

            log.Info("Starting NancyHost");

            HostConfiguration nancyConfig = new HostConfiguration();
            nancyConfig.UrlReservations.CreateAutomatically = true;
            nancyConfig.UnhandledExceptionCallback = e => log.Error(e.ToString());
            using (var host = new NancyHost(nancyConfig, new Uri("http://localhost:8080")))
            {
                host.Start();

                log.Info("Ready");
                Console.ReadKey();
            }
        }

    }
}
