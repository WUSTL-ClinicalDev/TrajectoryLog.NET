using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrajectoryLog.NET.TrajectoryLog.Specifications.AxisHelpers
{
    public class AxisData
    {
        public List<float[]> CollRtnExpected { get; set; }
        public List<float[]> CollRtnActual { get; set; }
        public List<float[]> GantryRtnExpected { get; set; }
        public List<float[]> GantryRtnActual { get; set; }
        public List<float[]> Y1Expected { get; set; }
        public List<float[]> Y1Actual { get; set; }
        public List<float[]> Y2Expected { get; set; }
        public List<float[]> Y2Actual { get; set; }
        public List<float[]> X1Expected { get; set; }
        public List<float[]> X1Actual { get; set; }
        public List<float[]> X2Expected { get; set; }
        public List<float[]> X2Actual { get; set; }
        public List<float[]> CouchVrtExp { get; set; }
        public List<float[]> CouchVrtAct { get; set; }
        public List<float[]> CouchLngExp { get; set; }
        public List<float[]> CouchLngAct { get; set; }
        public List<float[]> CouchLatExp { get; set; }
        public List<float[]> CouchLatAct { get; set; }
        public List<float[]> CouchRtnExp { get; set; }
        public List<float[]> CouchRtnAct { get; set; }
        public List<float[]> CouchPitExp { get; set; }
        public List<float[]> CouchPitAct { get; set; }
        public List<float[]> CouchRolExp { get; set; }
        public List<float[]> CouchRolAct { get; set; }
        public List<float[]> MUExp { get; set; }
        public List<float[]> MUAct { get; set; }
        public List<float[]> BeamHoldExp { get; set; }
        public List<float[]> BeamHoldAct { get; set; }
        public List<float[]> ControlPointExp { get; set; }
        public List<float[]> ControlPointAct { get; set; }
        public List<float[]> MLCExp { get; set; }
        public List<float[]> MLCAct { get; set; }
        public List<float[]> TargetPositionExp { get; set; }
        public List<float[]> TargetPositionAct { get; set; }
        public List<float[]> TrackingTargetExp { get; set; }
        public List<float[]> TrackingTargetAct { get; set; }
        public List<float[]> TrackingBaseExp { get; set; }
        public List<float[]> TrackingBaseAct { get; set; }
        public List<float[]> TrackingPhaseExp { get; set; }
        public List<float[]> TrackingPhaseAct { get; set; }
        public List<float[]> TCIExp { get; set; } //TrackingConformityIndexExpected
        public List<float[]> TCIAct { get; set; } //TrackingConformityIndexActual

        public AxisData()
        {
            CollRtnExpected = new List<float[]>();
            CollRtnActual = new List<float[]>();
            GantryRtnExpected = new List<float[]>();
            GantryRtnActual = new List<float[]>();
            Y1Expected = new List<float[]>();
            Y1Actual = new List<float[]>();
            Y2Expected = new List<float[]>();
            Y2Actual = new List<float[]>();
            X1Expected = new List<float[]>();
            X1Actual = new List<float[]>();
            X2Expected = new List<float[]>();
            X2Actual = new List<float[]>();
            CouchVrtExp = new List<float[]>();
            CouchVrtAct = new List<float[]>();
            CouchLngExp = new List<float[]>();
            CouchLngAct = new List<float[]>();
            CouchLatExp = new List<float[]>();
            CouchLatAct = new List<float[]>();
            CouchRtnExp = new List<float[]>();
            CouchRtnAct = new List<float[]>();
            CouchPitExp = new List<float[]>();
            CouchPitAct = new List<float[]>();
            CouchRolExp = new List<float[]>();
            CouchRolAct = new List<float[]>();
            MUExp = new List<float[]>();
            MUAct = new List<float[]>();
            BeamHoldExp = new List<float[]>();
            BeamHoldAct = new List<float[]>();
            ControlPointExp = new List<float[]>();
            ControlPointAct = new List<float[]>();
            MLCExp = new List<float[]>();
            MLCAct = new List<float[]>();
            TargetPositionExp = new List<float[]>();
            TargetPositionAct = new List<float[]>();
            TrackingTargetExp = new List<float[]>();
            TrackingTargetAct = new List<float[]>();
            TrackingBaseExp = new List<float[]>();
            TrackingBaseAct = new List<float[]>();
            TrackingPhaseExp = new List<float[]>();
            TrackingPhaseAct = new List<float[]>();
            TCIExp = new List<float[]>();
            TCIAct = new List<float[]>();
        }
    }
}
