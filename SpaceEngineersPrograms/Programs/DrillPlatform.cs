using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class DrillPlatform : MyGridProgram
    {
        private enum EDockState
        {
            Nothing,

            Dock,

            Undock
        }

        private static readonly UpdateType updateUpdateType = (UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100);

        private static readonly float pistonSpeed = 3.0f;

        private List<IMyShipConnector> shipConnectors = new List<IMyShipConnector>();

        private List<IMyPistonBase> pistons = new List<IMyPistonBase>();

        private EDockState dockState = EDockState.Nothing;

        public DrillPlatform()
        {
            // ...
        }

        private void UpdateComponents()
        {
            string tag = Me.CustomData.Trim();
            shipConnectors.Clear();
            pistons.Clear();
            GridTerminalSystem.GetBlocksOfType(shipConnectors, (ship_connector) => (ship_connector.CustomData.Trim() == tag));
            GridTerminalSystem.GetBlocksOfType(pistons, (piston) => (piston.CustomData.Trim() == tag));
        }

        private bool MovePistons(float velocity)
        {
            bool ret = true;
            float initial_velocity_per_piston = ((pistons.Count > 0) ? (velocity / pistons.Count) : 0.0f);
            foreach (IMyPistonBase piston in pistons)
            {
                piston.Velocity = initial_velocity_per_piston;
                if (((velocity < 0.0f) && (piston.Status != PistonStatus.Retracted)) || ((velocity > 0.0f) && (piston.Status != PistonStatus.Extended)))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private bool ArePistonsFinished(bool extend)
        {
            bool ret = true;
            foreach (IMyPistonBase piston in pistons)
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
            bool is_connected = true;
            switch (argument.Trim().ToLower())
            {
                case "dock":
                    if (dockState == EDockState.Nothing)
                    {
                        UpdateComponents();
                    }
                    dockState = EDockState.Dock;
                    break;
                case "undock":
                    if (dockState == EDockState.Nothing)
                    {
                        UpdateComponents();
                    }
                    dockState = EDockState.Undock;
                    break;
                default:
                    if ((updateSource & updateUpdateType) == 0)
                    {
                        dockState = EDockState.Nothing;
                    }
                    break;
            }
            switch (dockState)
            {
                case EDockState.Nothing:
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    break;
                case EDockState.Dock:
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    foreach (IMyShipConnector ship_connector in shipConnectors)
                    {
                        if (ship_connector.Status == MyShipConnectorStatus.Connectable)
                        {
                            ship_connector.Connect();
                        }
                        if (ship_connector.Status != MyShipConnectorStatus.Connected)
                        {
                            is_connected = false;
                        }
                    }
                    MovePistons(-pistonSpeed);
                    if (ArePistonsFinished(false) && (!is_connected))
                    {
                        dockState = EDockState.Undock;
                    }
                    break;
                case EDockState.Undock:
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    foreach (IMyShipConnector ship_connnector in shipConnectors)
                    {
                        if (ship_connnector.Status == MyShipConnectorStatus.Connected)
                        {
                            ship_connnector.Disconnect();
                        }
                    }
                    MovePistons(pistonSpeed);
                    break;
            }
            Echo("Number of ship connectors: " + shipConnectors.Count);
        }
    }
}
