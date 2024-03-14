using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
            var actualFluence = TrajectoryAPI.BuildFluence(locallog,"Actual");
            var expectedFluence = TrajectoryAPI.BuildFluence(locallog, "Expected");
            WriteFluence(actualFluence);
            WriteFluence(expectedFluence);
            Console.WriteLine("Do you want to write .csv? (y/n)");
            if (Console.ReadLine().Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                TrajectoryAPI.ToCSV(locallog);
            }
            Console.ReadLine();
        }

        private static void WriteFluence(double[,] actualFluence)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File (*.cvs)|*.csv";
            if(sfd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    for (int i = 0; i < actualFluence.GetLength(0); i++)
                    {
                        string line = String.Empty;
                        for (int j = 0; j < actualFluence.GetLength(1); j++)
                        {
                            line += $"{actualFluence[i, j]},";
                        }
                        sw.WriteLine(line.TrimEnd(','));
                    }
                    sw.Flush();
                }
            }
        }
    }
}
