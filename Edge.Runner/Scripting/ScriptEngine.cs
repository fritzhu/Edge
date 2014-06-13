using AluminumLua;
using Edge.Drivers;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner.Scripting
{
    public interface IScriptEngine
    {
        void ExecuteSystemScripts();
        void ExposeDevice(string name, IDeviceDriver device);
        void ExposeZone(int id, string name, string[] scenes);
        void ExecuteSceneScript(string file);
    }

    public class ScriptEngine : IScriptEngine
    {
        private static ScriptEngine instance = new ScriptEngine(new Logger(), SceneManager.Instance);
        public static ScriptEngine Instance
        {
            get
            {
                return instance;
            }
        }

        private ILogger log;
        private ISceneManager sceneManager;
        private LuaContext context = new LuaContext();

        private ScriptEngine(ILogger logger, ISceneManager sceneManager)
        {
            this.log = logger;
            this.sceneManager = sceneManager;

            context.AddBasicLibrary();
            context.AddIoLibrary();

            Func<LuaObject[], string> buildMessage = x =>
            {
                string s = "";
                foreach (LuaObject o in x)
                {
                    s += (o.IsNil ? "<NIL>" : o.AsString()) + " ";
                }
                return s;
            };

            context.SetGlobal("trace", new LuaFunction(x => { log.Trace(buildMessage(x)); return x.FirstOrDefault(); }));
            context.SetGlobal("info", new LuaFunction(x => { log.Info(buildMessage(x)); return x.FirstOrDefault(); }));
            context.SetGlobal("warn", new LuaFunction(x => { log.Warn(buildMessage(x)); return x.FirstOrDefault(); }));
            context.SetGlobal("error", new LuaFunction(x => { log.Error(buildMessage(x)); return x.FirstOrDefault(); }));
        }

        public void ExecuteSystemScripts()
        {
            foreach (string filename in Directory.EnumerateFiles("Scripts", "*.lua"))
            {
                log.Trace("Executing script in {0}", filename);
                using (var parser = new LuaParser(context, filename))
                {
                    parser.Parse();
                }
                log.Trace("Finished executing script");
            }
        }

        public void ExposeDevice(string name, IDeviceDriver device)
        {
            log.Trace("Exposing device {0} to Lua engine", name);
            Dictionary<LuaObject, LuaObject> devprops = new Dictionary<LuaObject, LuaObject>();

            foreach (var prop in device.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DeviceProperty<>)))
            {
                Dictionary<LuaObject, LuaObject> pt = new Dictionary<LuaObject, LuaObject>();
                pt.Add(LuaObject.FromString("Get"), new LuaFunction(x => ((dynamic)prop.GetValue(device)).Value));
                pt.Add(LuaObject.FromString("OnChange"), new LuaFunction(x =>
                {
                    ((dynamic)prop.GetValue(device)).AddChangeHandler(new PropertyChangeHandler((y, dyn) =>
                    {
                        x.FirstOrDefault().AsFunction().Invoke(new LuaObject[] { LuaObject.FromObject(dyn.OldValue), LuaObject.FromObject(dyn.NewValue) });
                    }));
                    return LuaObject.Nil;
                }));

                devprops.Add(prop.Name, LuaObject.NewTable(pt.ToArray()));
            }

            foreach (var ev in device.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(x => x.GetCustomAttribute(typeof(DeviceEventAttribute)) != null))
            {
                Dictionary<LuaObject, LuaObject> pt = new Dictionary<LuaObject, LuaObject>();
                pt.Add(LuaObject.FromString("AddHandler"), new LuaFunction(x =>
                {
                    var f = x.FirstOrDefault();
                    var func = f.AsFunction();
                    if (func != null)
                    {
                        ev.AddEventHandler(device, new EventHandler((src, arg) =>
                        {
                            func.Invoke(new LuaObject[0]);
                        }));
                    }
                    return LuaObject.Nil;
                }));

                devprops.Add(ev.Name, LuaObject.NewTable(pt.ToArray()));
            }

            foreach (var act in device.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(x => x.GetCustomAttribute(typeof(DeviceActionAttribute)) != null))
            {
                devprops.Add(act.Name, new LuaFunction(x =>
                {
                    List<object> parms = new List<object>();
                    foreach (var p in x)
                    {
                        if (p.IsBool)
                        {
                            parms.Add(p.AsBool());
                        }
                        else if (p.IsNil)
                        {
                            parms.Add(null);
                        }
                        else if (p.IsNumber)
                        {
                            parms.Add(p.AsNumber());
                        }
                        else if (p.IsString)
                        {
                            parms.Add(p.AsString());
                        }
                    }

                    return LuaObject.FromObject(act.Invoke(device, parms.ToArray()));
                }));
            }

            var dev = LuaObject.NewTable(devprops.ToArray());
            context.SetGlobal(name, dev);
        }

        public void ExposeZone(int id, string name, string[] scenes)
        {
            log.Trace("Exposing zone {0} with {1} scenes", name, scenes.Count());
            Dictionary<LuaObject, LuaObject> table = new Dictionary<LuaObject, LuaObject>();

            foreach (string s in scenes)
            {
                table.Add(s, new LuaFunction(parms => {
                    sceneManager.ActivateScene(this, id, s);
                    return LuaObject.Nil;
                }));
            }

            context.SetGlobal(name, LuaObject.NewTable(table.ToArray()));
        }

        public void ExecuteSceneScript(string file)
        {
            string filename = Path.Combine("Scripts\\Scenes", file);
            log.Trace("Executing scene script " + filename);
            using (var parser = new LuaParser(context, filename))
            {
                parser.Parse();
            }
            log.Trace("Scene script " + filename + " finished executing");
        }
    }
}
