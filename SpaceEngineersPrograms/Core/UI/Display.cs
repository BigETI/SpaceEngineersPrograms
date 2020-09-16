using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class Display : AUIControl
    {
        private string[] fonts;

        private string[] sprites;

        private IMyTextSurface textSurface;

        public override TextAlignment Alignment
        {
            get
            {
                return ((textSurface == null) ? base.Alignment : textSurface.Alignment);
            }
            set
            {
                base.Alignment = value;
                if (textSurface != null)
                {
                    textSurface.Alignment = value;
                }
            }
        }

        public override byte BackgroundAlpha
        {
            get
            {
                return ((textSurface == null) ? base.BackgroundAlpha : textSurface.BackgroundAlpha);
            }
            set
            {
                base.BackgroundAlpha = value;
                if (textSurface != null)
                {
                    textSurface.BackgroundAlpha = value;
                }
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return ((textSurface == null) ? base.BackgroundColor : textSurface.ScriptBackgroundColor);
            }
            set
            {
                base.BackgroundColor = value;
                if (textSurface != null)
                {
                    textSurface.ScriptBackgroundColor = value;
                }
            }
        }

        public override string Font
        {
            get
            {
                return ((textSurface == null) ? base.Font : textSurface.Font);
            }
            set
            {
                base.Font = value;
                if (textSurface != null)
                {
                    textSurface.Font = value;
                }
            }
        }

        public override IReadOnlyList<string> Fonts
        {
            get
            {
                IReadOnlyList<string> ret = fonts;
                if (fonts == null)
                {
                    if (textSurface == null)
                    {
                        ret = Parent.Fonts;
                    }
                    else
                    {
                        List<string> font_list = new List<string>();
                        textSurface.GetFonts(font_list);
                        fonts = font_list.ToArray();
                        font_list.Clear();
                        ret = fonts;
                    }
                }
                return ret;
            }
        }

        public override Color ForegroundColor
        {
            get
            {
                return ((textSurface == null) ? base.ForegroundColor : textSurface.ScriptForegroundColor);
            }
            set
            {
                base.ForegroundColor = value;
                if (textSurface != null)
                {
                    textSurface.ScriptForegroundColor = value;
                }
            }
        }

        public override float FontSize
        {
            get
            {
                return ((textSurface == null) ? base.FontSize : textSurface.FontSize);
            }
            set
            {
                base.FontSize = value;
                if (textSurface != null)
                {
                    textSurface.FontSize = value;
                }
            }
        }

        public override Vector2 Position
        {
            get
            {
                return Vector2.Zero;
            }
            set
            {
                // ...
            }
        }

        public override Vector2 Size
        {
            get
            {
                return ((textSurface == null) ? base.Size : textSurface.SurfaceSize);
            }
            set
            {
                // ...
            }
        }

        public override string Text
        {
            get
            {
                return ((textSurface == null) ? base.Text : textSurface.GetText());
            }
            set
            {
                base.Text = value;
                if (textSurface != null)
                {
                    textSurface.WriteText(value, false);
                }
            }
        }

        public override IReadOnlyList<string> Sprites
        {
            get
            {
                IReadOnlyList<string> ret = sprites;
                if (sprites == null)
                {
                    if (textSurface == null)
                    {
                        ret = Parent.Fonts;
                    }
                    else
                    {
                        List<string> sprite_list = new List<string>();
                        textSurface.GetSprites(sprite_list);
                        sprites = sprite_list.ToArray();
                        sprite_list.Clear();
                        ret = sprites;
                    }
                }
                return ret;
            }
        }

        public Display(IMyTextSurface textSurface, DisplayGroup parent) : base(parent)
        {
            if (textSurface == null)
            {
                throw new ArgumentNullException(nameof(textSurface));
            }
            this.textSurface = textSurface;
        }

        public override void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            if (textSurface == null)
            {
                base.Refresh(spriteDrawFrame);
            }
            else
            {
                textSurface.ContentType = ContentType.SCRIPT;
                using (MySpriteDrawFrame sprite_draw_frame = textSurface.DrawFrame())
                {
                    base.Refresh(sprite_draw_frame);
                }
            }
        }

        public override void ResetBackgroundAlpha()
        {
            base.ResetBackgroundAlpha();
            if (textSurface != null)
            {
                textSurface.BackgroundAlpha = base.BackgroundAlpha;
            }
        }

        public override void ResetBackgroundColor()
        {
            base.ResetBackgroundColor();
            if (textSurface != null)
            {
                textSurface.BackgroundColor = base.BackgroundColor;
            }
        }

        public override void ResetFontSize()
        {
            base.ResetFontSize();
            if (textSurface != null)
            {
                textSurface.FontSize = base.FontSize;
            }
        }

        public override void ResetForegroundColor()
        {
            base.ResetForegroundColor();
            if (textSurface != null)
            {
                textSurface.FontColor = base.ForegroundColor;
            }
        }
    }
}
