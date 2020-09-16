using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class Panel : Border
    {
        private static readonly string panelBackgroundSpriteName = "SquareSimple";

        private MySprite backgroundSprite;

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
            }
        }

        public override float SpriteRotation
        {
            get
            {
                return base.SpriteRotation;
            }
            set
            {
                base.SpriteRotation = value;
                backgroundSprite.RotationOrScale = value;
            }
        }

        public Panel(Vector2 position, Vector2 size, Color borderColor, float borderSize, IUIControl parent) : base(position, size, borderColor, borderSize, parent)
        {
            backgroundSprite = MySprite.CreateSprite(panelBackgroundSpriteName, position, size);
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            spriteDrawFrame?.Add(backgroundSprite);
            base.Refresh(spriteDrawFrame);
        }
    }
}
