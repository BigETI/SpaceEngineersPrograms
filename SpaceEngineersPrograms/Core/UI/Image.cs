using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class Image : AUIControl
    {
        private MySprite imageSprite;

        public override TextAlignment Alignment
        {
            get
            {
                return base.Alignment;
            }
            set
            {
                base.Alignment = value;
                imageSprite.Alignment = value;
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
                imageSprite.Color = value;
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
                imageSprite.Position = value;
            }
        }

        public override Vector2 Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                imageSprite.Size = value;
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
                imageSprite.Data = value;
            }
        }

        public Image(string texture, TextAlignment alignment, Vector2 position, Vector2 size, Color color, IUIControl parent) : base(parent)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            imageSprite = MySprite.CreateSprite(texture, position, size);
            base.Text = texture;
            Alignment = alignment;
            base.Position = position;
            base.Size = size;
            ForegroundColor = color;
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            spriteDrawFrame?.Add(imageSprite);
            base.Refresh(spriteDrawFrame);
        }

        public override void ResetForegroundColor()
        {
            imageSprite.Color = null;
            base.ResetForegroundColor();
        }

        public override void ResetPosition()
        {
            imageSprite.Position = null;
            base.ResetPosition();
        }

        public override void ResetSize()
        {
            imageSprite.Size = null;
            base.ResetSize();
        }
    }
}
