using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ninject;
using System.Reflection;
using Edge.Runner.Scripting;
using NLog;

namespace Edge.Runner.Web
{
    public class DefaultModule : NancyModule
    {
        NLog.Logger log = LogManager.GetCurrentClassLogger();

        public DefaultModule()
            : base("/zones")
        {
            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            IDatabase database = kernel.Get<IDatabase>();
            ISceneManager sceneManager = kernel.Get<ISceneManager>();
            IScriptEngine scriptEngine = kernel.Get<IScriptEngine>();

            Get["/list"] = parameters =>
                {
                    return JsonConvert.SerializeObject((from z in database.GetZones() select new { id = z.Id, name = z.Name }).ToArray());
                };

            Get["/{id}"] = parameters =>
                {
                    try
                    {
                        int id = parameters.id;
                        var scenes = (from s in database.GetScenes(id)
                                      select new
                                      {
                                          id = s.Id,
                                          name = s.Name,
                                          directControls = (from dc in database.GetDirectControlsForScene(s.Id)
                                                            select new
                                                            {
                                                                deviceId = dc.DeviceId,
                                                                deviceName = dc.DeviceName,
                                                                memberName = dc.MemberName
                                                            }).ToArray()
                                      }).ToArray();

                        var currentScene = sceneManager.GetActiveSceneInZone(id);
                        var sceneId = currentScene == null ? (int?)null : currentScene.Id;

                        return new
                        {
                            scenes = scenes,
                            activeScene = (from s in scenes where s.id == sceneId select s).FirstOrDefault()
                        };

                    }
                    catch (Exception e)
                    {
                        LogManager.GetCurrentClassLogger().LogException(LogLevel.Warn, "Error with web request: ", e);
                        return HttpStatusCode.NotAcceptable;
                    }
                };

            Get["/{id}/scene/{scene}"] = parameters =>
            {
                int id = parameters.id;
                int scene = parameters.scene;

                sceneManager.ActivateScene(scriptEngine, id, scene);

                return HttpStatusCode.OK;
            };
        }
    }
}
