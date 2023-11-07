using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            {"AxisEnumeration",4 }//number of axis * 4 bytes

        };
        #endregion
        #region APIFunctions
        /// <summary>
        /// Reads file from file system and converts to a trajectory log
        /// </summary>
        /// <param name="fileName">Filename of BIN file.</param>
        /// <returns>Trajectory Log from TrajectoryLog.Specticiations.TrajectorLog</returns>
        public static Specs.TrajectoryLog LoadLog(string fileName)
        {
            //instantiate the log file
            Specs.TrajectoryLog tLog = new Specs.TrajectoryLog();
            tLog.Header = new Specs.TrajectoryHeader();
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
                            break;
                        case "Version":
                            tLog.Header.Version = Encoding.ASCII.GetString(logReader.ReadBytes(data.Value));
                            break;
                        case "HeaderSize":
                            tLog.Header.HeaderSize = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            break;
                        case "SamplingIntervalMS":
                            tLog.Header.SampleIntervalMS = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            break;
                        case "NumberOfAxesSampled":
                            tLog.Header.NumberOfAxesSampled = BitConverter.ToInt32(logReader.ReadBytes(data.Value), 0);
                            break;
                        case "AxisEnumeration":
                            if (tLog.Header.NumberOfAxesSampled != 0)
                            {
                                tLog.Header.AxisEnumeration = new int[tLog.Header.NumberOfAxesSampled];
                                for(int i = 0; i < tLog.Header.NumberOfAxesSampled;i++)
                                {
                                    switch (i)
                                    {
                                        case 0://coll
                                        case 1://gantry
                                        case 2://y1
                                        case 3://y2
                                        case 4://x1
                                        case 5://x2
                                        case 6://vrt
                                        case 7://lng
                                        case 8://lat
                                        case 9://rtn
                                        case 10://pit
                                        case 11://rol
                                            tLog.Header.AxisEnumeration[i] = i;
                                            break;
                                        case 12://mu
                                            tLog.Header.AxisEnumeration[i] = 40;
                                            break;
                                        case 13://beam hold
                                            tLog.Header.AxisEnumeration[i] = 41;
                                            break;
                                        case 14://control point
                                            tLog.Header.AxisEnumeration[i] = 42;
                                            break;
                                        case 15://mlc
                                            tLog.Header.AxisEnumeration[i] = 50;
                                            break;
                                        case 16://target position
                                            tLog.Header.AxisEnumeration[i] = 60;
                                            break;
                                        case 17://tracking target
                                            tLog.Header.AxisEnumeration[i] = 61;
                                            break;
                                        case 18://tracking base
                                            tLog.Header.AxisEnumeration[i] = 62;
                                            break;
                                        case 19://tracking phase
                                            tLog.Header.AxisEnumeration[i] = 63;
                                            break;
                                        case 20://racking conformity index
                                            tLog.Header.AxisEnumeration[i] = 64;
                                            break;
                                        default:
                                            tLog.HeaderError.Append($"Invalied Axis Enumeration {i}");
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                tLog.HeaderError.Append("Axis enumeration attempted without number of axes known. ");
                            }
                            break;
                        default:
                            tLog.HeaderError.Append($"Unsupported: {data.Key}");
                            break;

                    }
                }
            }
            return tLog;
        }
        #endregion
    }

}
