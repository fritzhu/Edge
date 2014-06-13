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
using Edge.Drivers;
using NLog;

namespace Edge.Runner.Web
{
    public class DeviceModule : NancyModule
    {
        public DeviceModule()
            : base("/device")
        {
            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            var database = kernel.Get<IDatabase>();

            Get["/{id}/{memberName}"] = parameters =>
                {
                    int deviceId = parameters.id;
                    string memberName = parameters.memberName;

                    var device = Program.DevicesById[deviceId];
                    var type = device.GetType();
                    var member = type.GetMember(memberName).FirstOrDefault();

                    object value = null;

                    if (member.MemberType == MemberTypes.Property)
                    {
                        var prop = type.GetProperty(memberName);
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DeviceProperty<>))
                        {
                            value = ((dynamic)prop.GetValue(device)).Value;
                        }
                        else
                        {
                            value = prop.GetValue(device);
                        }
                    }

                    return JsonConvert.SerializeObject(value);
                };

            Get["/{id}/{memberName}/{newValue}"] = parameters =>
            {
                int deviceId = parameters.id;
                string memberName = parameters.memberName;
                var propValue = parameters.newValue;
                
                var device = Program.DevicesById[deviceId];
                var type = device.GetType();
                var member = type.GetMember(memberName).FirstOrDefault();

                if (!propValue.HasValue)
                {
                    return HttpStatusCode.NotAcceptable;
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    var prop = type.GetProperty(memberName);
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DeviceProperty<>))
                    {
                        var ppv = (dynamic)prop.GetValue(device);
                        var valuePropType = ((PropertyInfo)ppv.GetType().GetProperty("Value")).PropertyType;

                        var parseMethod = valuePropType.GetMethod("Parse");
                        var setMethod = ppv.GetType().GetMethod("SetValue").Invoke(ppv, new object[] {
                            Convert.ChangeType(parseMethod.Invoke(null, new object[] { propValue.Value }), valuePropType)
                        });
                    }
                    else
                    {
                        prop.SetValue(device, propValue);
                    }
                }

                return HttpStatusCode.OK;
            };
        }
    }
}
