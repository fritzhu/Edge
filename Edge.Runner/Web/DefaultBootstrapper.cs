using Nancy;
using Nancy.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Runner.Web
{
    public class DefaultBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            var dir = StaticContentConventionBuilder.AddDirectory("/", "/Web/Content", "html", "css", "js", "png", "jpg");
            conventions.StaticContentsConventions.Add(dir);
        }
    }
}
