using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SpaceEngineersPrograms.Programs
{
    public class HangarManagerProgram : MyGridProgram
    {
        private delegate void ForeignGridConnectedDelegate(long entityID);

        private delegate void ForeignGridDisconnectedDelegate(long entityID);

        private delegate void RegisteredShipConnectedDelegate(ShipIGC shipIGC);

        private delegate void RegisteredShipDisconnectedDelegate(ShipIGC shipIGC);

        public class ShipIGC
        {
            private readonly IMyIntergridCommunicationSystem igc;

            private string name = string.Empty;

            private List<string> unreachedMessages = new List<string>();

            public string Channel { get; }

            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                    name = value;
                }
            }

            public long ShipID { get; }

            public Vector3 Size { get; set; } = Vector3.PositiveInfinity;

            public ShipIGC(long shipID, IMyIntergridCommunicationSystem igc)
            {
                if (igc == null)
                {
                    throw new ArgumentNullException(nameof(igc));
                }
                this.igc = igc;
                ShipID = shipID;
            }

            private bool AttemptSending(string message) => ((Channel == null) ? false : igc.SendUnicastMessage(ShipID, "ship", message));

            public bool Send(string message)
            {
                bool ret = false;
                if (message != null)
                {
                    ret = AttemptSending(message);
                }
                return ret;
            }

            public void Update()
            {
                while (unreachedMessages.Count > 0)
                {
                    if (AttemptSending(unreachedMessages[0]))
                    {
                        unreachedMessages.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private class ShipDockingStation
        {
            private long lastConnectedEntityID;

            private MyShipConnectorStatus lastShipConnectorStatus;

            private ShipIGC registeredShipIGC;

            private IMyShipConnector shipConnector;

            public event ForeignGridConnectedDelegate OnForeignGridConnected;

            public event ForeignGridDisconnectedDelegate OnForeignGridDisconnected;

            public event RegisteredShipDisconnectedDelegate OnRegisteredShipDisconnected;

            public event RegisteredShipConnectedDelegate OnRegisteredShipConnected;

            public MyShipConnectorStatus ShipConnectionStatus
            {
                get
                {
                    return shipConnector.Status;
                }
            }

            public ShipDockingStation(IMyShipConnector shipConnector)
            {
                if (shipConnector == null)
                {
                    throw new ArgumentNullException(nameof(shipConnector));
                }
                this.shipConnector = shipConnector;
                lastShipConnectorStatus = ShipConnectionStatus;
            }

            public bool RegisterShip(ShipIGC shipIGC)
            {
                if (shipIGC == null)
                {
                    throw new ArgumentNullException(nameof(shipIGC));
                }
                bool ret = false;
                if (registeredShipIGC == null)
                {
                    registeredShipIGC = shipIGC;
                    ret = true;
                }
                return ret;
            }

            public void Update()
            {
                MyShipConnectorStatus ship_connector_status = shipConnector.Status;
                if (lastShipConnectorStatus != ship_connector_status)
                {
                    switch (ship_connector_status)
                    {
                        case MyShipConnectorStatus.Unconnected:
                        case MyShipConnectorStatus.Connectable:
                            if (lastShipConnectorStatus == MyShipConnectorStatus.Connected)
                            {
                                if (registeredShipIGC == null)
                                {
                                    OnForeignGridDisconnected?.Invoke(lastConnectedEntityID);
                                }
                                else if (lastConnectedEntityID == registeredShipIGC.ShipID)
                                {
                                    OnRegisteredShipDisconnected?.Invoke(registeredShipIGC);
                                }
                                else
                                {
                                    OnForeignGridDisconnected?.Invoke(lastConnectedEntityID);
                                }
                            }
                            break;
                        case MyShipConnectorStatus.Connected:
                            lastConnectedEntityID = shipConnector.OtherConnector.CubeGrid.EntityId;
                            if (registeredShipIGC == null)
                            {
                                shipConnector.Disconnect();
                                OnForeignGridConnected?.Invoke(lastConnectedEntityID);
                            }
                            else if (shipConnector.OtherConnector.CubeGrid.EntityId == registeredShipIGC.ShipID)
                            {
                                OnRegisteredShipConnected?.Invoke(registeredShipIGC);
                            }
                            else
                            {
                                shipConnector.Disconnect();
                                OnForeignGridConnected?.Invoke(lastConnectedEntityID);
                            }
                            break;
                    }
                    lastShipConnectorStatus = ship_connector_status;
                }
            }

            public bool UnregisterShip()
            {
                bool ret = (registeredShipIGC != null);
                registeredShipIGC = null;
                return ret;
            }
        }

        private IMyBroadcastListener hangarBroadcastListener;

        private Dictionary<long, ShipIGC> shipIGCs = new Dictionary<long, ShipIGC>();

        private Commands<bool, ShipIGC> shipCommands = new Commands<bool, ShipIGC>(' ');

        private List<ShipDockingStation> dockingStations = new List<ShipDockingStation>();

        private Dictionary<long, ShipDockingStation> shipDockingStationRelationship = new Dictionary<long, ShipDockingStation>();

        public HangarManagerProgram()
        {
            hangarBroadcastListener = IGC.RegisterBroadcastListener("hangar.server");
            shipCommands.Add("discover", DiscoverHangarCommand, "name");
            shipCommands.Add("register", RegisterShipCommand, "name");
            shipCommands.Add("dock", DockShipCommand, "width", "height", "length");
            shipCommands.Add("undock", UndockShipCommand, "width", "height", "length");
        }

        public static bool DiscoverHangarCommand(IReadOnlyList<string> arguments, ShipIGC shipIGC)
        {
            bool ret = false;
            if (!(shipIGC.IsRegistered))
            {
                shipIGC.Send("identify");
            }
            return ret;
        }

        public static bool RegisterShipCommand(IReadOnlyList<string> arguments, ShipIGC shipIGC)
        {
            bool ret = !(shipIGC.IsRegistered);
            shipIGC.IsRegistered = true;
            shipIGC.Name = arguments[0];
            return ret;
        }

        public static bool DockShipCommand(IReadOnlyList<string> arguments, ShipIGC shipIGC)
        {
            bool ret = false;
            if (shipIGC.IsRegistered)
            {

            }
            else
            {
                shipIGC.Send("identify");
            }
            return ret;
        }

        public static bool UndockShipCommand(IReadOnlyList<string> arguments, ShipIGC shipIGC)
        {
            bool ret = false;

            return ret;
        }

        public void Main(string argument, UpdateType updateType)
        {
            while (hangarBroadcastListener.HasPendingMessage)
            {
                MyIGCMessage igc_message = hangarBroadcastListener.AcceptMessage();
                ShipIGC ship_igc;
                if (shipIGCs.ContainsKey(igc_message.Source))
                {
                    ship_igc = shipIGCs[igc_message.Source];
                }
                else
                {
                    ship_igc = new ShipIGC(igc_message.Source, IGC);
                    shipIGCs.Add(igc_message.Source, ship_igc);
                }
                try
                {
                    string igc_message_data = igc_message.As<string>();
                    if (igc_message_data != null)
                    {
                        shipCommands.Parse(igc_message_data, ship_igc);
                    }
                }
                catch (Exception e)
                {
                    Echo(e.ToString());
                }
            }
        }
    }
}
