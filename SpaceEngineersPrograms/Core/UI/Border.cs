using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class Border : AUIControl
    {
        private static readonly string borderSideSpriteName = "SquareSimple";

        private MySprite topBorderSprite;

        private MySprite bottomBorderSprite;

        private MySprite leftBorderSprite;

        private MySprite rightBorderSprite;

        public Border(Vector2 position, Vector2 size, Color borderColor, float borderSize, IUIControl parent) : base(parent)
        {
            topBorderSprite = MySprite.CreateSprite(borderSideSpriteName, position, Vector2.Zero);
            bottomBorderSprite = MySprite.CreateSprite(borderSideSpriteName, position, Vector2.Zero);
            leftBorderSprite = MySprite.CreateSprite(borderSideSpriteName, position, Vector2.Zero);
            rightBorderSprite = MySprite.CreateSprite(borderSideSpriteName, position, Vector2.Zero);
            base.Position = position;
            base.Size = size;
            BorderColor = borderColor;
            base.BorderSize = borderSize;
            UpdateVisuals();
        }

        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
                topBorderSprite.Color = value;
                bottomBorderSprite.Color = value;
                leftBorderSprite.Color = value;
                rightBorderSprite.Color = value;
            }
        }

        public override float BorderSize
        {
            get
            {
                return base.BorderSize;
            }
            set
            {
                base.BorderSize = value;
                UpdateVisuals();
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
                UpdateVisuals();
            }
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            spriteDrawFrame?.Add(topBorderSprite);
            spriteDrawFrame?.Add(bottomBorderSprite);
            spriteDrawFrame?.Add(leftBorderSprite);
            spriteDrawFrame?.Add(rightBorderSprite);
            base.Refresh(spriteDrawFrame);
        }

        public override void ResetBorderColor()
        {
            topBorderSprite.Color = null;
            bottomBorderSprite.Color = null;
            leftBorderSprite.Color = null;
            rightBorderSprite.Color = null;
            base.ResetBorderColor();
        }

        public override void ResetBorderSize()
        {

            base.ResetBorderSize();
        }

        private void UpdateVisuals()
        {
            Vector2 horizontal_border_size = new Vector2(Size.X - (BorderSize * 2.0f), BorderSize);
            Vector2 vertical_border_size = new Vector2(BorderSize, Size.Y);
            topBorderSprite.Size = horizontal_border_size;
            topBorderSprite.Position = new Vector2(Position.X, Position.Y - ((Size.Y - BorderSize) * 0.5f));
            bottomBorderSprite.Size = horizontal_border_size;
            bottomBorderSprite.Position = new Vector2(Position.X, Position.Y + ((Size.Y - BorderSize) * 0.5f));
            leftBorderSprite.Size = vertical_border_size;
            leftBorderSprite.Position = new Vector2(Position.X - ((Size.X - BorderSize) * 0.5f), Position.Y);
            rightBorderSprite.Size = vertical_border_size;
            rightBorderSprite.Position = new Vector2(Position.X + ((Size.X - BorderSize) * 0.5f), Position.Y);
        }
    }
}
