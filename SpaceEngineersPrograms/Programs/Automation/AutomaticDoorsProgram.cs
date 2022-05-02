using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    internal class AutomaticDoorsProgram : MyGridProgram
    {
        private class AutomaticDoor
        {
            private static readonly double slideDoorOpenTime = 5.0;

            private static readonly double advancedDoorOpenTime = 5.0;

            private static readonly double hangarDoorOpenTime = 120.0;

            private static readonly double doorBaseOpenTime = 5.0;

            private static readonly double miscellaneousDoorOpenTime = 5.0;

            private IMyDoor door;

            private string settingsInput = string.Empty;

            private bool doNotIgnore = true;

            private float? customDoorOpenTime;

            private double elapsedOpenTime;

            public DoorStatus Status => door.Status;

            public double OpenTime => ((customDoorOpenTime == null) ? ((door is IMyAirtightSlideDoor) ? slideDoorOpenTime : ((door is IMyAdvancedDoor) ? advancedDoorOpenTime : ((door is IMyAirtightHangarDoor) ? hangarDoorOpenTime : ((door is IMyAirtightDoorBase) ? doorBaseOpenTime : miscellaneousDoorOpenTime)))) : customDoorOpenTime.Value);

            public AutomaticDoor(IMyDoor door)
            {
                this.door = door;
            }

            public void Update(double seconds)
            {
                if (settingsInput != door.CustomData)
                {
                    string[] configuration_attributes = door.CustomData.Split('\n');
                    doNotIgnore = true;
                    customDoorOpenTime = null;
                    foreach (string configuration_attribute in configuration_attributes)
                    {
                        string trimmed_configuration_attribute = configuration_attribute.Trim();
                        string[] key_value_pair = trimmed_configuration_attribute.Trim().Split(':');
                        string key = ((key_value_pair.Length > 0) ? key_value_pair[0] : string.Empty);
                        string value = ((key_value_pair.Length > 1) ? ((trimmed_configuration_attribute.Length > (key.Length + 1)) ? trimmed_configuration_attribute.Substring(key.Length + 1).Trim() : string.Empty) : string.Empty);
                        switch (key.Trim())
                        {
                            case "ignore":
                                doNotIgnore = false;
                                break;
                            case "openTime":
                                float open_time;
                                if (float.TryParse(value, out open_time))
                                {
                                    customDoorOpenTime = open_time;
                                }
                                break;
                        }
                    }
                    settingsInput = door.CustomData;
                }
                if (doNotIgnore && door.IsWorking && (Status == DoorStatus.Open))
                {
                    elapsedOpenTime += seconds;
                    if (elapsedOpenTime >= OpenTime)
                    {
                        door.CloseDoor();
                        elapsedOpenTime = 0.0;
                    }
                }
                else
                {
                    elapsedOpenTime = 0.0;
                }
            }
        }

        private List<IMyDoor> doorBlocks = new List<IMyDoor>();

        private Dictionary<long, AutomaticDoor> doors = new Dictionary<long, AutomaticDoor>();

        private HashSet<long> missingDoors = new HashSet<long>();

        public AutomaticDoorsProgram()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            double seconds = Runtime.TimeSinceLastRun.TotalSeconds;
            foreach (long key in doors.Keys)
            {
                missingDoors.Add(key);
            }
            GridTerminalSystem.GetBlocksOfType(doorBlocks);
            foreach (IMyDoor door_block in doorBlocks)
            {
                if (!(doors.ContainsKey(door_block.EntityId)))
                {
                    doors.Add(door_block.EntityId, new AutomaticDoor(door_block));
                }
                missingDoors.Remove(door_block.EntityId);
            }
            foreach (long key in missingDoors)
            {
                doors.Remove(key);
            }
            missingDoors.Clear();
            foreach (AutomaticDoor door in doors.Values)
            {
                door.Update(seconds);
            }
        }
    }
}
