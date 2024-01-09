using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrajectoryLog.NET.TrajectoryLog.Specifications
{
    public class MetaData
    {
        public string PatientID { get; set; }
        public string PlanName { get; set; }
        public string SOPInstanceUID { get; set; }                            
        public double MUPlanned { get; set; }
        public double MURemaining { get; set; }
        public string Energy { get; set; }
        public string BeamName { get; set; }
    }
}
