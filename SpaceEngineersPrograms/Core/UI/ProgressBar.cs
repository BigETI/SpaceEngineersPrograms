using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class ProgressBar : Border
    {
        private static readonly string backgroundSpriteName = "SquareSimple";

        private static readonly string progressSpriteName = "SquareSimple";

        private MySprite backgroundSprite;

        private MySprite progressSprite;

        private float value;

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }
            set
            {
                base.BackgroundColor = value;
                backgroundSprite.Color = value;
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
                progressSprite.Color = value;
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
                backgroundSprite.Position = value;
                UpdateVisuals();
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
                backgroundSprite.Size = value;
                UpdateVisuals();
            }
        }

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = Math.Min(Math.Max(value, 0.0f), 1.0f);
                UpdateVisuals();
            }
        }

        public ProgressBar(Vector2 position, Vector2 size, Color backgroundColor, Color progressColor, Color borderColor, float borderSize, float value, IUIControl parent) : base(position, size, borderColor, borderSize, parent)
        {
            backgroundSprite = MySprite.CreateSprite(backgroundSpriteName, position, size);
            progressSprite = MySprite.CreateSprite(progressSpriteName, position, Vector2.Zero);
            BackgroundColor = backgroundColor;
            ForegroundColor = progressColor;
            Value = value;
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            spriteDrawFrame?.Add(backgroundSprite);
            spriteDrawFrame?.Add(progressSprite);
            base.Refresh(spriteDrawFrame);
        }

        public override void ResetBackgroundColor()
        {
            backgroundSprite.Color = null;
            base.ResetBackgroundColor();
        }

        public override void ResetForegroundColor()
        {
            progressSprite.Color = null;
            base.ResetForegroundColor();
        }

        public override void ResetSize()
        {
            backgroundSprite.Size = null;
            progressSprite.Size = new Vector2(Size.X * Value, Size.Y);
            UpdateVisuals();
            base.ResetSize();
        }

        private void UpdateVisuals()
        {
            Vector2 progress_size = new Vector2((Size.X - (BorderSize * 2)) * value, Size.Y);
            backgroundSprite.Position = Position;
            backgroundSprite.Size = Size;
            progressSprite.Size = progress_size;
            progressSprite.Position = new Vector2(Position.X - ((Size.X - progress_size.X) * 0.5f) + BorderSize, Position.Y);
        }
    }
}
