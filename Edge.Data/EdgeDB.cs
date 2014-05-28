using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Edge.Data.Model;

namespace Edge.Data
{
    public class EdgeDB
    {
        private static SqlConnection OpenConnection()
        {
            SqlConnection conn = new SqlConnection("Data Source=FRITZ-PC\\SQLEXPRESS;Initial Catalog=Edge;Integrated Security=true");
            conn.Open();
            return conn;
        }

        public static IEnumerable<DriverFactory> AllDriverFactories
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.Query<DriverFactory>("select Id, Name, TypeName from DriverFactory");
                }
            }
        }

        public static IEnumerable<Device> AllDevices
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.Query<Device>("select Id, Name, DriverFactoryId from Device");
                }
            }
        }

        public static Scene GetScene(string sceneName)
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<Scene>("select Id, ZoneId, Name, StartScript, StopScript from Scene where Name=@Name", new { Name = sceneName }).FirstOrDefault();
            }
        }

        public static IEnumerable<Zone> AllZones
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.Query<Zone>("select Id, Name from Zone");
                }
            }
        }

        public static IEnumerable<Scene> GetScenesForZone(int id)
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<Scene>("select s.Id, s.ZoneId, s.Name, s.StartScript, s.StopScript from Scene s where s.ZoneId = @ZoneId", new { ZoneId = id });
            }
        }

        public static Zone GetZone(int id)
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<Zone>("select Id, Name from Zone where Id = @Id", new { Id = id }).FirstOrDefault();
            }
        }

        public static IEnumerable<DeviceConfiguration> GetConfigsForDevice(int deviceId)
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<DeviceConfiguration>("select DeviceId, PropertyName, PropertyValue from DeviceConfiguration where DeviceId=@DeviceId", new { DeviceId=deviceId });
            }
        }

        public static DriverFactory GetDriverFactory(int id)
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<DriverFactory>("select Id, Name, TypeName from DriverFactory where Id = @Id", new { Id = id }).FirstOrDefault();
            }
        }
    }
}
