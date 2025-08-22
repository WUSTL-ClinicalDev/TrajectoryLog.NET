using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrajectoryLog.NET.TrajectorySpecifications;
using TrajectoryLog.NET.TrajectorySpecifications.AxisHelpers;
using TrajectoryLog.NET.TrajectorySpecifications.Headers;
using Specs = TrajectoryLog.NET.TrajectorySpecifications;

namespace TrajectoryLog.NET
{
    public static class TrajectoryAPI
    {
        #region Statics
        /// <summary>
        /// Definitions of log parts and memory allocation (from the Trajectory Log File Specification Guide). 
        /// </summary>
        public static Dictionary<string, int> LogSizes = new Dictionary<string, int>()
        {
            { "Signature",16 },
            {"Version",16 },
            {"HeaderSize",4 },
            {"SamplingIntervalMS",4 },
            {"NumberOfAxesSampled",4 },
            {"AxisEnumeration",4 },//number of axis * 4 bytes
            {"SamplesPerAxis", 4},
            {"AxisScale",4 },
            {"NumberOfSubbeams", 4},
            {"IsTruncated",4 },
            {"NumberOfSnapshots",4 },
            {"MLCModel",4 },
            {"MetaData",745 },
            {"Subbeam" , 560},
            {"Snapshot", 4 }
        };
        //public static Dictionary<string, int> MetaDataSizes = new Dictionary<string, int>
        //{
        //    {"PatientID", 256},
        //    {"PlanName", 64},
        //    {"SOPInstanceUID", 64},
        //    {"MUPlanned", 10},
        //    {"MURemaining", 10},
        //    {"Energy", 7},
        //    {"BeamName", 256}
        //};
        /// <summary>
        /// SubBeam Components
        /// </summary>
        public static Dictionary<string, int> SubbeamSizes = new Dictionary<string, int>
        {
            {"cp",4 },
            {"mu",4 },
            {"radtime",4 },
            {"seq",4 },
            {"name",544 }//512 for name and 32 for reserved.
        };

        private static bool bDebug;
        private static int headerReserveSize = 0;
        #endregion
        #region APIFunctions
        /// <summary>
        /// Enables Console Writing for debugging.
        /// </summary>
        public static void EnableDebug()
        {
            bDebug = true;
        }
        /// <summary>
        /// Disables Console Writing output.
        /// </summary>
        public static void DisableDebug()
        {
            bDebug = false;
        }
        /// <summary>
        /// Reads file from file system and converts to a trajectory log
        /// </summary>
        /// <param name="fileName">Filename of BIN file.</param>
        /// <returns>Trajectory Log from TrajectoryLog.Specticiations.TrajectorLog</returns>
        public static Specs.TrajectoryLogInfo LoadLog(string fileName)
        {
            //instantiate the log file
            Specs.TrajectoryLogInfo tLog = new Specs.TrajectoryLogInfo();
            //fill the log file data.
            using (BinaryReader logReader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                foreach (var data in LogSizes)
                {
                    switch (data.Key)
                    {
                        case "Signature":
                            string signature = Encoding.ASCII.GetString(logReader.ReadBytes(data.Value));
                            tLog.Header.Signature = signature.Replace("\0", "");
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.Signature}"); }
                            break;
                        case "Version":
                            tLog.Header.Version = Encoding.ASCII.GetString(logReader.ReadBytes(data.Value));
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.Version}"); }
                            break;
                        case "HeaderSize":
                            tLog.Header.HeaderSize = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.HeaderSize}"); }
                            break;
                        case "SamplingIntervalMS":
                            tLog.Header.SampleIntervalMS = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.SampleIntervalMS}"); }
                            break;
                        case "NumberOfAxesSampled":
                            tLog.Header.NumberOfAxesSampled = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            headerReserveSize = 1024 - (64 + tLog.Header.NumberOfAxesSampled * 8);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.NumberOfAxesSampled}"); }
                            break;
                        case "AxisEnumeration":
                            if (tLog.Header.NumberOfAxesSampled != 0)
                            {
                                tLog.Header.AxisEnumeration = new int[tLog.Header.NumberOfAxesSampled];
                                for (int i = 0; i < tLog.Header.NumberOfAxesSampled; i++)
                                {
                                    tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                }
                                if (bDebug) { Console.WriteLine($"{data.Key}: [{String.Join(" ", tLog.Header.AxisEnumeration)}]"); }
                            }
                            else
                            {
                                tLog.HeaderError.Append("Axis enumeration attempted without number of axes known. ");
                            }
                            break;
                        case "SamplesPerAxis":
                            if (tLog.Header.NumberOfAxesSampled != 0)
                            {
                                tLog.Header.SamplesPerAxis = new int[tLog.Header.NumberOfAxesSampled];
                                for (int i = 0; i < tLog.Header.NumberOfAxesSampled; i++)
                                {
                                    tLog.Header.SamplesPerAxis[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                }
                                if (bDebug) { Console.WriteLine($"{data.Key}: [{String.Join(" ", tLog.Header.SamplesPerAxis)}]"); }
                            }
                            else
                            {
                                tLog.HeaderError.Append("Samples per axis attempted without number of axes known.");
                            }
                            break;
                        case "AxisScale":
                            tLog.Header.AxisScale = (AxisScaleEnum)BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.AxisScale}"); }
                            break;
                        case "NumberOfSubbeams":
                            tLog.Header.NumberOfSubbeams = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.NumberOfSubbeams}"); }
                            break;
                        case "IsTruncated":
                            tLog.Header.IsTruncated = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.IsTruncated}"); }
                            break;
                        case "NumberOfSnapshots":
                            tLog.Header.NumberOfSnapshots = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.NumberOfSnapshots}"); }
                            break;
                        case "MLCModel":
                            tLog.Header.MLCModel = (MLCModelEnum)BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            if (bDebug) { Console.WriteLine($"{data.Key}: {tLog.Header.MLCModel}"); }
                            break;
                        case "MetaData":
                            tLog.Header.MetaData = ConvertMetaData(logReader);
                            break;
                        //TODO Add other header components.
                        case "Subbeam":
                            for (int i = 0; i < tLog.Header.NumberOfSubbeams; i++)
                            {
                                if (bDebug) { Console.WriteLine($"SubBeam {i + 1}"); }
                                Subbeam sb = new Subbeam();
                                foreach (var sub in SubbeamSizes)
                                {
                                    switch (sub.Key)
                                    {
                                        case "cp":
                                            sb.CP = BitConverter.ToInt32(logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.CP}"); }
                                            break;
                                        case "mu":
                                            sb.Mu = BitConverter.ToSingle(logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.Mu}"); }
                                            break;
                                        case "radtime":
                                            sb.RadTime = BitConverter.ToSingle(logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.RadTime}"); }
                                            break;
                                        case "seq":
                                            sb.Seq = BitConverter.ToInt32(logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.Seq}"); }
                                            break;
                                        case "name":
                                            string localName = Encoding.ASCII.GetString(logReader.ReadBytes(sub.Value - 32));
                                            sb.Name = localName.Replace("\0", "");// logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.Name}"); }
                                            logReader.ReadBytes(32);//read out reserved.
                                            break;

                                    }
                                }
                                tLog.Header.Subbeams.Add(sb);
                            }
                            break;
                        case "Snapshot":
                            CollectAxisData(tLog.Header.AxisEnumeration, tLog.Header.SamplesPerAxis, tLog.Header.NumberOfSnapshots, tLog.Header.AxisData, logReader);
                            break;
                        default:
                            tLog.HeaderError.Append($"Unsupported: {data.Key}");
                            break;

                    }
                }
            }
            return tLog;
        }
        public static bool SplitLogBeams(Specs.TrajectoryLogInfo tlog)
        {
            int numberOfSubBeams = tlog.Header.Subbeams.Count();
            if (numberOfSubBeams > 1)
            {
                //inialize the list
                tlog.Header.AxesPerBeam = new List<AxisData>();

            }
            else
            {
                return false;
            }
            int startIndex = 0;
            int endIndex = 0;
            foreach (var beam in tlog.Header.Subbeams.OrderBy(sb => sb.Seq))
            {
                bool isLast = beam.Seq == numberOfSubBeams - 1;


                if (!isLast)
                {
                    int nextCP = tlog.Header.Subbeams.ElementAt(beam.Seq + 1).CP;
                    endIndex = tlog.Header.AxisData.ControlPointAct.First().ToList().IndexOf(tlog.Header.AxisData.ControlPointAct.First().FirstOrDefault(cp => cp >= nextCP));


                }
                else
                {
                    endIndex = tlog.Header.AxisData.ControlPointAct.First().Length;

                }
                AxisData ad = IterateAxisData(tlog.Header.AxisEnumeration, tlog.Header.AxisData, startIndex, endIndex);
                tlog.Header.AxesPerBeam.Add(ad);
                startIndex = endIndex;
            }
            return true;
        }



        /// <summary>
        /// Exports trajectory log file to CSV
        /// </summary>
        /// <param name="tlog">Log file to be serialized and saved.</param>
        /// <returns></returns>
        public static bool ToCSV(Specs.TrajectoryLogInfo tlog)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma Separated Value (*.csv)|*.csv";
            if (sfd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    foreach (var header in LogSizes)
                    {
                        switch (header.Key)
                        {
                            case "Signature":
                                sw.WriteLine($"Signature,{tlog.Header.Signature}");
                                break;
                            case "Version":
                                sw.WriteLine($"Version,{tlog.Header.Version}");
                                break;
                            case "HeaderSize":
                                sw.WriteLine($"Header Size,{tlog.Header.HeaderSize}");
                                break;
                            case "SamplingIntervalMS":
                                sw.WriteLine($"Sampling Interval (mS),{tlog.Header.SampleIntervalMS}");
                                break;
                            case "NumberOfAxesSampled":
                                sw.WriteLine($"Number of Axes Sampled,{tlog.Header.NumberOfAxesSampled}");
                                break;
                            case "AxisEnumeration":
                                sw.WriteLine($"Axis Enumeration,[{String.Join("\t ", tlog.Header.AxisEnumeration)}]");
                                break;
                            case "SamplesPerAxis":
                                sw.WriteLine($"Samples Per Axis,[{String.Join("\t ", tlog.Header.SamplesPerAxis)}]");
                                break;
                            case "AxisScale":
                                sw.WriteLine($"Axis Scale,{tlog.Header.AxisScale}");
                                break;
                            case "NumberOfSubbeams":
                                sw.WriteLine($"Number of Subbeams,{tlog.Header.NumberOfSubbeams}");
                                break;
                            case "IsTruncated":
                                sw.WriteLine($"Is Truncated (1 = truncated / 0 = not truncated),{tlog.Header.IsTruncated}");
                                break;
                            case "NumberOfSnapshots":
                                sw.WriteLine($"Number of Snapshots,{tlog.Header.NumberOfSnapshots}");
                                break;
                            case "MLCModel":
                                sw.WriteLine($"MLC Model,{tlog.Header.MLCModel}");
                                break;
                        }
                    }
                    // start writing out axis data
                    // first find the number of MLC axes
                    int mlcsamples = tlog.Header.AxisEnumeration.Any(x => x == 50) ? tlog.Header.SamplesPerAxis[Array.IndexOf(tlog.Header.AxisEnumeration, 50)] : 0;
                    sw.WriteLine($"Gantry Expected[deg],{String.Join(",", tlog.Header.AxisData.GantryRtnExpected.First())}");
                    sw.WriteLine($"Gantry Actual[deg],{String.Join(",", tlog.Header.AxisData.GantryRtnActual.First())}");
                    sw.WriteLine($"Collimator Expected[deg],{String.Join(",", tlog.Header.AxisData.CollRtnExpected.First())}");
                    sw.WriteLine($"Collimator Actual[deg],{String.Join(",", tlog.Header.AxisData.CollRtnActual.First())}");
                    sw.WriteLine($"X1 Expected[cm],{String.Join(",", tlog.Header.AxisData.X1Expected.First())}");
                    sw.WriteLine($"X1 Actual[cm],{String.Join(",", tlog.Header.AxisData.X1Actual.First())}");
                    sw.WriteLine($"X2 Expected[cm],{String.Join(",", tlog.Header.AxisData.X2Expected.First())}");
                    sw.WriteLine($"X2 Actual[cm],{String.Join(",", tlog.Header.AxisData.X2Actual.First())}");
                    sw.WriteLine($"Y1 Expected[cm],{String.Join(",", tlog.Header.AxisData.Y1Expected.First())}");
                    sw.WriteLine($"Y1 Actual[cm],{String.Join(",", tlog.Header.AxisData.Y1Actual.First())}");
                    sw.WriteLine($"Y2 Expected[cm],{String.Join(",", tlog.Header.AxisData.Y2Expected.First())}");
                    sw.WriteLine($"Y2 Actual[cm],{String.Join(",", tlog.Header.AxisData.Y2Actual.First())}");
                    sw.WriteLine($"Couch Lat Expected [cm],{String.Join(",", tlog.Header.AxisData.CouchLatExp.First())}");
                    sw.WriteLine($"Couch Lat Actual [cm],{String.Join(",", tlog.Header.AxisData.CouchLatAct.First())}");
                    sw.WriteLine($"Couch Lng Expected [cm],{String.Join(",", tlog.Header.AxisData.CouchLngExp.First())}");
                    sw.WriteLine($"Couch Lng Actual [cm],{String.Join(",", tlog.Header.AxisData.CouchLngAct.First())}");
                    sw.WriteLine($"Couch Vert Expected [cm],{String.Join(",", tlog.Header.AxisData.CouchVrtExp.First())}");
                    sw.WriteLine($"Couch Vert Actual [cm],{String.Join(",", tlog.Header.AxisData.CouchVrtAct.First())}");
                    sw.WriteLine($"Couch Rtn Expected [deg],{String.Join(",", tlog.Header.AxisData.CouchRtnExp.First())}");
                    sw.WriteLine($"Couch Rtn Actual [deg],{String.Join(",", tlog.Header.AxisData.CouchRtnAct.First())}");
                    sw.WriteLine($"Couch Pit Expected [deg],{String.Join(",", tlog.Header.AxisData.CouchPitExp.First())}");
                    sw.WriteLine($"Couch Pit Actual [deg],{String.Join(",", tlog.Header.AxisData.CouchPitAct.First())}");
                    sw.WriteLine($"Couch Roll Expected [deg],{String.Join(",", tlog.Header.AxisData.CouchRolExp.First())}");
                    sw.WriteLine($"Couch Roll Actual [deg],{String.Join(",", tlog.Header.AxisData.CouchRolAct.First())}");
                    sw.WriteLine($"Mu Expected,{String.Join(",", tlog.Header.AxisData.MUExp.First())}");
                    sw.WriteLine($"MU Actual,{String.Join(",", tlog.Header.AxisData.MUAct.First())}");
                    sw.WriteLine($"Beam Hold Expected,{String.Join(",", tlog.Header.AxisData.BeamHoldExp.First())}");
                    sw.WriteLine($"Beam HOld Actual,{String.Join(",", tlog.Header.AxisData.BeamHoldAct.First())}");
                    sw.WriteLine($"Control Point Expected,{String.Join(",", tlog.Header.AxisData.ControlPointExp.First())}");
                    sw.WriteLine($"Control Point Actual,{String.Join(",", tlog.Header.AxisData.ControlPointAct.First())}");

                    // for MLCs only
                    for (int i = 0; i < mlcsamples; i++)
                    {
                        if (tlog.Header.MLCModel != MLCModelEnum.SX2 && i == 0)
                        {
                            sw.WriteLine($"Carriage A Expected [cm],{String.Join(",", tlog.Header.AxisData.MLCExp.ElementAt(i))}");
                            sw.WriteLine($"Carriage A Actual [cm],{String.Join(",", tlog.Header.AxisData.MLCAct.ElementAt(i))}");
                            i++;
                            sw.WriteLine($"Carriage B Expected [cm],{String.Join(",", tlog.Header.AxisData.MLCExp.ElementAt(i))}");
                            sw.WriteLine($"Carriage B Actual [cm],{String.Join(",", tlog.Header.AxisData.MLCAct.ElementAt(i))}");
                        }
                        else
                        {
                            sw.WriteLine($"Leaf {i - 1} Expected [cm],{String.Join(",", tlog.Header.AxisData.MLCExp.ElementAt(i))}");
                            sw.WriteLine($"Leaf {i - 1} Actual [cm],{String.Join(",", tlog.Header.AxisData.MLCAct.ElementAt(i))}");
                        }
                    }
                    sw.Flush();
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Builds fluence from Trajectory Log File
        /// </summary>
        /// <param name="tlog">Trajectory Log file to read the MLC Positions</param>
        /// <param name="MLCstring">"Expected" or "Actual"</param>
        /// <returns>Returns double array that is the size of the field available to the linac (in mm).
        /// Could be 400x400mm^2 for TrueBeam or 280x280mm^2 for Halcyon.
        /// 1mm resolution may cause issues constructing fluence for HDMLC.
        /// </returns>
        public static double[,] BuildFluence(Specs.TrajectoryLogInfo tlog, string MLCstring)
        {
            int fieldX = 800;
            int fieldY = 800;
            if (tlog.Header.MLCModel == MLCModelEnum.SX2)
            {
                fieldX = 280;
                fieldY = 280;
            }
            if (tlog.Header.MLCModel == MLCModelEnum.NDS120HD)
            {
                fieldX = 440;
                fieldY = 800;
            }
            double[,] fluence = new double[fieldY, fieldX];
            float muStart = 0f;
            float totalMU = MLCstring == "Expected" ? tlog.Header.AxisData.MUExp.First().Max() : tlog.Header.AxisData.MUAct.First().Max();
            //int i = 0;
            var logs = MLCstring == "Expected" ? tlog.Header.AxisData.MLCExp : tlog.Header.AxisData.MLCAct;
            for (int i = 0; i < logs.ElementAt(2).Count(); i++)//loop through MLC control points.
            {
                float muCurrent = tlog.Header.AxisData.MUExp.ElementAt(0).ElementAt(i);
                AddFluenceFromMLCData(tlog.Header.MLCModel, logs, muStart / totalMU, muCurrent / totalMU, i, ref fluence);
                muStart = muCurrent;

            }
            return fluence;
        }
        /// <summary>
        /// Build Simple PDF Report of Trajectory Log Analysis
        /// </summary>
        /// <param name="tlog">Trajectory Log to analyze</param>
        /// <returns>Gantry, MU and MLC statistics and fluence images.</returns>
        public static bool PublishPDF(Specs.TrajectoryLogInfo tlog)
        {
            //Initialize Flow Document
            FlowDocument flowDocument = new FlowDocument() { FontSize = 12, FontFamily = new FontFamily("Arial") };
            flowDocument.Blocks.Add(new Paragraph(new Run($"Trajectory report for {tlog.Header.MetaData.BeamName}")) { FontWeight = FontWeights.Bold, Margin = new Thickness(5) });
            flowDocument.Blocks.Add(new Paragraph(new Run("RMS Results")) { FontWeight = FontWeights.Bold, Margin = new Thickness(5) });
            //include RMS data for Gantry, Collimator, Couch
            double gantryMax = 0.0;
            double gantryMaxLoc = 0.0;
            double gantryRMS = CalculateRMS(tlog.Header.AxisData.GantryRtnActual, tlog.Header.AxisData.GantryRtnExpected, "Gantry", out gantryMax, out gantryMaxLoc);
            double muMax = 0.0;
            double muMaxLoc = 0.0;
            double muRMS = CalculateRMS(tlog.Header.AxisData.MUAct, tlog.Header.AxisData.MUExp, "MU", out muMax, out muMaxLoc);
            double mlcMax = 0.0;
            double mlcMaxLoc = 0.0;
            double mlcRMS = CalculateRMS(tlog.Header.AxisData.MLCAct, tlog.Header.AxisData.MLCExp, "MLC", out mlcMax, out mlcMaxLoc);
            string mlcMaxNum = String.Empty;// Convert.ToInt16(mlcMaxLoc);
            if (tlog.Header.MLCModel == MLCModelEnum.SX2)
            {
                if (mlcMaxLoc > 58)
                {
                    mlcMaxNum = $"Leaf {mlcMaxLoc - 58} X1 Bank";
                }
                else
                {
                    mlcMaxNum = $"Leaf {mlcMaxLoc - 1} X2 Bank";
                }
            }
            else
            {
                if (mlcMaxLoc > 61)
                {
                    mlcMaxNum = $"Leaf {mlcMaxLoc - 61} X1 Bank";
                }
                else
                {
                    mlcMaxNum = $"leaf {mlcMaxLoc - 1} X2 Bank";
                }
            }
            StackPanel rmsSP = new StackPanel();
            rmsSP.Children.Add(new TextBlock
            {
                Text = "Gantry: ",
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"\tMax deviation: {gantryMax:F2}[deg] at {gantryMaxLoc} deg",
                Margin = new Thickness(5)
            });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"\tGantry RMS: {gantryRMS:F3} [deg]",
                Margin = new Thickness(5)
            });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"MU:",
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"\tMax Deviation: {muMax:F3}MU at {muMaxLoc:F2}MU",
                Margin = new Thickness(5)
            });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"\tMU RMS: {muRMS:F3} [MU]",
                Margin = new Thickness(5)
            });
            rmsSP.Children.Add(new TextBlock { Text = "MLC: ", FontWeight = FontWeights.Bold, FontSize = 14 });
            rmsSP.Children.Add(new TextBlock { Text = $"\tMax Deviation: {mlcMax:F3} {mlcMaxNum}", Margin = new Thickness(5) });
            rmsSP.Children.Add(new TextBlock
            {
                Text = $"\tAverage MLC RMS: {mlcRMS:F3} [cm]",
                Margin = new Thickness(5)
            });

            flowDocument.Blocks.Add(new BlockUIContainer(rmsSP));
            //Print visualizations.
            flowDocument.Blocks.Add(new Paragraph(new Run("Fluence Analysis")) { FontWeight = FontWeights.Bold, Margin = new Thickness(5) });
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Children.Add(new TextBlock { Text = "Actual Image", Margin = new Thickness(5) });
            TextBlock tb2 = new TextBlock { Text = "Expected Image", Margin = new Thickness(5) };
            Grid.SetColumn(tb2, 1);
            grid.Children.Add(tb2);
            var actualFluence = BuildFluence(tlog, "Actual");
            var expectedFluence = BuildFluence(tlog, "Expected");
            double res = tlog.Header.MLCModel == MLCModelEnum.SX2 ? 25.4 : 50.8;
            BitmapSource actualImageSource = BuildFluenceImage(actualFluence, res);
            BitmapSource expectedImageSource = BuildFluenceImage(expectedFluence, res);
            Image actualImage = new Image { Source = actualImageSource, Width = 300, Height = 300, Margin = new Thickness(5) };
            Image expectedImage = new Image { Source = expectedImageSource, Width = 300, Height = 300, Margin = new Thickness(5) };
            Grid.SetRow(actualImage, 1);
            Grid.SetRow(expectedImage, 1);
            Grid.SetColumn(expectedImage, 1);
            grid.Children.Add(actualImage);
            grid.Children.Add(expectedImage);
            flowDocument.Blocks.Add(new BlockUIContainer(grid));

            System.Windows.Controls.PrintDialog printer = new System.Windows.Controls.PrintDialog();
            //printer.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
            flowDocument.PageHeight = 1056;
            flowDocument.PageWidth = 816;
            flowDocument.PagePadding = new System.Windows.Thickness(50);
            flowDocument.ColumnGap = 0;
            flowDocument.ColumnWidth = 816;
            IDocumentPaginatorSource source = flowDocument;
            if (printer.ShowDialog() == true)
            {
                printer.PrintDocument(source.DocumentPaginator, "TreatmentPlanReport");
                return true;
            }
            return false;
        }



        #endregion
        #region Helpers
        /// <summary>
        /// Metadata strings will sometimes require the removal of padding or new line characters.
        /// </summary>
        /// <param name="logReader"></param>
        /// <returns></returns>
        private static MetaData ConvertMetaData(BinaryReader logReader)
        {
            var metaData = new MetaData();
            string metaDataString = Encoding.UTF8.GetString(logReader.ReadBytes(headerReserveSize));
            metaDataString = metaDataString.Replace("\0", "");
            //metaDataString = metaDataString.Replace("\n", "");
            metaDataString = metaDataString.Replace("\r", "");
            metaDataString = metaDataString.Replace("\t", "");
            if (bDebug) { Console.WriteLine(metaDataString); }
            string[] metaDataSplit = metaDataString.Split('\n');
            metaData.PatientID = metaDataSplit.ElementAt(0).Split(':').ElementAt(1);
            metaData.PlanName = metaDataSplit.ElementAt(1).Split(':').ElementAt(1);
            metaData.SOPInstanceUID = metaDataSplit.ElementAt(2).Split(':').ElementAt(1);
            metaData.MUPlanned = Convert.ToDouble(metaDataSplit.ElementAt(3).Split(':').ElementAt(1));
            metaData.MURemaining = Convert.ToDouble(metaDataSplit.ElementAt(4).Split(':').ElementAt(1));
            metaData.Energy = metaDataSplit.ElementAt(5).Split(':').ElementAt(1);
            metaData.BeamName = metaDataSplit.ElementAt(6).Split(':').ElementAt(1);
            //foreach (var meta in MetaDataSizes)
            //{
            //    switch (meta.Key)
            //    {
            //        case "PatientID":
            //            metaData.PatientID = Encoding.UTF8.GetString(logReader.ReadBytes(256));
            //            if (bDebug) { Console.WriteLine($"{meta.Key}:{metaData.PatientID}"); }
            //            break;
            //        case "PlanName":
            //            metaData.PlanName = Encoding.UTF8.GetString(logReader.ReadBytes(meta.Value));
            //            if (bDebug) { Console.WriteLine($"{meta.Key}:{metaData.PlanName}"); }
            //            break;

            //    }
            //}
            return metaData;
        }
        /// <summary>
        /// Iterate through all samples in an axis and axes and write data from each axis. 
        /// </summary>
        /// <param name="axisEnumeration"></param>
        /// <param name="samplesPerAxis"></param>
        /// <param name="numberOfSnapshots"></param>
        /// <param name="axisData"></param>
        /// <param name="logReader"></param>
        private static void CollectAxisData(int[] axisEnumeration, int[] samplesPerAxis, int numberOfSnapshots, AxisData axisData, BinaryReader logReader)
        {
            //set up the lists first;
            int axisIterator = 0;

            foreach (int axis in axisEnumeration)
            {
                switch (axis)
                {
                    case 0://collimator
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CollRtnExpected.Add(new float[numberOfSnapshots]);
                            axisData.CollRtnActual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 1://gantry
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.GantryRtnExpected.Add(new float[numberOfSnapshots]);
                            axisData.GantryRtnActual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 2://Y1
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.Y1Expected.Add(new float[numberOfSnapshots]);
                            axisData.Y1Actual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 3://Y2
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.Y2Expected.Add(new float[numberOfSnapshots]);
                            axisData.Y2Actual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 4://x1
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.X1Expected.Add(new float[numberOfSnapshots]);
                            axisData.X1Actual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 5://x2
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.X2Expected.Add(new float[numberOfSnapshots]);
                            axisData.X2Actual.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 6://Couch vrt
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchVrtExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchVrtAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 7://Couch Lng
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchLngExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchLngAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 8://Couch Lat
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchLatExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchLatAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 9://Couch Rtn
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchRtnExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchRtnAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 10://Couch Pit
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchPitExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchPitAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 11://Couch Rol
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.CouchRolExp.Add(new float[numberOfSnapshots]);
                            axisData.CouchRolAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 40://MU
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.MUExp.Add(new float[numberOfSnapshots]);
                            axisData.MUAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 41://Beam Hold
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.BeamHoldExp.Add(new float[numberOfSnapshots]);
                            axisData.BeamHoldAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 42://Control Point
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.ControlPointExp.Add(new float[numberOfSnapshots]);
                            axisData.ControlPointAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 50://MLC
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.MLCExp.Add(new float[numberOfSnapshots]);
                            axisData.MLCAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 60://Target Position
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.TargetPositionExp.Add(new float[numberOfSnapshots]);
                            axisData.TargetPositionAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 61://Tracking Target
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.TrackingTargetExp.Add(new float[numberOfSnapshots]);
                            axisData.TrackingTargetAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 62://Tracking Base
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.TrackingBaseExp.Add(new float[numberOfSnapshots]);
                            axisData.TrackingBaseAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 63://Tracking Phase
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.TrackingPhaseExp.Add(new float[numberOfSnapshots]);
                            axisData.TrackingPhaseAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                    case 64://Tracking Conformity Index
                        for (int sample = 0; sample < samplesPerAxis[axisIterator]; sample++)
                        {
                            axisData.TCIExp.Add(new float[numberOfSnapshots]);
                            axisData.TCIAct.Add(new float[numberOfSnapshots]);
                        }
                        break;
                }
                axisIterator++;
            }
            //loop through snapshots first.

            for (int snapShot = 0; snapShot < numberOfSnapshots; snapShot++)
            {
                //iterate through the axis enumeration to figure out which is the next axis to add to.
                axisIterator = 0;
                foreach (int axis in axisEnumeration)
                {
                    switch (axis)
                    {
                        case 0://collimator
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CollRtnExpected, axisData.CollRtnActual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Collimator Expected: {String.Join(", ", axisData.CollRtnExpected.ElementAt(0))}");
                                Console.WriteLine($"Collimator Actual: {String.Join(", ", axisData.CollRtnActual.ElementAt(0))}");
                            }
                            break;
                        case 1://gantry
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.GantryRtnExpected, axisData.GantryRtnActual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Gantry Rotation Expected: {String.Join(", ", axisData.GantryRtnExpected.ElementAt(0))}");
                                Console.WriteLine($"Gantry Rotation Actual: {String.Join(", ", axisData.GantryRtnActual.ElementAt(0))}");
                            }
                            break;
                        case 2: //Y1
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.Y1Expected, axisData.Y1Actual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Y1 Expected: {String.Join(", ", axisData.Y1Expected.ElementAt(0))}");
                                Console.WriteLine($"Y1 Actual: {String.Join(", ", axisData.Y1Actual.ElementAt(0))}");
                            }
                            break;
                        case 3: //Y2
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.Y2Expected, axisData.Y2Actual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Y2 Expected: {String.Join(", ", axisData.Y2Expected.ElementAt(0))}");
                                Console.WriteLine($"Y2 Actual: {String.Join(", ", axisData.Y2Actual.ElementAt(0))}");
                            }
                            break;
                        case 4: //X1
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.X1Expected, axisData.X1Actual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"X1 Expected: {String.Join(", ", axisData.X1Expected.ElementAt(0))}");
                                Console.WriteLine($"X1 Actual: {String.Join(", ", axisData.X1Actual.ElementAt(0))}");
                            }
                            break;
                        case 5: //X2
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.X2Expected, axisData.X2Actual, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"X2 Expected: {String.Join(", ", axisData.X2Expected.ElementAt(0))}");
                                Console.WriteLine($"X2 Actual: {String.Join(", ", axisData.X2Actual.ElementAt(0))}");
                            }
                            break;
                        case 6: //Couch Vrt
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchVrtExp, axisData.CouchVrtAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Vrt Expected: {String.Join(", ", axisData.CouchVrtExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Vrt Actual: {String.Join(", ", axisData.CouchVrtAct.ElementAt(0))}");
                            }
                            break;
                        case 7: //Couch Lng
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchLngExp, axisData.CouchLngAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Lng Expected: {String.Join(", ", axisData.CouchLngExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Lng Actual: {String.Join(", ", axisData.CouchLngAct.ElementAt(0))}");
                            }
                            break;
                        case 8: //Couch Lat
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchLatExp, axisData.CouchLatAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Lat Expected: {String.Join(", ", axisData.CouchLatExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Lat Actual: {String.Join(", ", axisData.CouchLatAct.ElementAt(0))}");
                            }
                            break;
                        case 9: //Couch Rtn
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchRtnExp, axisData.CouchRtnAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Rtn Expected: {String.Join(", ", axisData.CouchRtnExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Rtn Actual: {String.Join(", ", axisData.CouchRtnAct.ElementAt(0))}");
                            }
                            break;
                        case 10: //Couch Pit
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchPitExp, axisData.CouchPitAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Pit Expected: {String.Join(", ", axisData.CouchPitExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Pit Actual: {String.Join(", ", axisData.CouchPitAct.ElementAt(0))}");
                            }
                            break;
                        case 11: //Couch Rol
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CouchRolExp, axisData.CouchRolAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Couch Roll Expected: {String.Join(", ", axisData.CouchRolExp.ElementAt(0))}");
                                Console.WriteLine($"Couch Roll Actual: {String.Join(", ", axisData.CouchRolAct.ElementAt(0))}");
                            }
                            break;
                        case 40: //MU
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.MUExp, axisData.MUAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"MU Expected: {String.Join(", ", axisData.MUExp.ElementAt(0))}");
                                Console.WriteLine($"MU Actual: {String.Join(", ", axisData.MUAct.ElementAt(0))}");
                            }
                            break;
                        case 41: //Beam Hold
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.BeamHoldExp, axisData.BeamHoldAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Beam Hold Expected: {String.Join(", ", axisData.BeamHoldExp.ElementAt(0))}");
                                Console.WriteLine($"Beam Hold Actual: {String.Join(", ", axisData.BeamHoldAct.ElementAt(0))}");
                            }
                            break;
                        case 42: //Control Point
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.ControlPointExp, axisData.ControlPointAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Control Point Expected: {String.Join(", ", axisData.ControlPointExp.ElementAt(0))}");
                                Console.WriteLine($"Control Point Actual: {String.Join(", ", axisData.ControlPointAct.ElementAt(0))}");
                            }
                            break;
                        case 50: //MLC
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.MLCExp, axisData.MLCAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"MLC Expected: {String.Join(", ", axisData.MLCExp.ElementAt(0))}");
                                Console.WriteLine($"MLC Actual: {String.Join(", ", axisData.MLCAct.ElementAt(0))}");
                            }
                            break;
                        case 60: //Target position
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.TargetPositionExp, axisData.TargetPositionAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Target Position Expected: {String.Join(", ", axisData.TargetPositionExp.ElementAt(0))}");
                                Console.WriteLine($"Target Position Actual: {String.Join(", ", axisData.TargetPositionAct.ElementAt(0))}");
                            }
                            break;
                        case 61: //Tracking Target
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.TrackingTargetExp, axisData.TrackingTargetAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Tracking Target Expected: {String.Join(", ", axisData.TrackingTargetExp.ElementAt(0))}");
                                Console.WriteLine($"Tracking Target Actual: {String.Join(", ", axisData.TrackingTargetAct.ElementAt(0))}");
                            }
                            break;
                        case 62: //Tracking Base
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.TrackingBaseExp, axisData.TrackingBaseAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Tracking Base Expected: {String.Join(", ", axisData.TrackingBaseExp.ElementAt(0))}");
                                Console.WriteLine($"Tracking Base Actual: {String.Join(", ", axisData.TrackingBaseAct.ElementAt(0))}");
                            }
                            break;
                        case 63: //Tracking Phase
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.TrackingPhaseExp, axisData.TrackingPhaseAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Tracking Phase Expected: {String.Join(", ", axisData.TrackingPhaseExp.ElementAt(0))}");
                                Console.WriteLine($"Tracking Phase Actual: {String.Join(", ", axisData.TrackingPhaseAct.ElementAt(0))}");
                            }
                            break;
                        case 64: //Tracking Conformity Index
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.TCIExp, axisData.TCIAct, logReader);
                            if (bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Tracking Conformity Index Expected: {String.Join(", ", axisData.TCIExp.ElementAt(0))}");
                                Console.WriteLine($"Tracking Conformity Index Actual: {String.Join(", ", axisData.TCIAct.ElementAt(0))}");
                            }
                            break;
                    }
                    axisIterator++;
                }
            }
        }
        /// <summary>
        /// Convert numerical data in each axis to readable values. 
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="sampleCount"></param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="logReader"></param>
        private static void GetAxisData(int snapshot, int sampleCount, List<float[]> expected, List<float[]> actual, BinaryReader logReader)
        {
            //loop through number of samples
            for (int sample = 0; sample < sampleCount; sample++)
            {
                expected.ElementAt(sample)[snapshot] = BitConverter.ToSingle(logReader.ReadBytes(4), 0);
                actual.ElementAt(sample)[snapshot] = BitConverter.ToSingle(logReader.ReadBytes(4), 0);
            }

        }
        /// <summary>
        /// Translate MLC information to fluence.
        /// </summary>
        /// <param name="mLCModel"></param>
        /// <param name="mlcCollections"></param>
        /// <param name="muStart"></param>
        /// <param name="muCurrent"></param>
        /// <param name="cp"></param>
        /// <param name="fluence"></param>
        private static void AddFluenceFromMLCData(MLCModelEnum mLCModel, List<float[]> mlcCollections, float muStart, float muCurrent, int cp, ref double[,] fluence)
        {
            if (mLCModel == MLCModelEnum.SX2)
            {
                //go up from the bottom of the fluence map, determine the position of each leaf and use the leaf above and below to determine how many rows will receive fluence.
                for (int i = 0; i < 28; i++)
                {
                    //starting from the bottom at position 280
                    //leaf model behavior: 
                    //Leaf 1 -> 28  Proximal Bank X2
                    //Leaf 29 -> 57 Distal Bank X2
                    //Leaf 58->85 Proximal Bank X1
                    //Leaf 86->114 Distal Bank X1
                    //no need to ever use 29, 57, 86, or 114 as they are out of the 28x28 field size.                          
                    //first 5mm

                    int colStart = fluence.GetLength(0) / 2 - Convert.ToInt32(mlcCollections.ElementAt(57 + i + 2).ElementAt(cp) * 10);//- sign is Varian Scale conversion. 
                    int colEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(mlcCollections.ElementAt(i + 2).ElementAt(cp) * 10);//we need +2 to skip the carriages.
                    int colAfterStart = fluence.GetLength(0) / 2 - Convert.ToInt32(mlcCollections.ElementAt(57 + i + 31).ElementAt(cp) * 10);
                    int colAfterEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(mlcCollections.ElementAt(i + 31).ElementAt(cp) * 10);
                    int colBeforeStart = fluence.GetLength(0) / 2 - Convert.ToInt32(mlcCollections.ElementAt(57 + i + 30).ElementAt(cp) * 10);
                    int colBeforeEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(mlcCollections.ElementAt(i + 30).ElementAt(cp) * 10);
                    int rows = fluence.GetLength(1) - 1;
                    //determine delimiter for first 5
                    int lowerFluenceStart = colStart > colBeforeStart ? colStart : colBeforeStart;
                    int upperFluenceStart = colStart > colAfterStart ? colStart : colAfterStart;
                    int lowerFluenceEnd = colEnd < colBeforeEnd ? colEnd : colBeforeEnd;
                    int upperFluenceEnd = colEnd < colAfterEnd ? colEnd : colAfterEnd;
                    int rowStart = i * 10;
                    for (int ii = 0; ii < 5; ii++)//add the MU for 5 mm
                    {
                        for (int iii = lowerFluenceStart; iii < lowerFluenceEnd; iii++)
                        {
                            fluence[iii, rows - (rowStart + ii)] += muCurrent - muStart;
                        }
                    }
                    for (int ii = 5; ii < 10; ii++)
                    {
                        for (int iii = upperFluenceStart; iii < upperFluenceEnd; iii++)
                        {
                            fluence[iii, rows - (rowStart + ii)] += muCurrent - muStart;
                        }
                    }
                }
                //for testing only. 
                //for(int i = 0; i < 20; i++)
                //{
                //    fluence[i, i] = 10;
                //    fluence[i, 100] = 20;
                //    fluence[200, i] = 40;
                //}
            }
            else if (mLCModel == MLCModelEnum.NDS120 || mLCModel == MLCModelEnum.NDS120HD)
            {
                bool isEdge = mLCModel == MLCModelEnum.NDS120HD;
                //loop through leaves from bottom of the field.
                //starting from the bottom at position 280
                //leaf model behavior: 
                //Edge -> 1-14: 0.5cm
                //      -> 15 - 46: 0.25cm
                //      -> 47 - 60: 0.5cm
                //Standard Millennium -> 1-10: 1.0cm
                //      -> 11-50: 0.5cm
                //      -> 51-60: 1.0cm
                //Leaf 1 -> 28  Proximal Bank X2
                //all leaf widths multiplied by 2 in order to keep integer values. 
                int[] mlcWidths = new int[60];
                for (int i = 0; i < 10; i++)
                {
                    mlcWidths[i] = isEdge ? 10 : 20;
                }
                for (int i = 10; i < 14; i++)
                {
                    mlcWidths[i] = isEdge ? 10 : 10;
                }
                for (int i = 14; i < 46; i++)
                {
                    mlcWidths[i] = isEdge ? 5 : 10;
                }
                for (int i = 46; i < 50; i++)
                {
                    mlcWidths[i] = isEdge ? 10 : 10;
                }
                for (int i = 50; i < 60; i++)
                {
                    mlcWidths[i] = isEdge ? 10 : 20;
                }
                //loop through MLC
                int currentRow = 0;
                for (int i = 0; i < 60; i++)
                {
                    int rowEnd = currentRow + mlcWidths[i];
                    int colStart = fluence.GetLength(0) / 2 - Convert.ToInt32(mlcCollections.ElementAt(i + 62).ElementAt(cp) * 20);
                    //int colStart = fluence.GetLength(0) / 2 + Convert.ToInt32(leafPositions[0, i] * 1) * 2;//do not need (-) sign as not in Varian Scale conversion. 
                    int colEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(mlcCollections.ElementAt(i + 2).ElementAt(cp) * 20);
                    //int colEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(leafPositions[1, i] * 1) * 2;//we need +2 to skip the carriages.
                    int rows = fluence.GetLength(1);
                    for (int j = currentRow; j < rowEnd; j++)//row loop
                    {
                        for (int jj = colStart; jj < colEnd; jj++)
                        {
                            fluence[jj, rows - j] += muCurrent - muStart;
                        }
                    }
                    currentRow = rowEnd;
                }
                //double rowStart = mLCModel == MLCModelEnum.NDS120 ? 0.0 : 90.0;
                //for (int i = 0; i < 60; i++)
                //{
                //    //float[0,*] is x1 leaf, float [1,*] is x2 leaf.
                //    //X1 leaves in trajectory log start after all x2 leaves.
                //    int colStart = fluence.GetLength(0) / 2 - Convert.ToInt32(mlcCollections.ElementAt(i + 62).ElementAt(cp) * 10);
                //    int colEnd = fluence.GetLength(0) / 2 + Convert.ToInt32(mlcCollections.ElementAt(i + 2).ElementAt(cp) * 10);//+2 to skip the carriages.
                //    //for NDS120, the first 10 leaves are 1cm, next 40 are 0.5cm and next 10 are 1cm. 
                //    //for NDSHD, the first 14 leavs are 0.5cm, next 32 are 0.25cm, and final 14 are 0.5cm
                //    int halfLeafStart = mLCModel == MLCModelEnum.NDS120 ? 10 : 14;
                //    int halfLeafEnd = mLCModel == MLCModelEnum.NDS120 ? 49 : 55;
                //    int step = mLCModel == MLCModelEnum.NDS120 ? 10 : 5;
                //    double halfStep = mLCModel == MLCModelEnum.NDS120 ? 5 : 2.5;
                //    if (i < halfLeafStart || i > halfLeafEnd)
                //    {
                //        for (int ii = 0; ii < step; ii++)
                //        {
                //            for (int iii = colStart; iii < colEnd; iii++)
                //            {
                //                fluence[iii, (int)rowStart + ii] += muCurrent - muStart;
                //            }
                //        }
                //        rowStart += (double)step;
                //    }
                //    else
                //    {
                //        for (int ii = 0; ii < halfStep; ii++)
                //        {
                //            for (int iii = colStart; iii < colEnd; iii++)
                //            {
                //                fluence[iii, (int)rowStart + ii] += muCurrent - muStart;
                //            }
                //        }
                //        rowStart += halfStep;
                //    }

                //}
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Retrieve RMS devations from trajectory log axis data.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="parameter"></param>
        /// <param name="maxDiff"></param>
        /// <param name="maxDiffLoc"></param>
        /// <returns></returns>
        private static double CalculateRMS(List<float[]> actual, List<float[]> expected, string parameter, out double maxDiff, out double maxDiffLoc)
        {
            if (parameter != "MLC")
            {
                var actualData = actual.First();
                var expectedData = expected.First();
                List<double> squares = new List<double>();
                List<Tuple<double, double>> differences = new List<Tuple<double, double>>();
                for (int i = 0; i < actualData.Length; i++)
                {
                    squares.Add((actualData[i] - expectedData[i]) * (actualData[i] - expectedData[i]));
                    differences.Add(new Tuple<double, double>(actualData[i] - expectedData[i], actualData[i]));
                }
                var maxTuple = differences.OrderByDescending(d => Math.Abs(d.Item1)).First();
                maxDiff = maxTuple.Item1;
                maxDiffLoc = maxTuple.Item2;
                return Math.Sqrt(squares.Average());
            }
            else
            {
                List<double> squares = new List<double>();
                List<Tuple<double, double>> differences = new List<Tuple<double, double>>();
                for (int i = 2; i < actual.Count(); i++)
                {
                    List<double> leafSquares = new List<double>();
                    var actualData = actual.ElementAt(i);
                    var expectedData = expected.ElementAt(i);
                    for (int j = 0; j < actualData.Length; j++)
                    {
                        leafSquares.Add((actualData[j] - expectedData[j]) * (actualData[j] - expectedData[j]));
                        differences.Add(new Tuple<double, double>(actualData[j] - expectedData[j], i));
                    }
                    squares.Add(Math.Sqrt(leafSquares.Average()));
                }
                var maxTuple = differences.OrderByDescending(d => Math.Abs(d.Item1)).First();
                maxDiff = maxTuple.Item1;
                maxDiffLoc = maxTuple.Item2;
                return squares.Average();
            }
        }
        private static BitmapSource BuildFluenceImage(double[,] fluence, double res)
        {
            double[] flat_pixels = new double[fluence.GetLength(0) * fluence.GetLength(1)];
            //lay out pixels into single array
            for (int i = 0; i < fluence.GetLength(0); i++)
            {
                for (int ii = 0; ii < fluence.GetLength(1); ii++)
                {
                    flat_pixels[i + fluence.GetLength(0) * ii] = fluence[i, ii];
                }
            }
            //translate into byte array
            var drr_max = flat_pixels.Max();
            var drr_min = flat_pixels.Min();
            PixelFormat format = PixelFormats.Gray8;//low res image, but only 1 byte per pixel. 
            int stride = (fluence.GetLength(0) * format.BitsPerPixel + 7) / 8;
            byte[] image_bytes = new byte[stride * fluence.GetLength(0)];
            for (int i = 0; i < flat_pixels.Length; i++)
            {
                double value = flat_pixels[i];
                image_bytes[i] = Convert.ToByte(255 * ((value - drr_min) / (drr_max - drr_min)));
            }
            //build the bitmapsource.
            return BitmapSource.Create(fluence.GetLength(0), fluence.GetLength(1), res, res, format, null, image_bytes, stride);
        }
        private static AxisData IterateAxisData(int[] axisEnumeration, AxisData axisData, int start_index, int end_index)
        {
            AxisData ad = new AxisData();
            foreach (int axis in axisEnumeration)
            {
                switch (axis)
                {
                    case 0://collimator
                        float[] coll_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CollRtnExpected.First(), start_index, coll_exp, 0, end_index - start_index);
                        ad.CollRtnExpected.Add(coll_exp);
                        float[] coll_act = new float[end_index - start_index];
                        Array.Copy(axisData.CollRtnActual.First(), start_index, coll_act, 0, end_index - start_index);
                        ad.CollRtnActual.Add(coll_act);
                        break;
                    case 1://gantry
                        float[] gantry_exp = new float[end_index - start_index];
                        Array.Copy(axisData.GantryRtnExpected.First(), start_index, gantry_exp, 0, end_index - start_index);
                        ad.GantryRtnExpected.Add(gantry_exp);
                        float[] gantry_act = new float[end_index - start_index];
                        Array.Copy(axisData.GantryRtnActual.First(), start_index, gantry_act, 0, end_index - start_index);
                        ad.GantryRtnActual.Add(gantry_act);
                        break;
                    case 2://Y1
                        float[] y1_exp = new float[end_index - start_index];
                        Array.Copy(axisData.Y1Expected.First(), start_index, y1_exp, 0, end_index - start_index);
                        ad.Y1Expected.Add(y1_exp);
                        float[] y1_act = new float[end_index - start_index];
                        Array.Copy(axisData.Y1Actual.First(), start_index, y1_act, 0, end_index - start_index);
                        ad.Y1Actual.Add(y1_act);
                        break;
                    case 3://Y2
                        float[] y2_exp = new float[end_index - start_index];
                        Array.Copy(axisData.Y2Expected.First(), start_index, y2_exp, 0, end_index - start_index);
                        ad.Y2Expected.Add(y2_exp);
                        float[] y2_act = new float[end_index - start_index];
                        Array.Copy(axisData.Y2Actual.First(), start_index, y2_act, 0, end_index - start_index);
                        ad.Y2Actual.Add(y2_act);
                        break;
                    case 4://x1
                        float[] x1_exp = new float[end_index - start_index];
                        Array.Copy(axisData.X1Expected.First(), start_index, x1_exp, 0, end_index - start_index);
                        ad.X1Expected.Add(x1_exp);
                        float[] x1_act = new float[end_index - start_index];
                        Array.Copy(axisData.X1Actual.First(), start_index, x1_act, 0, end_index - start_index);
                        ad.X1Actual.Add(x1_act);
                        break;
                    case 5://x2
                        float[] x2_exp = new float[end_index - start_index];
                        Array.Copy(axisData.X2Expected.First(), start_index, x2_exp, 0, end_index - start_index);
                        ad.X2Expected.Add(x2_exp);
                        float[] x2_act = new float[end_index - start_index];
                        Array.Copy(axisData.X2Actual.First(), start_index, x2_act, 0, end_index - start_index);
                        ad.X2Actual.Add(x2_act);
                        break;
                    case 6://Couch vrt
                        float[] vrt_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchVrtExp.First(), start_index, vrt_exp, 0, end_index - start_index);
                        ad.CouchVrtExp.Add(vrt_exp);
                        float[] vrt_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchVrtAct.First(), start_index, vrt_act, 0, end_index - start_index);
                        ad.CouchVrtAct.Add(vrt_act);
                        break;
                    case 7://Couch Lng
                        float[] lng_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchLngExp.First(), start_index, lng_exp, 0, end_index - start_index);
                        ad.CouchLngExp.Add(lng_exp);
                        float[] lng_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchLngAct.First(), start_index, lng_act, 0, end_index - start_index);
                        ad.CouchLngAct.Add(lng_act);
                        break;
                    case 8://Couch Lat
                        float[] lat_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchLatExp.First(), start_index, lat_exp, 0, end_index - start_index);
                        ad.CouchLatExp.Add(lat_exp);
                        float[] lat_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchLatAct.First(), start_index, lat_act, 0, end_index - start_index);
                        ad.CouchLatAct.Add(lat_act);
                        break;
                    case 9://Couch Rtn
                        float[] rtn_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchRtnExp.First(), start_index, rtn_exp, 0, end_index - start_index);
                        ad.CouchRtnExp.Add(rtn_exp);
                        float[] rtn_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchRtnAct.First(), start_index, rtn_act, 0, end_index - start_index);
                        ad.CouchRtnAct.Add(rtn_act);
                        break;
                    case 10://Couch Pit
                        float[] pitch_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchPitExp.First(), start_index, pitch_exp, 0, end_index - start_index);
                        ad.CouchPitExp.Add(pitch_exp);
                        float[] pitch_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchPitAct.First(), start_index, pitch_act, 0, end_index - start_index);
                        ad.CouchPitAct.Add(pitch_act);
                        break;
                    case 11://Couch Rol
                        float[] rol_exp = new float[end_index - start_index];
                        Array.Copy(axisData.CouchRolExp.First(), start_index, rol_exp, 0, end_index - start_index);
                        ad.CouchRolExp.Add(rol_exp);
                        float[] rol_act = new float[end_index - start_index];
                        Array.Copy(axisData.CouchRolAct.First(), start_index, rol_act, 0, end_index - start_index);
                        ad.CouchRolAct.Add(rol_act);
                        break;
                    case 40://MU
                        float[] mu_exp = new float[end_index - start_index];
                        Array.Copy(axisData.MUExp.First(), start_index, mu_exp, 0, end_index - start_index);
                        ad.MUExp.Add(mu_exp);
                        float[] mu_act = new float[end_index - start_index];
                        Array.Copy(axisData.MUAct.First(), start_index, mu_act, 0, end_index - start_index);
                        ad.MUAct.Add(mu_act);
                        break;
                    case 41://Beam Hold
                        float[] bh_exp = new float[end_index - start_index];
                        Array.Copy(axisData.BeamHoldExp.First(), start_index, bh_exp, 0, end_index - start_index);
                        ad.BeamHoldExp.Add(bh_exp);
                        float[] bh_act = new float[end_index - start_index];
                        Array.Copy(axisData.BeamHoldAct.First(), start_index, bh_act, 0, end_index - start_index);
                        ad.BeamHoldAct.Add(bh_act);
                        break;
                    case 42://Control Point
                        float[] cp_exp = new float[end_index - start_index];
                        Array.Copy(axisData.ControlPointExp.First(), start_index, cp_exp, 0, end_index - start_index);
                        ad.ControlPointExp.Add(cp_exp);
                        float[] cp_act = new float[end_index - start_index];
                        Array.Copy(axisData.ControlPointAct.First(), start_index, cp_act, 0, end_index - start_index);
                        ad.ControlPointAct.Add(cp_act);
                        break;
                    case 50://MLC
                        foreach (var mlc_leaf in axisData.MLCExp)
                        {
                            float[] mlc_exp = new float[end_index - start_index];
                            Array.Copy(axisData.MLCExp.First(), start_index, mlc_exp, 0, end_index - start_index);
                            ad.MLCExp.Add(mlc_exp);
                        }
                        foreach (var leaf in axisData.MLCAct)
                        {
                            float[] mlcAct = new float[end_index - start_index];
                            Array.Copy(axisData.MLCAct.First(), start_index, mlcAct, 0, end_index - start_index);
                            ad.MLCAct.Add(mlcAct);
                        }
                        break;
                    case 60://Target Position
                        float[] tp_exp = new float[end_index - start_index];
                        Array.Copy(axisData.TargetPositionExp.First(), start_index, tp_exp, 0, end_index - start_index);
                        ad.TargetPositionExp.Add(tp_exp);
                        float[] tp_act = new float[end_index - start_index];
                        Array.Copy(axisData.TargetPositionAct.First(), start_index, tp_act, 0, end_index - start_index);
                        ad.TargetPositionAct.Add(tp_act);
                        break;
                    case 61://Tracking Target
                        float[] tt_exp = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingTargetExp.First(), start_index, tt_exp, 0, end_index - start_index);
                        ad.TrackingTargetExp.Add(tt_exp);
                        float[] tt_act = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingTargetAct.First(), start_index, tt_act, 0, end_index - start_index);
                        ad.TrackingTargetAct.Add(tt_act);
                        break;
                    case 62://Tracking Base
                        float[] tb_exp = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingBaseExp.First(), start_index, tb_exp, 0, end_index - start_index);
                        ad.TrackingBaseExp.Add(tb_exp);
                        float[] tb_act = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingBaseAct.First(), start_index, tb_act, 0, end_index - start_index);
                        ad.TrackingBaseAct.Add(tb_act);
                        break;
                    case 63://Tracking Phase
                        float[] trackp_exp = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingPhaseExp.First(), start_index, trackp_exp, 0, end_index - start_index);
                        ad.TrackingPhaseExp.Add(trackp_exp);
                        float[] trackp_act = new float[end_index - start_index];
                        Array.Copy(axisData.TrackingPhaseAct.First(), start_index, trackp_act, 0, end_index - start_index);
                        ad.TrackingPhaseAct.Add(trackp_act);
                        break;
                    case 64://Tracking Conformity Index
                        float[] tci_exp = new float[end_index - start_index];
                        Array.Copy(axisData.TCIExp.First(), start_index, tci_exp, 0, end_index - start_index);
                        ad.TCIExp.Add(tci_exp);
                        float[] tci_act = new float[end_index - start_index];
                        Array.Copy(axisData.TCIAct.First(), start_index, tci_act, 0, end_index - start_index);
                        ad.TCIAct.Add(tci_act);
                        break;
                }
            }
            return ad;
        }
        #endregion
    }

}
