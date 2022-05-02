using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class TankProgram : MyGridProgram
    {
        private readonly List<IMyMotorSuspension> motorSuspensions = new List<IMyMotorSuspension>();

        private static readonly float defaultMaximalSteeringAngle = 25.0f;

        private static readonly float tankModeMaximalSteeringAngle = 45.0f;

        private bool tankModeState;

        public TankProgram()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        public bool TankModeState
        {
            get
            {
                return tankModeState;
            }
            set
            {
                if (tankModeState != value)
                {
                    tankModeState = value;
                    GridTerminalSystem.GetBlocksOfType(motorSuspensions);
                    motorSuspensions.ForEach
                    (
                        (motorSuspension) =>
                        {
                            bool is_tank_wheel = false;
                            bool is_right_tank_wheel = false;
                            switch (motorSuspension.CustomData.Trim().ToLower())
                            {
                                case "frontlefttankwheel":
                                    motorSuspension.SteeringOverride = tankModeState ? 1.0f : 0.0f;
                                    is_tank_wheel = true;
                                    break;
                                case "backlefttankwheel":
                                    motorSuspension.SteeringOverride = tankModeState ? -1.0f : 0.0f;
                                    is_tank_wheel = true;
                                    break;
                                case "frontrighttankwheel":
                                    motorSuspension.SteeringOverride = tankModeState ? 1.0f : 0.0f;
                                    is_tank_wheel = true;
                                    is_right_tank_wheel = true;
                                    break;
                                case "backrighttankwheel":
                                    motorSuspension.SteeringOverride = tankModeState ? -1.0f : 0.0f;
                                    is_tank_wheel = true;
                                    is_right_tank_wheel = true;
                                    break;
                            }
                            if (is_tank_wheel)
                            {
                                motorSuspension.MaxSteerAngle = tankModeState ? tankModeMaximalSteeringAngle : defaultMaximalSteeringAngle;
                                if (is_right_tank_wheel)
                                {
                                    motorSuspension.InvertPropulsion = tankModeState;
                                    motorSuspension.InvertSteer = tankModeState;
                                }
                            }
                        }
                    );
                }
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.Trim().ToLower())
            {
                case "enable":
                    TankModeState = true;
                    break;
                case "disable":
                    TankModeState = false;
                    break;
                case "toggle":
                    TankModeState = !tankModeState;
                    break;
            }
        }
    }
}
