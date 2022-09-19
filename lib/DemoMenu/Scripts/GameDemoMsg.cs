using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nv
{
    public class GameDemoMsg 
    {
        public string Tab { get; set; }
        public string Option { get; set; }
        public List<DemoData> Data { get; set; }
    }

    public class DemoOptionsMsg 
    {
        public DemoOptions DemoOptions { get; set; }
    }
}
