using AluminumLua;
using Edge.Data;
using Edge.Drivers;
using Edge.Drivers.WeMo;
using Edge.Runner.Scripting;
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
    class Program
    {
        private static Dictionary<string, IDeviceDriverFactory> DriverFactories = new Dictionary<string, IDeviceDriverFactory>();
        private static Dictionary<string, IDeviceDriver> Devices = new Dictionary<string, IDeviceDriver>();

        static Logger log = LogManager.GetCurrentClassLogger();

        static EdgeDataSet dataSet;

        static void Main(string[] args)
        {
            using (dataSet = new EdgeDataSet())
            {
                log.Info("Main()");
                try
                {

                    log.Trace("Reading config.xml");
                    dataSet.ReadXml("config.xml");

                    foreach (var driver in dataSet.DriverFactory)
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
                            log.WarnException(string.Format("Failed to load driver factory class {0}", driver.TypeName), e);
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
                            log.WarnException(string.Format("Failed to initialize driver factory class {0}", df.Key), e);
                        }
                    }

                    foreach (var device in dataSet.Device)
                    {
                        try
                        {
                            if (!DriverFactories.ContainsKey(device.DriverTypeName))
                            {
                                throw new Exception(string.Format("Driver {0} is not available.", device.DriverTypeName));
                            }

                            var config = new Dictionary<string, string>();
                            foreach (var cfg in device.GetDeviceConfigurationRows())
                            {
                                config.Add(cfg.PropertyName, cfg.PropertyValue);
                            }

                            log.Trace("Instantiating device {0} using driver {1}", device.Name, device.DriverTypeName);
                            var driver = DriverFactories[device.DriverTypeName].Instantiate(device.Name, config);

                            Devices.Add(device.Name, driver);
                        }
                        catch (Exception e)
                        {
                            log.WarnException(string.Format("Failed to setup device {0}!", device.Name), e);
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
                ScriptEngine engine = null;
                try
                {
                    engine = new ScriptEngine();

                    foreach (var device in Devices)
                    {
                        engine.ExposeDevice(device.Key, device.Value);
                    }

                    foreach (var scene in dataSet.Scene.GroupBy(x => x.ZoneName))
                    {
                        engine.ExposeZone(string.IsNullOrEmpty(scene.Key) ? "All" : scene.Key, (from s in scene select s.Name).ToArray());
                    }

                    engine.ExecuteSystemScripts();
                }
                catch (Exception e)
                {
                    log.FatalException("Failed to start scripting engine!", e);
                }

                log.Info("Ready");

                while (true)
                {
                    Console.ReadKey();
                    ActivateScene(engine, "Evening");
                    Console.ReadKey();
                    ActivateScene(engine, "Goodnight");
                }
            }
        }

        public static void ActivateScene(ScriptEngine scriptEngine, string sceneName)
        {
            var scene = (from s in dataSet.Scene where s.Name == sceneName select s).FirstOrDefault();
            if (scene == null) return;

            log.Trace("Activating scene {0}", sceneName);
            if (!string.IsNullOrEmpty(scene.StartScript))
            {
                try
                {
                    scriptEngine.ExecuteSceneScript(scene.StartScript);
                }
                catch (Exception e)
                {
                    log.WarnException("Failed to execute start script for scene " + scene.Name, e);
                }
            }
            log.Info("Scene {0} activated", sceneName);
        }

        private static LuaObject LDefineRoom(LuaObject[] parms)
        {
            var name = parms[0].AsString();
            
            Dictionary<LuaObject, LuaObject> table = new Dictionary<LuaObject, LuaObject>();
            table.Add(LuaObject.FromString("Name"), LuaObject.FromString(name));
            table.Add(LuaObject.FromString("Scenes"), LuaObject.NewTable());
            table.Add(LuaObject.FromString("DefineScene"), new LuaFunction(x =>
            {
                log.Info("DefineScene {0}", x[0].AsString());
                return LuaObject.Nil;
            }));
            return LuaObject.FromTable(table);
        }

        /*
        static void RegisterOnChange(Engine e, string device, string property, Func<JsValue, JsValue[], JsValue> callback)
        {
            if (!Devices.ContainsKey(device))
            {
                throw new Exception(string.Format("Device {0} not found!", device));
            }

            var dev = Devices[device];
            var inf = dev.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public);

            if (inf == null)
            {
                throw new Exception(string.Format("Device {0} has no property {1}!", device, property));
            }

            if (!inf.PropertyType.IsGenericType || inf.PropertyType.GetGenericTypeDefinition() != typeof(DeviceProperty<>))
            {
                throw new Exception(string.Format("Property {0} on device {1} is not a {2}!", property, device, typeof(DeviceProperty<>)));
            }

            dynamic devProp = inf.GetValue(dev);

            devProp.AddChangeHandler(new PropertyChangeHandler((sender, args) =>
            {
                try
                {
                    callback(e.Global, new JsValue[] { JsValue.FromObject(e, args.OldValue), JsValue.FromObject(e, args.NewValue) });
                }
                catch (Exception ex)
                {
                    log.WarnException("Event failed!", ex);
                }
            }));
        }*/

    }
}
