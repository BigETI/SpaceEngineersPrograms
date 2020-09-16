using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class PulsingLights : MyGridProgram
    {
        private List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        private static readonly Color beginColor = new Color(1.0f, 0.0f, 0.0f);

        private static readonly float beginIntensity = 5.0f;

        private static readonly Color endColor = new Color(0.0f, 1.0f, 0.0f);

        private static readonly float endIntensity = 5.0f;

        private static readonly double animationTime = 1.0;

        private double elapsedTime;

        private bool direction;

        public PulsingLights()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main()
        {
            elapsedTime += Runtime.TimeSinceLastRun.TotalSeconds;
            while (elapsedTime >= animationTime)
            {
                elapsedTime -= animationTime;
                direction = !direction;
            }
            float time = (float)((direction ? (animationTime - elapsedTime) : elapsedTime) / animationTime);
            float intensity = MathHelper.Lerp(beginIntensity, endIntensity, time);
            Color color = Color.Lerp(beginColor, endColor, time);
            GridTerminalSystem.GetBlocks(blocks);
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyLightingBlock)
                {
                    IMyLightingBlock lighting_block = (IMyLightingBlock)block;
                    lighting_block.Intensity = intensity;
                    lighting_block.Color = color;
                }
                else if (block is IMyReflectorLight)
                {
                    IMyReflectorLight reflector_light = (IMyReflectorLight)block;
                    reflector_light.Intensity = intensity;
                    reflector_light.Color = color;
                }
            }
        }
    }
}
