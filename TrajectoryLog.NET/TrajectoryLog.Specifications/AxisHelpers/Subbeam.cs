using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrajectoryLog.NET.TrajectorySpecifications.AxisHelpers
{
    public class Subbeam
    {
        public int CP { get; set; }
        public float Mu { get; set; }
        public float RadTime { get; set; }
        public int Seq { get; set; }
        public string Name { get; set; }
        //public string Reserved { get; set; }
    }
}
