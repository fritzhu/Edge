using Edge.Runner.Scripting;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IScriptEngine>().ToConstant(ScriptEngine.Instance);
            Bind<ILogger>().To<Logger>();
            Bind<IDatabase>().To<EdgeDatabase>();
            Bind<ISceneManager>().ToConstant(SceneManager.Instance);
        }
    }
}
