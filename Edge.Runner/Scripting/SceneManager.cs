using Edge.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner.Scripting
{
    public interface ISceneManager
    {
        void ActivateScene(IScriptEngine scriptEngine, int zoneId, string sceneName);
        void ActivateScene(IScriptEngine scriptEngine, int zoneId, int sceneId);
        Scene GetActiveSceneInZone(int zoneId);
    }

    public class SceneManager : ISceneManager
    {
        private static SceneManager instance = new SceneManager();
        public static SceneManager Instance
        {
            get
            {
                return instance;
            }
        }

        private IDatabase database;
        private ILogger log;
        private Dictionary<int, Scene> currentScenes = new Dictionary<int, Scene>();

        private SceneManager() 
        {
            this.database = new EdgeDatabase();
            this.log = new Logger();
        }

        public Scene GetActiveSceneInZone(int zoneId)
        {
            return currentScenes.ContainsKey(zoneId) ? currentScenes[zoneId] : null;
        }

        public void ActivateScene(IScriptEngine scriptEngine, int zoneId, string sceneName)
        {
            var scene = database.GetScene(zoneId, sceneName);
            if (scene == null) return;

            var zone = database.GetZone(scene.ZoneId);

            var currentScene = GetActiveSceneInZone(zoneId);

            if (currentScene != null)
            {
                if (!string.IsNullOrEmpty(currentScene.StopScript))
                {
                    try
                    {
                        scriptEngine.ExecuteSceneScript(currentScene.StopScript);
                    }
                    catch (Exception e)
                    {
                        log.Warn("Failed to execute stop script for scene " + currentScene.Name, e);
                    }
                }
            }

            log.Trace("Activating scene {0} for zone {1}", sceneName, zone.Name);

            if (!string.IsNullOrEmpty(scene.StartScript))
            {
                try
                {
                    scriptEngine.ExecuteSceneScript(scene.StartScript);
                }
                catch (Exception e)
                {
                    log.Warn("Failed to execute start script for scene " + scene.Name, e);
                }
            }

            this.currentScenes[zoneId] = scene;

            log.Info("Activated scene {0} for zone {1}", sceneName, zone.Name);
        }

        public void ActivateScene(IScriptEngine scriptEngine, int zoneId, int sceneId)
        {
            var scene = database.GetScene(zoneId, sceneId);
            if (scene == null) return;

            var zone = database.GetZone(scene.ZoneId);

            var currentScene = GetActiveSceneInZone(zoneId);

            if (currentScene != null)
            {
                if (!string.IsNullOrEmpty(currentScene.StopScript))
                {
                    try
                    {
                        scriptEngine.ExecuteSceneScript(currentScene.StopScript);
                    }
                    catch (Exception e)
                    {
                        log.Warn("Failed to execute stop script for scene " + currentScene.Name, e);
                    }
                }
            }

            log.Trace("Activating scene {0} for zone {1}", scene.Name, zone.Name);

            if (!string.IsNullOrEmpty(scene.StartScript))
            {
                try
                {
                    scriptEngine.ExecuteSceneScript(scene.StartScript);
                }
                catch (Exception e)
                {
                    log.Warn("Failed to execute start script for scene " + scene.Name, e);
                }
            }

            this.currentScenes[zoneId] = scene;

            log.Info("Activated scene {0} for zone {1}", scene.Name, zone.Name);
        }
    }
}
