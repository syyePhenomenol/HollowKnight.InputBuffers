using HutongGames.PlayMaker.Actions;
using Modding;
using System.Collections.Generic;
using Vasi;

namespace InputBuffers
{
    public enum BufferedAction
    {
        NONE,
        JUMP,
        DASH,
        ATTACK,
        CAST_WAIT,
        CAST
    }

    public class InputBuffers : Mod, IMenuMod, IGlobalSettings<Settings>
    {
        public static InputBuffers Instance;
        public override string GetVersion() => "1.0.0";

        public static Settings GS = new();
        public void OnLoadGlobal(Settings gs) => GS = gs;
        public Settings OnSaveGlobal() => GS;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new ()
            {
                new()
                {
                    Name = "Buffer Jumps",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferJump = !GS.BufferJump;
                    },
                    Loader = () => GS.BufferJump ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Dash",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferDash = !GS.BufferDash;
                    },
                    Loader = () => GS.BufferDash ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Attacks",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferAttack = !GS.BufferAttack;
                    },
                    Loader = () => GS.BufferAttack ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Spell Casts",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferSpellCast = !GS.BufferSpellCast;
                    },
                    Loader = () => GS.BufferSpellCast ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Duration",
                    Description = "Affects the above buffers only.",
                    Values = new string[]
                    {
                        "50 ms (2.5 frames)",
                        "100 ms (5 frames)",
                        "200 ms (10 frames)",
                        "300 ms (15 frames)",
                        "400 ms (20 frames)",
                        "500 ms (25 frames)",
                        "750 ms (37.5 frames)",
                        "1000 ms (50 frames)",
                    },
                    Saver = opt =>
                    {
                        GS.BufferDuration = opt;
                    },
                    Loader = () => GS.BufferDuration
                },

                new()
                {
                    Name = "Buffer Focus",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferFocus = !GS.BufferFocus;
                        SetFocusBuffer();
                    },
                    Loader = () => GS.BufferFocus ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Dream Nail",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferDreamNail = !GS.BufferDreamNail;
                        SetDreamNailBuffer();
                    },
                    Loader = () => GS.BufferDreamNail ? 1 : 0
                },

                new()
                {
                    Name = "Buffer Superdash",
                    Description = "",
                    Values = new string[] { "Off", "On" },
                    Saver = opt =>
                    {
                        GS.BufferSuperdash = !GS.BufferSuperdash;
                        SetSuperdashBuffer();
                    },
                    Loader = () => GS.BufferSuperdash ? 1 : 0
                },
            };
        }

        public bool ToggleButtonInsideMenu => false;

        public static BufferedAction bufferedAction = BufferedAction.NONE;

        public override void Initialize()
        {
            Instance = this;

            On.HeroController.Start += HeroController_Start;
            On.HeroController.LookForQueueInput += HeroController_LookForQueueInput;
            On.HeroController.FixedUpdate += HeroController_FixedUpdate;

            On.HeroController.DoWallJump += HeroController_DoWallJump;
            On.HeroController.HeroJump += HeroController_HeroJump;
            On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
            On.HeroController.HeroDash += HeroController_HeroDash;
            On.HeroController.DoAttack += HeroController_DoAttack;
        }

        private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            if (self.GetComponent<BufferInputListener>() == null)
            {
                Log("Adding BufferInputListener");
                self.gameObject.AddComponent<BufferInputListener>();
            }

            SetFocusBuffer();
            SetDreamNailBuffer();
            SetSuperdashBuffer();
        }

        private void HeroController_LookForQueueInput(On.HeroController.orig_LookForQueueInput orig, HeroController self)
        {
            if (self.acceptingInput && !GameManager.instance.isPaused && GameManager.instance.IsGameplayScene())
            {
                if (bufferedAction == BufferedAction.JUMP)
                {
                    if (InputHandler.Instance.inputActions.jump.IsPressed)
                    {
                        if ((bool)Reflection.CanWallJump.Invoke(self, null))
                        {
                            Log("Buffered wall jump performed");
                            Reflection.DoWallJump.Invoke(self, null);
                        }
                        else if ((bool)Reflection.CanJump.Invoke(self, null))
                        {
                            Log("Buffered jump performed");
                            Reflection.HeroJump.Invoke(self, null);
                        }
                        else if ((bool)Reflection.CanDoubleJump.Invoke(self, null))
                        {
                            Log("Buffered double jump performed");
                            Reflection.DoDoubleJump.Invoke(self, null);
                        }

                        return;
                    }
                    else
                    {
                        Log("Buffered jump cancelled");
                        bufferedAction = BufferedAction.NONE;
                    }
                }

                if (bufferedAction == BufferedAction.DASH
                    && (bool)Reflection.CanDash.Invoke(self, null))
                {
                    Log("Buffered dash performed");
                    Reflection.HeroDash.Invoke(self, null);
                }

                if (bufferedAction == BufferedAction.ATTACK
                    && (bool)Reflection.CanAttack.Invoke(self, null))
                {
                    Log("Buffered attack performed");
                    Reflection.DoAttack.Invoke(self, null);
                }
            }

            orig(self);
        }

        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);

            if (self.acceptingInput && !GameManager.instance.isPaused && GameManager.instance.IsGameplayScene())
            {
                if (bufferedAction == BufferedAction.CAST_WAIT && HeroController.instance.CanCast())
                {
                    if (!InputHandler.Instance.inputActions.cast.IsPressed)
                    {
                        if (self.move_input > 0f && !self.cState.facingRight)
                        {
                            self.FlipSprite();
                        }
                        else if (self.move_input < 0f && self.cState.facingRight)
                        {
                            self.FlipSprite();
                        }

                        bufferedAction = BufferedAction.CAST;
                    }
                    else
                    {
                        Instance.Log("Buffered spell cast cancelled");
                        bufferedAction = BufferedAction.NONE;
                    }

                }

                if (bufferedAction == BufferedAction.CAST && HeroController.instance.CanCast())
                {
                    if (!InputHandler.Instance.inputActions.cast.IsPressed)
                    {
                        Instance.Log("Buffered spell cast performed");
                        HeroController.instance.spellControl.SendEvent("QUICK CAST");
                    }
                    else
                    {
                        Instance.Log("Buffered spell cast cancelled");
                    }

                    bufferedAction = BufferedAction.NONE;
                }
            }
        }

        private void HeroController_DoWallJump(On.HeroController.orig_DoWallJump orig, HeroController self)
        {
            orig(self);

            Log("Buffer cleared on wall jump");
            bufferedAction = BufferedAction.NONE;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            orig(self);

            Log("Buffer cleared on jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);

            Log("Buffer cleared on double jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);

            Log("Buffer cleared on dash");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoAttack(On.HeroController.orig_DoAttack orig, HeroController self)
        {
            orig(self);

            Log("Buffer cleared on attack");
            bufferedAction = BufferedAction.NONE;
        }

        private void SetFocusBuffer()
        {
            var fsm = HeroController.instance?.spellControl;

            if (fsm == null) return;

            if (GS.BufferFocus)
            {
                fsm.GetAction<ListenForCast>("Inactive", 1).isPressed = fsm.GetAction<ListenForCast>("Inactive", 1).wasPressed;
            }
            else
            {
                fsm.GetAction<ListenForCast>("Inactive", 1).isPressed = fsm.GetAction<ListenForCast>("Inactive", 1).wasReleased;
            }
        }

        private void SetDreamNailBuffer()
        {
            var fsm = HeroController.instance?.gameObject?.LocateMyFSM("Dream Nail");

            if (fsm == null) return;

            if (GS.BufferDreamNail)
            {
                fsm.GetAction<ListenForDreamNail>("Inactive", 0).isPressed = fsm.GetAction<ListenForDreamNail>("Inactive", 0).wasPressed;
            }
            else
            {
                fsm.GetAction<ListenForDreamNail>("Inactive", 0).isPressed = fsm.GetAction<ListenForDreamNail>("Inactive", 0).wasReleased;
            }
        }

        private void SetSuperdashBuffer()
        {
            var fsm = HeroController.instance?.superDash;

            if (fsm == null) return;

            if (GS.BufferSuperdash)
            {
                fsm.GetAction<ListenForSuperdash>("Inactive", 0).isPressed = fsm.GetAction<ListenForSuperdash>("Inactive", 0).wasPressed;
            }
            else
            {
                fsm.GetAction<ListenForSuperdash>("Inactive", 0).isPressed = fsm.GetAction<ListenForSuperdash>("Inactive", 0).wasReleased;
            }
        }
    }
}