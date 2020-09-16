using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class Label : AUIControl
    {
        private MySprite labelSprite;

        public override TextAlignment Alignment
        {
            get
            {
                return base.Alignment;
            }
            set
            {
                base.Alignment = value;
                labelSprite.Alignment = value;
            }
        }

        public override string Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                labelSprite.FontId = value;
            }
        }

        public override float FontSize
        {
            get
            {
                return base.FontSize;
            }
            set
            {
                base.FontSize = value;
                labelSprite.RotationOrScale = value;
            }
        }

        public override Color ForegroundColor
        {
            get
            {
                return base.ForegroundColor;
            }
            set
            {
                base.ForegroundColor = value;
                labelSprite.Color = value;
            }
        }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                labelSprite.Position = value;
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                labelSprite.Data = value;
            }
        }

        public Label(string text, string font, Color fontColor, float fontSize, TextAlignment alignment, Vector2 position, IUIControl parent) : base(parent)
        {
            labelSprite = MySprite.CreateText(text, font, fontColor, fontSize, alignment);
            base.Text = text;
            base.Font = font;
            base.ForegroundColor = fontColor;
            base.FontSize = fontSize;
            Alignment = alignment;
            Position = position;
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            spriteDrawFrame?.Add(labelSprite);
            base.Refresh(spriteDrawFrame);
        }

        public override void ResetForegroundColor()
        {
            labelSprite.Color = null;
            base.ResetForegroundColor();
        }

        public override void ResetPosition()
        {
            labelSprite.Position = null;
            base.ResetPosition();
        }
    }
}
