using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrajectoryLog.NET;

namespace TrajectoryLog.NET.Client
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Trajectory Log File (*.bin)|*.bin";
            TrajectoryAPI.EnableDebug();
            TrajectorySpecifications.TrajectoryLogInfo locallog = null;
            if(ofd.ShowDialog() == true)
            {
                locallog = TrajectoryAPI.LoadLog(ofd.FileName);
            }
            //testing fluence export
            TrajectoryAPI.BuildExpectedFluence(locallog,"Expected");
            Console.WriteLine("Do you want to write .csv? (y/n)");
            if (Console.ReadLine().Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                TrajectoryAPI.ToCSV(locallog);
            }
            Console.ReadLine();
        }
    }
}
