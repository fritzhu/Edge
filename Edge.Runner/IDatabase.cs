using Edge.Data;
using Edge.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner
{
    public interface IDatabase
    {
        IEnumerable<Device> GetDevices();
        IEnumerable<Scene> GetScenes(int zoneId);
        Scene GetScene(int zoneId, string sceneName);
        Scene GetScene(int zoneId, int sceneId);
        IEnumerable<Zone> GetZones();
        Zone GetZone(int id);
        IEnumerable<SceneDirectControl> GetDirectControlsForScene(int sceneId);
    }

    public class EdgeDatabase : IDatabase
    {
        public IEnumerable<SceneDirectControl> GetDirectControlsForScene(int sceneId)
        {
            return EdgeDB.GetDirectControlsForScene(sceneId);
        }

        public IEnumerable<Device> GetDevices()
        {
            return EdgeDB.AllDevices;
        }

        public IEnumerable<Scene> GetScenes(int zoneId)
        {
            return EdgeDB.GetScenesForZone(zoneId);
        }

        public Scene GetScene(int zoneId, string sceneName)
        {
            return EdgeDB.GetScene(zoneId, sceneName);
        }

        public Scene GetScene(int zoneId, int sceneId)
        {
            return EdgeDB.GetScene(zoneId, sceneId);
        }

        public IEnumerable<Zone> GetZones()
        {
            return EdgeDB.AllZones;
        }

        public Zone GetZone(int id)
        {
            return EdgeDB.GetZone(id);
        }
    }
}
