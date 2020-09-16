using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class Elevator : MyGridProgram
    {
        private class Level
        {
            private static readonly float pistonsSpeed = 1.0f;

            private IReadOnlyCollection<IMyPistonBase> pistons;

            public string Name { get; private set; }

            public float Height { get; private set; }

            public Level(string name, IReadOnlyCollection<IMyPistonBase> pistons, float height)
            {
                Name = name;
                this.pistons = pistons;
                Height = height;
            }

            public bool Move(bool extend)
            {
                bool ret = true;
                float piston_speed = ((pistons.Count > 0) ? (pistonsSpeed / pistons.Count) : 0.0f);
                foreach (IMyPistonBase piston in pistons)
                {
                    if (extend ? (piston.Status != PistonStatus.Extended) : (piston.Status != PistonStatus.Retracted))
                    {
                        piston.Velocity = (extend ? piston_speed : (-piston_speed));
                        ret = false;
                    }
                }
                return ret;
            }

            public bool IsFinished(bool extended)
            {
                bool ret = true;
                foreach (IMyPistonBase piston in pistons)
                {
                    if ((piston.Status != PistonStatus.Stopped) && (extended ? (piston.Status != PistonStatus.Extended) : (piston.Status != PistonStatus.Retracted)))
                    {
                        ret = false;
                        break;
                    }
                }
                return ret;
            }
        }

        private enum EElevatorState
        {
            Nothing,

            Checking,

            Closing,

            Moving,

            Opening,

            Waiting,

            Finished
        }

        private static readonly UpdateType updateUpdateType = (UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100);

        private static readonly IReadOnlyDictionary<string, float> heights = new Dictionary<string, float>
        {
            { "Erdgeschoss", 2.0f },
            { "Laboratorium", 12.0f },
            { "Schlafräume", 22.0f }
        };

        private static readonly float verticalPistonSpeed = 1.0f;

        private static readonly float elevatorWaitTime = 5.0f;

        private static readonly float pistonDistanceEpsilon = 0.0625f;

        private List<IMyPistonBase> verticalPistons = new List<IMyPistonBase>();

        private Dictionary<string, Level> levels = new Dictionary<string, Level>();

        private Level targetLevel;

        private Dictionary<float, Level> elevatorQueue = new Dictionary<float, Level>();

        private double elapsedElevatorWaitTime;

        private EElevatorState elevatorState;

        public Elevator()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            UpdateLevels();
        }

        private void RequestElevator(Level level)
        {
            if ((level != null) && (level != targetLevel))
            {
                if ((elevatorState == EElevatorState.Nothing) || (elevatorState == EElevatorState.Finished))
                {
                    targetLevel = level;
                    elevatorState = EElevatorState.Checking;
                }
                else
                {
                    if (!(elevatorQueue.ContainsKey(level.Height)))
                    {
                        elevatorQueue.Add(level.Height, level);
                    }
                }
            }
        }

        private void RequestElevator(string levelName)
        {
            if (levelName != null)
            {
                string level_name = levelName.Trim();
                if (level_name.Length > 0)
                {
                    if (levels.ContainsKey(level_name))
                    {
                        RequestElevator(levels[level_name]);
                    }
                    else
                    {
                        Echo("Level \"" + levelName + "\" not found!");
                    }
                }
                else
                {
                    Echo("Level name can not be empty.");
                }
            }
            else
            {
                Echo("Level name can not be null.");
            }
        }

        private static bool OpenLevel(Level level)
        {
            return ((level == null) ? true : level.Move(true));
        }

        private void UpdateLevels()
        {
            string tag = Me.CustomData.Trim();
            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            Dictionary<string, List<IMyPistonBase>> level_pistons_lookup = new Dictionary<string, List<IMyPistonBase>>();
            verticalPistons.Clear();
            levels.Clear();
            GridTerminalSystem.GetBlocksOfType(pistons);
            foreach (IMyPistonBase piston in pistons)
            {
                string[] split_custom_data = piston.CustomData.Trim().Split(':');
                if (split_custom_data.Length >= 2)
                {
                    string piston_tag = split_custom_data[0];
                    string piston_function = split_custom_data[1];
                    if (piston_tag == tag)
                    {
                        if (piston_function == "Vertical")
                        {
                            verticalPistons.Add(piston);
                        }
                        else
                        {
                            if (heights.ContainsKey(piston_function))
                            {
                                List<IMyPistonBase> level_pistons = null;
                                if (level_pistons_lookup.ContainsKey(piston_function))
                                {
                                    level_pistons = level_pistons_lookup[piston_function];
                                }
                                else
                                {
                                    level_pistons = new List<IMyPistonBase>();
                                    level_pistons_lookup.Add(piston_function, level_pistons);
                                }
                                level_pistons.Add(piston);
                            }
                            else
                            {
                                Echo("Height for \"" + piston_function + "\" is not defined in script.");
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, List<IMyPistonBase>> level_piston in level_pistons_lookup)
            {
                levels.Add(level_piston.Key, new Level(level_piston.Key, level_piston.Value, heights[level_piston.Key]));
            }
            pistons.Clear();
            level_pistons_lookup.Clear();
        }

        private static bool ArePistonsFinished(IReadOnlyCollection<IMyPistonBase> pistons)
        {
            bool ret = true;
            foreach (IMyPistonBase piston in pistons)
            {
                if ((piston.Status != PistonStatus.Extended) && (piston.Status != PistonStatus.Retracted) && (piston.Status != PistonStatus.Stopped))
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        private bool CheckElevator()
        {
            bool ret = true;
            float target_piston_position = ((verticalPistons.Count > 0) ? (targetLevel.Height / verticalPistons.Count) : 0.0f);
            foreach (IMyPistonBase vertical_piston in verticalPistons)
            {
                if (Math.Abs(vertical_piston.CurrentPosition - target_piston_position) > pistonDistanceEpsilon)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        private void CloseElevator()
        {
            bool all_closed = true;
            foreach (Level level in levels.Values)
            {
                level.Move(false);
                if (!(level.IsFinished(false)))
                {
                    all_closed = false;
                }
            }
            if (all_closed)
            {
                elevatorState = EElevatorState.Moving;
            }
        }

        private void MoveElevator()
        {
            if (targetLevel == null)
            {
                elevatorState = EElevatorState.Finished;
            }
            else
            {
                float vertical_piston_speed = ((verticalPistons.Count > 0) ? (verticalPistonSpeed / verticalPistons.Count) : 0.0f);
                float target_piston_position = ((verticalPistons.Count > 0) ? (targetLevel.Height / verticalPistons.Count) : 0.0f);
                foreach (IMyPistonBase vertical_piston in verticalPistons)
                {
                    if (vertical_piston.CurrentPosition < target_piston_position)
                    {
                        vertical_piston.MinLimit = vertical_piston.LowestPosition;
                        vertical_piston.MaxLimit = target_piston_position;
                        vertical_piston.Velocity = vertical_piston_speed;
                    }
                    else if (vertical_piston.CurrentPosition > target_piston_position)
                    {
                        vertical_piston.MaxLimit = vertical_piston.HighestPosition;
                        vertical_piston.MinLimit = target_piston_position;
                        vertical_piston.Velocity = -vertical_piston_speed;
                    }
                }
                if (ArePistonsFinished(verticalPistons))
                {
                    elevatorState = EElevatorState.Opening;
                }
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & updateUpdateType) == 0)
            {
                UpdateLevels();
                RequestElevator(argument);
            }
            switch (elevatorState)
            {
                case EElevatorState.Nothing:
                    if (levels.Count > 0)
                    {
                        RequestElevator(levels.FirstPair().Value);
                    }
                    break;
                case EElevatorState.Checking:
                    elevatorState = (CheckElevator() ? EElevatorState.Opening : EElevatorState.Closing);
                    break;
                case EElevatorState.Closing:
                    CloseElevator();
                    break;
                case EElevatorState.Moving:
                    MoveElevator();
                    break;
                case EElevatorState.Opening:
                    if (OpenLevel(targetLevel))
                    {
                        elapsedElevatorWaitTime = 0.0;
                        elevatorState = EElevatorState.Waiting;
                    }
                    break;
                case EElevatorState.Waiting:
                    elapsedElevatorWaitTime += Runtime.TimeSinceLastRun.TotalSeconds;
                    if (elapsedElevatorWaitTime >= elevatorWaitTime)
                    {
                        elapsedElevatorWaitTime = 0.0f;
                        if (elevatorQueue.Count > 0)
                        {
                            float key = float.PositiveInfinity;
                            foreach (float height in elevatorQueue.Keys)
                            {
                                if (key > height)
                                {
                                    key = height;
                                }
                            }
                            targetLevel = elevatorQueue[key];
                            elevatorQueue.Remove(key);
                            elevatorState = EElevatorState.Checking;
                        }
                        else
                        {
                            elevatorState = EElevatorState.Finished;
                        }
                    }
                    break;
            }
            Echo("State: " + elevatorState + ((targetLevel == null) ? string.Empty : (" (" + targetLevel.Name + ")")));
        }
    }
}
