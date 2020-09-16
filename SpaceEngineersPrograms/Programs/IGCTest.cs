using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class IGCTest : MyGridProgram
    {
        private class TestClass
        {
            public string Message { get; private set; }

            public TestClass(string message)
            {
                Message = ((message == null) ? string.Empty : message);
            }
        }

        private struct TestStruct
        {
            public string Message { get; private set; }

            public TestStruct(string message)
            {
                Message = ((message == null) ? string.Empty : message);
            }
        }

        private static readonly string defaultChannel = "default";

        private IMyBroadcastListener listener;

        private string channel = defaultChannel;

        private uint count;

        private List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        private string Channel
        {
            get
            {
                string channel = Me.CustomData.Trim();
                if (channel.Length <= 0)
                {
                    channel = defaultChannel;
                }
                return channel;
            }
            set
            {
                if ((value != null) && (value != Channel))
                {
                    channel = value;
                    if (channel.Trim().Length <= 0)
                    {
                        channel = defaultChannel;
                    }
                    Print("IGC listens to channel \"" + channel + "\" now.");
                }
            }
        }

        private IMyBroadcastListener Listener
        {
            get
            {
                string channel = Channel;
                if (listener == null)
                {
                    listener = IGC.RegisterBroadcastListener(channel);
                }
                else if (this.channel != channel)
                {
                    IGC.DisableBroadcastListener(listener);
                    listener = IGC.RegisterBroadcastListener(channel);
                }
                Channel = channel;
                return listener;
            }
        }

        public IGCTest()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private void Print(string text, string tag = "")
        {
            if ((text != null) && (tag != null))
            {
                GridTerminalSystem.GetBlocks(blocks);
                foreach (IMyTerminalBlock block in blocks)
                {
                    if ((block is IMyTextPanel) && (block.CustomData.Trim() == tag))
                    {
                        IMyTextPanel text_panel = (IMyTextPanel)block;
                        text_panel.WriteText(text, true);
                        text_panel.WriteText("\n", true);
                    }
                }
            }
        }

        public void Main()
        {
            while (Listener.HasPendingMessage)
            {
                MyIGCMessage message = Listener.AcceptMessage();
                if (message.Data is string)
                {
                    Print("Recieved message: " + document.OuterXml);
                }
                else
                {
                    Print("Message is not " + typeof(string).FullName + ", but is " + message.Data.GetType().FullName);
                }
            }
            if (count >= 100U)
            {
                IGC.SendBroadcastMessage(Channel, document.OuterXml, TransmissionDistance.AntennaRelay);
                count = 0U;
            }
            ++count;
        }
    }
}
