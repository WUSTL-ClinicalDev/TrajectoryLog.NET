using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrajectoryLog.NET.TrajectorySpecifications.AxisHelpers;
using TrajectoryLog.NET.TrajectorySpecifications.Headers;

namespace TrajectoryLog.NET.TrajectorySpecifications
{
    public class TrajectoryHeader
    {
        public string Signature { get; set; }
        public string Version { get; set; }
        public int HeaderSize { get; set; }
        public int SampleIntervalMS { get; set; }
        public int NumberOfAxesSampled { get; set; }
        public int[] AxisEnumeration { get; set; }
        public int[] SamplesPerAxis { get; set; }
        public Headers.AxisScaleEnum AxisScale { get; set; }
        public int NumberOfSubbeams { get; set; }
        //1 if truncated 0 if not truncated. 
        public int IsTruncated { get; set; }
        public int NumberOfSnapshots { get; set; }
        public MLCModelEnum MLCModel { get; set; }
        public MetaData MetaData { get; set; }
        //Reserved?
        public List<Subbeam> Subbeams { get; set; }
        public AxisData AxisData { get; set; }
        public List<AxisData> AxesPerBeam { get; set; }
        public TrajectoryHeader()
        {
            Subbeams = new List<Subbeam>();
            AxisData = new AxisData();
        }

    }
}
