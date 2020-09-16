using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public interface IUIControl
    {
        TextAlignment Alignment { get; set; }

        byte BackgroundAlpha { get; set; }

        Color BackgroundColor { get; set; }

        Color BorderColor { get; set; }

        float BorderSize { get; set; }

        string Text { get; set; }

        string Font { get; set; }

        IReadOnlyList<string> Fonts { get; }

        float FontSize { get; set; }

        Color ForegroundColor { get; set; }

        IReadOnlyList<IUIControl> Controls { get; }

        IUIControl Parent { get; set; }

        Vector2 Position { get; set; }

        Vector2 Size { get; set; }

        float SpriteRotation { get; set; }

        SpriteType SpriteType { get; set; }

        IReadOnlyList<string> Sprites { get; }

        void Refresh(MySpriteDrawFrame? spriteDrawFrame);

        void ResetBackgroundAlpha();

        void ResetBackgroundColor();

        void ResetBorderColor();

        void ResetBorderSize();

        void ResetFontSize();

        void ResetForegroundColor();

        void ResetPosition();

        void ResetSize();
    }
}
