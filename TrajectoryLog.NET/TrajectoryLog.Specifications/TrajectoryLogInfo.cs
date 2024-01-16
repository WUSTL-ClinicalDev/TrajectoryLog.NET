using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrajectoryLog.NET.TrajectorySpecifications
{
    public class TrajectoryLogInfo
    {
        public TrajectoryHeader Header { get; set; }
        public StringBuilder HeaderError { get; set; }
        public TrajectoryLogInfo()
        {
            Header = new TrajectoryHeader();
            HeaderError = new StringBuilder();
        }
    }
}
