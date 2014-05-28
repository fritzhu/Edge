using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Data.Model
{
    public class Scene
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }
        public string StartScript { get; set; }
        public string StopScript { get; set; }

    }
}
