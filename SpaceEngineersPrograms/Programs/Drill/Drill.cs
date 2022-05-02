using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class Drill : MyGridProgram
    {
        private enum EPistonGroupType
        {
            Straight,

            Sideways,

            Vertical
        }

        private enum EDrillState
        {
            Nothing,

            InitializeVertical,

            InitializeSideways,

            InitializeStraight,

            OperationStraight,

            OperationSideways,

            OperationVertical,

            FinishVertical,

            FinishSideways,

            FinishStraight
        }

        private static readonly float guideSpeed = 3.0f;

        private static readonly float operationalSpeed = 0.325f;

        private static readonly double adjustmentTime = 10.0;

        private static readonly EPistonGroupType[] pistonGroupTypes = (EPistonGroupType[])(Enum.GetValues(typeof(EPistonGroupType)));

        private List<IMyPistonBase>[] pistonGroups = new List<IMyPistonBase>[pistonGroupTypes.Length];

        private List<IMyShipDrill> drills = new List<IMyShipDrill>();

        private EDrillState drillState = EDrillState.Nothing;

        private double elapsedTime;

        private bool isStraightExtending = true;

        private bool isSidewaysExtending = true;

        public Drill()
        {
            string storage = Storage;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            if (storage != null)
            {
                byte[] data = Convert.FromBase64String(storage);
                if (data.Length == (sizeof(byte) + sizeof(double) + sizeof(byte)))
                {
                    try
                    {
                        EDrillState drill_state = (EDrillState)(data[0]);
                        double elapsed_time = BitConverter.ToDouble(data, 1);
                        bool is_straight_extending = (data[9] & 0x1) == 0x1;
                        bool is_sideways_extending = (data[9] & 0x2) == 0x2;
                        drillState = drill_state;
                        elapsedTime = elapsed_time;
                        isStraightExtending = is_straight_extending;
                        isSidewaysExtending = is_sideways_extending;
                    }
                    catch (Exception e)
                    {
                        Echo(e.ToString());
                    }
                }
                Storage = string.Empty;
            }
            Refresh();
        }

        private void Refresh()
        {
            List<IMyPistonBase> all_pistons = new List<IMyPistonBase>();
            string tag = Me.CustomData.Trim();
            drills.Clear();
            GridTerminalSystem.GetBlocksOfType(all_pistons, (piston) => piston.IsFunctional);
            GridTerminalSystem.GetBlocksOfType(drills, (drill) => (drill.IsFunctional && (drill.CustomData.Trim() == tag)));
            for (int i = 0; i < pistonGroups.Length; i++)
            {
                if (pistonGroups[i] == null)
                {
                    pistonGroups[i] = new List<IMyPistonBase>();
                }
                else
                {
                    pistonGroups[i].Clear();
                }
            }
            foreach (IMyPistonBase piston in all_pistons)
            {
                string[] split_piston_custom_data = piston.CustomData.Split(':');
                if (split_piston_custom_data.Length >= 2)
                {
                    string piston_tag = split_piston_custom_data[0].Trim();
                    string piston_group_type = split_piston_custom_data[1].Trim();
                    if (piston_tag == tag)
                    {
                        foreach (EPistonGroupType piston_orientation in pistonGroupTypes)
                        {
                            if (piston_group_type == piston_orientation.ToString())
                            {
                                pistonGroups[(int)piston_orientation].Add(piston);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool MovePistonGroup(EPistonGroupType pistonGroupType, float velocity)
        {
            bool ret = true;
            List<IMyPistonBase> piston_group = pistonGroups[(int)pistonGroupType];
            float initial_velocity_per_piston = ((piston_group.Count > 0) ? (velocity / piston_group.Count) : 0.0f);
            foreach (IMyPistonBase piston in piston_group)
            {
                piston.Velocity = initial_velocity_per_piston;
                if (((velocity < 0.0f) && (piston.Status != PistonStatus.Retracted)) || ((velocity > 0.0f) && (piston.Status != PistonStatus.Extended)))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private void StopPistonGroup(EPistonGroupType pistonGroupType)
        {
            foreach (IMyPistonBase piston in pistonGroups[(int)pistonGroupType])
            {
                if ((piston.Status == PistonStatus.Extending) || (piston.Status == PistonStatus.Retracting))
                {
                    piston.Velocity = 0.0f;
                }
            }
        }

        private void SetDrillsState(bool state)
        {
            foreach (IMyShipDrill drill in drills)
            {
                drill.Enabled = state;
            }
        }

        private bool IsPistonGroupFinished(EPistonGroupType pistonGroupType, bool extend)
        {
            bool ret = true;
            foreach (IMyPistonBase piston in pistonGroups[(int)pistonGroupType])
            {
                if (piston.Status != (extend ? PistonStatus.Extended : PistonStatus.Retracted))
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.Trim().ToLower())
            {
                case "refresh":
                    Refresh();
                    break;
                case "start":
                case "restart":
                    drillState = EDrillState.InitializeVertical;
                    break;
                case "stop":
                    drillState = EDrillState.FinishVertical;
                    break;
            }
            switch (drillState)
            {
                case EDrillState.InitializeVertical:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Vertical, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.InitializeSideways;
                    }
                    break;
                case EDrillState.InitializeSideways:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Sideways, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.InitializeStraight;
                    }
                    break;
                case EDrillState.InitializeStraight:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Straight, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.OperationStraight;
                    }
                    break;
                case EDrillState.OperationStraight:
                    SetDrillsState(true);
                    if (MovePistonGroup(EPistonGroupType.Straight, isStraightExtending ? Math.Abs(operationalSpeed) : -Math.Abs(operationalSpeed)))
                    {
                        drillState = (IsPistonGroupFinished(EPistonGroupType.Vertical, true) ? EDrillState.FinishVertical : (IsPistonGroupFinished(EPistonGroupType.Sideways, isSidewaysExtending) ? EDrillState.OperationVertical : EDrillState.OperationSideways));
                        elapsedTime = 0.0;
                    }
                    break;
                case EDrillState.OperationSideways:
                    elapsedTime += Runtime.TimeSinceLastRun.TotalSeconds;
                    SetDrillsState(true);
                    if (MovePistonGroup(EPistonGroupType.Sideways, isSidewaysExtending ? Math.Abs(operationalSpeed) : -Math.Abs(operationalSpeed)) || (elapsedTime >= adjustmentTime))
                    {
                        StopPistonGroup(EPistonGroupType.Sideways);
                        drillState = EDrillState.OperationStraight;
                        isStraightExtending = !isStraightExtending;
                        elapsedTime = 0.0;
                    }
                    break;
                case EDrillState.OperationVertical:
                    elapsedTime += Runtime.TimeSinceLastRun.TotalSeconds;
                    SetDrillsState(true);
                    if (MovePistonGroup(EPistonGroupType.Vertical, Math.Abs(operationalSpeed)) || (elapsedTime >= adjustmentTime))
                    {
                        StopPistonGroup(EPistonGroupType.Vertical);
                        drillState = EDrillState.OperationStraight;
                        isSidewaysExtending = !isSidewaysExtending;
                        isStraightExtending = !isStraightExtending;
                        elapsedTime = 0.0;
                    }
                    break;
                case EDrillState.FinishVertical:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Vertical, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.FinishSideways;
                    }
                    break;
                case EDrillState.FinishSideways:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Sideways, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.FinishStraight;
                    }
                    break;
                case EDrillState.FinishStraight:
                    SetDrillsState(false);
                    if (MovePistonGroup(EPistonGroupType.Straight, -Math.Abs(guideSpeed)))
                    {
                        drillState = EDrillState.Nothing;
                        isStraightExtending = true;
                        isSidewaysExtending = true;
                    }
                    break;
            }
        }

        public void Save()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)drillState);
            data.AddRange(BitConverter.GetBytes(elapsedTime));
            data.Add((byte)((isStraightExtending ? 0x1 : 0x0) | (isSidewaysExtending ? 0x2 : 0x0)));
            Storage = Convert.ToBase64String(data.ToArray());
            data.Clear();
        }
    }
}
