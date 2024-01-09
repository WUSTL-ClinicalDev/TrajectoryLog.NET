using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrajectoryLog.NET.TrajectoryLog.Specifications
{
    public class TrajectoryLog
    {
        public TrajectoryHeader Header { get; set; }
        public StringBuilder HeaderError { get; set; }
        public TrajectoryLog()
        {
            Header = new TrajectoryHeader();
            HeaderError = new StringBuilder();
        }
    }
}
