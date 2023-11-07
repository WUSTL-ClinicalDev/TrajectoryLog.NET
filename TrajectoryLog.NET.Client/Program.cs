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
            if(ofd.ShowDialog() == true)
            {
                Trajectory.LoadLog(ofd.FileName);
            }
        }
    }
}
