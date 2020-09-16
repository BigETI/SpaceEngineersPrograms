using Sandbox.ModAPI.Ingame;
using System;

namespace SpaceEngineersPrograms
{
    public class DisplayGroup : AUIControl
    {
        public DisplayGroup(IMyTextSurfaceProvider textSurfaceProvider) : base(null)
        {
            if (textSurfaceProvider == null)
            {
                throw new ArgumentNullException(nameof(textSurfaceProvider));
            }
            for (int i = 0; i < textSurfaceProvider.SurfaceCount; i++)
            {
                new Display(textSurfaceProvider.GetSurface(i), this);
            }
        }
    }
}
