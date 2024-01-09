using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrajectoryLog.NET.TrajectoryLog.Specifications;
using TrajectoryLog.NET.TrajectoryLog.Specifications.AxisHelpers;
using TrajectoryLog.NET.TrajectoryLog.Specifications.Headers;
using Specs = TrajectoryLog.NET.TrajectoryLog.Specifications;

namespace TrajectoryLog.NET
{
    public static class Trajectory
    {
        #region Statics
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
        public static void EnableDebug()
        {
            bDebug = true;
        }
        public static void DisableDebug()
        {
            bDebug = false;
        }
        /// <summary>
        /// Reads file from file system and converts to a trajectory log
        /// </summary>
        /// <param name="fileName">Filename of BIN file.</param>
        /// <returns>Trajectory Log from TrajectoryLog.Specticiations.TrajectorLog</returns>
        public static Specs.TrajectoryLog LoadLog(string fileName)
        {
            //instantiate the log file
            Specs.TrajectoryLog tLog = new Specs.TrajectoryLog();
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
                                    //switch (i)
                                    //{
                                    //    case 0://coll
                                    //    case 1://gantry
                                    //    case 2://y1
                                    //    case 3://y2
                                    //    case 4://x1
                                    //    case 5://x2
                                    //    case 6://vrt
                                    //    case 7://lng
                                    //    case 8://lat
                                    //    case 9://rtn
                                    //    case 10://pit
                                    //    case 11://rol
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 12://mu
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 13://beam hold
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 14://control point
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 15://mlc
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 16://target position
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 17://tracking target
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 18://tracking base
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 19://tracking phase
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    case 20://racking conformity index
                                    //        tLog.Header.AxisEnumeration[i] = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                                    //        break;
                                    //    default:
                                    //        tLog.HeaderError.Append($"Invalied Axis Enumeration {i}");
                                    //        break;
                                    //}
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
                                            sb.Name = BitConverter.ToString(logReader.ReadBytes(sub.Value), 0);
                                            if (bDebug) { Console.WriteLine($"{sub.Key}: {sb.Name}"); }
                                            break;

                                    }
                                }
                                tLog.Header.Subbeams.Add(sb);
                            }
                            break;
                        case "Snapshot":
                            CollectAxisData(tLog.Header.AxisEnumeration, tLog.Header.SamplesPerAxis, tLog.Header.NumberOfSnapshots, tLog.Header.AxisData, logReader);
                            //foreach (var axis in tLog.Header.AxisEnumeration)
                            //{
                            //    switch (axis)
                            //    {
                            //        case 0://collimator
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CollRtnExpected, tLog.Header.AxisData.CollRtnActual, logReader, "Collimator");
                            //            break;
                            //        case 1: //gantry
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.GantryRtnExpected, tLog.Header.AxisData.GantryRtnActual, logReader, "Gantry");
                            //            break;
                            //        case 2: //y1
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.Y1Expected, tLog.Header.AxisData.Y1Actual, logReader, "y1");
                            //            break;
                            //        case 3: //y2
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.Y2Expected, tLog.Header.AxisData.Y2Actual, logReader, "y2");
                            //            break;
                            //        case 4: //x1
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.X1Expected, tLog.Header.AxisData.X1Actual, logReader, "x1");
                            //            break;
                            //        case 5: //x2
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.X2Expected, tLog.Header.AxisData.X2Actual, logReader, "x2");
                            //            break;
                            //        case 6: //Couch Vrt
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchVrtExp, tLog.Header.AxisData.CouchVrtAct, logReader, "Couch Vrt");
                            //            break;
                            //        case 7: //Couch Lng
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchLngExp, tLog.Header.AxisData.CouchLngAct, logReader, "Couch Lng");
                            //            break;
                            //        case 8: //Couch Lat
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchLatExp, tLog.Header.AxisData.CouchLatAct, logReader, "Couch Lat");
                            //            break;
                            //        case 9: //Couch Rtn
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchRtnExp, tLog.Header.AxisData.CouchRtnAct, logReader, "Couch Rtn");
                            //            break;
                            //        case 10: //Couch Pit
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchPitExp, tLog.Header.AxisData.CouchPitAct, logReader, "Couch Pit");
                            //            break;
                            //        case 11: //Couch Roll
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.CouchRolExp, tLog.Header.AxisData.CouchRolAct, logReader, "Couch Roll");
                            //            break;
                            //        case 12: //MU
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.MUExp, tLog.Header.AxisData.MUAct, logReader, "MU");
                            //            break;
                            //        case 13: //Beam Hold
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.BeamHoldExp, tLog.Header.AxisData.BeamHoldAct, logReader, "beam hold");
                            //            break;
                            //        case 14: //Ctrl Point
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.ControlPointExp, tLog.Header.AxisData.ControlPointAct, logReader, "Ctrl Point");
                            //            break;
                            //        case 15: //MLC
                            //           CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.MLCExp, tLog.Header.AxisData.MLCAct, logReader, "MLC");
                            //           break;
                            //        case 16: //Target Position
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.TargetPositionExp, tLog.Header.AxisData.TargetPositionAct, logReader, "Target Position");
                            //            break;
                            //        case 17: //Tracking Target
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.TrackingTargetExp, tLog.Header.AxisData.TrackingTargetAct, logReader, "Tracking target");
                            //            break;
                            //        case 18: //Tracking Base
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.TrackingBaseExp, tLog.Header.AxisData.TrackingBaseAct, logReader, "Tracking Base");
                            //            break;
                            //        case 19: //Tracking Phase
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.TrackingPhaseExp, tLog.Header.AxisData.TrackingPhaseAct, logReader, "Tracking Phase");
                            //            break;
                            //        case 20: //Tracking Conformity Index
                            //            CollectAxisData(tLog.Header.SamplesPerAxis[axis], tLog.Header.NumberOfSnapshots, tLog.Header.AxisData.TCIExp, tLog.Header.AxisData.TCIAct, logReader, "Tracking Conformity Index");
                            //            break;

                            //    }
                            //}
                            break;
                        default:
                            tLog.HeaderError.Append($"Unsupported: {data.Key}");
                            break;

                    }
                }
            }
            return tLog;
        }

        public static bool ToCSV(Specs.TrajectoryLog tlog)
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
                        if(tlog.Header.MLCModel != MLCModelEnum.SX2 && i == 0)
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


        #endregion
        #region Helpers
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
                        for(int sample = 0; sample<samplesPerAxis[axisIterator]; sample++)
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
                            GetAxisData(snapShot, samplesPerAxis[axisIterator], axisData.CollRtnExpected, axisData.CollRtnActual,logReader);
                            if(bDebug && snapShot == numberOfSnapshots - 1)
                            {
                                Console.WriteLine($"Collimator Expected: {String.Join(", ",axisData.CollRtnExpected.ElementAt(0))}");
                                Console.WriteLine($"Collimator Actual: {String.Join(", ",axisData.CollRtnActual.ElementAt(0))}");
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
        private static void GetAxisData(int snapshot, int sampleCount, List<float[]> expected, List<float[]> actual, BinaryReader logReader)
        {
            //loop through number of samples
            for (int sample = 0; sample < sampleCount; sample++)
            {
                expected.ElementAt(sample)[snapshot] = BitConverter.ToSingle(logReader.ReadBytes(4), 0);
                actual.ElementAt(sample)[snapshot] = BitConverter.ToSingle(logReader.ReadBytes(4), 0);
            }

        }
        #endregion
    }

}
