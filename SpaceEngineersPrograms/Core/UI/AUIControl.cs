using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public abstract class AUIControl : IUIControl
    {
        private byte? backgroundAlpha;

        private Color? backgroundColor;

        private Color? borderColor;

        private float? borderSize;

        private List<IUIControl> controls = new List<IUIControl>();

        private string font;

        private float? fontSize;

        private Color? foregroundColor;

        private IUIControl parent;

        private string text = string.Empty;

        public virtual TextAlignment Alignment { get; set; }

        public virtual byte BackgroundAlpha
        {
            get
            {
                return ((backgroundAlpha == null) ? ((Parent == null) ? (byte)0xFF : Parent.BackgroundAlpha) : backgroundAlpha.Value);
            }
            set
            {
                backgroundAlpha = value;
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                return ((backgroundColor == null) ? ((Parent == null) ? Color.Black : Parent.BackgroundColor) : backgroundColor.Value);
            }
            set
            {
                backgroundColor = value;
            }
        }

        public virtual Color BorderColor
        {
            get
            {
                return ((borderColor == null) ? Color.White : borderColor.Value);
            }
            set
            {
                borderColor = value;
            }
        }

        public virtual float BorderSize
        {
            get
            {
                return ((borderSize == null) ? 0.0f : borderSize.Value);
            }
            set
            {
                borderSize = value;
            }
        }

        public virtual IReadOnlyList<IUIControl> Controls => controls;

        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                text = value;
            }
        }

        public virtual string Font
        {
            get
            {
                return (font == null) ? ((Parent == null) ? "DEBUG" : Parent.Font) : font;
            }
            set
            {
                font = value;
            }
        }

        public virtual IReadOnlyList<string> Fonts => ((Parent == null) ? Array.Empty<string>() : Parent.Fonts);

        public virtual float FontSize
        {
            get
            {
                return (fontSize == null) ? ((Parent == null) ? 1.0f : Parent.FontSize) : fontSize.Value;
            }
            set
            {
                fontSize = value;
            }
        }

        public virtual Color ForegroundColor
        {
            get
            {
                return ((foregroundColor == null) ? ((Parent == null) ? Color.Black : Parent.ForegroundColor) : foregroundColor.Value);
            }
            set
            {
                foregroundColor = value;
            }
        }

        public virtual IUIControl Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != value)
                {
                    if (parent is AUIControl)
                    {
                        ((AUIControl)parent).controls.Remove(this);
                    }
                    parent = value;
                    if (parent is AUIControl)
                    {
                        List<IUIControl> ui_controls = ((AUIControl)parent).controls;
                        bool add_ui_control = true;
                        foreach (IUIControl ui_control in ui_controls)
                        {
                            if (ui_control == this)
                            {
                                add_ui_control = false;
                                break;
                            }
                        }
                        if (add_ui_control)
                        {
                            ui_controls.Add(this);
                        }
                    }
                }
            }
        }

        public virtual Vector2 Position { get; set; }

        public virtual Vector2 Size { get; set; }

        public virtual float SpriteRotation { get; set; }

        public virtual SpriteType SpriteType { get; set; }

        public virtual IReadOnlyList<string> Sprites => ((Parent == null) ? Array.Empty<string>() : Parent.Sprites);

        public AUIControl(IUIControl parent)
        {
            Parent = parent;
        }

        public virtual void Refresh(MySpriteDrawFrame? spriteDrawFrame)
        {
            foreach (IUIControl control in controls)
            {
                control?.Refresh(spriteDrawFrame);
            }
        }

        public virtual void ResetBackgroundAlpha()
        {
            backgroundAlpha = null;
        }

        public virtual void ResetBackgroundColor()
        {
            backgroundColor = null;
        }

        public virtual void ResetBorderColor()
        {
            borderColor = null;
        }

        public virtual void ResetBorderSize()
        {
            borderSize = null;
        }

        public virtual void ResetFontSize()
        {
            fontSize = null;
        }

        public virtual void ResetForegroundColor()
        {
            foregroundColor = null;
        }

        public virtual void ResetPosition()
        {
            Position = Vector2.Zero;
        }

        public virtual void ResetSize()
        {
            Size = Vector2.Zero;
        }
    }
}
