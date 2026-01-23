using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrajectoryLog.NET;
using TrajectoryLog.NET.TrajectorySpecifications;

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
            if (ofd.ShowDialog() == true)
            {
                locallog = TrajectoryAPI.LoadLog(ofd.FileName);
            }
            if (locallog.Header.Subbeams.Count() > 1)
            {
                TrajectoryAPI.SplitLogBeams(locallog);
            }
            //testing fluence export
            //var actualFluence = TrajectoryAPI.BuildFluence(locallog, "Actual");
            //var expectedFluence = TrajectoryAPI.BuildFluence(locallog, "Expected");
            //WriteFluence(actualFluence);
            //WriteFluence(expectedFluence);
            //Console.WriteLine("Do you want to write .csv? (y/n)");
            //if (Console.ReadLine().Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            //{
            //    TrajectoryAPI.ToCSV(locallog);
            //}
            //TrajectoryAPI.PublishPDF(locallog);
            //modify MLC leaf positions.
            TrajectoryAPI.ModifyMLCActualPositions(locallog, 0.05f);//leaf positions are in cm
            SaveLog(locallog);
            Console.ReadLine();
        }

        private static void SaveLog(TrajectoryLogInfo locallog)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Binary File (*.bin)|*.bin";
            sfd.Title = "Save Modified Trajectory Log File";
            if(sfd.ShowDialog() == true)
            {
                TrajectoryAPI.SaveLog(locallog, sfd.FileName);
            }
            Console.WriteLine($"Log saved!");
        }

        private static void WriteFluence(double[,] actualFluence)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File (*.csv)|*.csv";
            if (sfd.ShowDialog() == true)
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
