using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersPrograms
{
    public class SpritesPreviewers : MyGridProgram
    {
        private class SpritesPreviewer
        {
            private string[] sprites;

            private uint selectedSpriteIndex;

            public IMyTextPanel TextPanel { get; private set; }

            public IReadOnlyList<string> Sprites
            {
                get
                {
                    if (sprites == null)
                    {
                        sprites = Array.Empty<string>();
                    }
                    return sprites;
                }
            }

            public uint SelectedSpritesIndex
            {
                get
                {
                    return selectedSpriteIndex;
                }
                set
                {
                    if (Sprites.Count > 0)
                    {
                        selectedSpriteIndex = value % (uint)Sprites.Count;
                    }
                }
            }

            public string SelectedSprite
            {
                get
                {
                    string ret = null;
                    if (Sprites.Count > 0)
                    {
                        ret = Sprites[(int)SelectedSpritesIndex];
                    }
                    return ret;
                }
            }

            public float ZoomFactor { get; set; }

            public SpritesPreviewer(IMyTextPanel textPanel, IReadOnlyList<string> sprites)
            {
                TextPanel = textPanel;
                this.sprites = Array.Empty<string>();
                selectedSpriteIndex = 0U;
                ZoomFactor = 1.0f;
                if (sprites != null)
                {
                    this.sprites = new string[sprites.Count];
                    for (int index = 0; index < this.sprites.Length; index++)
                    {
                        this.sprites[index] = sprites[index];
                    }
                }
            }
        }

        private delegate void SpritesPreviewerKeyInputActionDelegate(SpritesPreviewer spritesPreviewer);

        private List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        private Dictionary<long, SpritesPreviewer> spritesPreviewers = new Dictionary<long, SpritesPreviewer>();

        private HashSet<long> missingSpritePreviewers = new HashSet<long>();

        private IReadOnlyDictionary<string, SpritesPreviewerKeyInputActionDelegate> spritesPreviewersKeyInputActions = new Dictionary<string, SpritesPreviewerKeyInputActionDelegate>
        {
            { "a", MoveLeft },
            { "left", MoveLeft },
            { "d", MoveRight },
            { "right", MoveRight },
            { "w", ZoomIn },
            { "up", ZoomIn },
            { "s", ZoomOut },
            { "down", ZoomOut }
        };

        private static void MoveLeft(SpritesPreviewer spritesPreviewer)
        {
            if (spritesPreviewer.SelectedSpritesIndex == 0U)
            {
                spritesPreviewer.SelectedSpritesIndex = ((spritesPreviewer.Sprites.Count > 0) ? (uint)(spritesPreviewer.Sprites.Count - 1) : 0U);
            }
            else
            {
                spritesPreviewer.SelectedSpritesIndex--;
            }
        }

        private static void MoveRight(SpritesPreviewer spritesPreviewer)
        {
            spritesPreviewer.SelectedSpritesIndex++;
        }

        private static void ZoomIn(SpritesPreviewer spritesPreviewer)
        {
            spritesPreviewer.ZoomFactor *= 2.0f;
        }

        private static void ZoomOut(SpritesPreviewer spritesPreviewer)
        {
            spritesPreviewer.ZoomFactor *= 0.5f;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Dictionary<string, object> inputs = Me.GetProperty("ControlModule.Inputs")?.As<Dictionary<string, object>>()?.GetValue(Me);
            if (inputs != null)
            {
                blocks.Clear();
                foreach (long key in spritesPreviewers.Keys)
                {
                    missingSpritePreviewers.Add(key);
                }
                GridTerminalSystem.GetBlocks(blocks);
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block is IMyTextPanel)
                    {
                        IMyTextPanel text_panel = (IMyTextPanel)block;
                        if (text_panel.CustomData.Trim() == "previewSprites")
                        {
                            if (!(spritesPreviewers.ContainsKey(text_panel.EntityId)))
                            {
                                List<string> sprites = new List<string>();
                                text_panel.GetSprites(sprites);
                                spritesPreviewers.Add(text_panel.EntityId, new SpritesPreviewer(text_panel, sprites));
                            }
                            missingSpritePreviewers.Remove(text_panel.EntityId);
                        }
                    }
                }
                foreach (long key in missingSpritePreviewers)
                {
                    spritesPreviewers.Remove(key);
                }
                missingSpritePreviewers.Clear();
                foreach (string input in inputs.Keys)
                {
                    if (spritesPreviewersKeyInputActions.ContainsKey(input))
                    {
                        SpritesPreviewerKeyInputActionDelegate sprites_previewers_key_input_action = spritesPreviewersKeyInputActions[input];
                        foreach (SpritesPreviewer sprite_previewer in spritesPreviewers.Values)
                        {
                            sprites_previewers_key_input_action(sprite_previewer);
                        }
                    }
                }
                foreach (SpritesPreviewer sprite_previewer in spritesPreviewers.Values)
                {
                    string selected_sprite = sprite_previewer.SelectedSprite;
                    if (selected_sprite != null)
                    {
                        sprite_previewer.TextPanel.ContentType = ContentType.SCRIPT;
                        using (MySpriteDrawFrame sprite_draw_frame = sprite_previewer.TextPanel.DrawFrame())
                        {
                            sprite_draw_frame.Add(MySprite.CreateSprite(selected_sprite, sprite_previewer.TextPanel.SurfaceSize * 0.5f, sprite_previewer.TextPanel.SurfaceSize * sprite_previewer.ZoomFactor));
                            sprite_draw_frame.Add(MySprite.CreateText(selected_sprite, "DEBUG", Color.White, 1.0f, TextAlignment.CENTER));
                        }
                    }
                }
            }
        }
    }
}
