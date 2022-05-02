using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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

    public class InputBuffers : Mod, IGlobalSettings<Settings>
    {
        public static InputBuffers Instance;
        public override string GetVersion() => "1.0.0";

        public static Settings GS = new();
        public void OnLoadGlobal(Settings gs) => GS = gs;
        public Settings OnSaveGlobal() => GS;

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

            SetFocusBuffer(GS.BufferFocus);
            SetDreamNailBuffer(GS.BufferDreamNail);
            SetSuperdashBuffer(GS.BufferSuperdash);
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

        private void SetFocusBuffer(bool value)
        {
            var fsm = HeroController.instance.spellControl;

            if (value)
            {
                fsm.GetAction<ListenForCast>("Inactive", 1).isPressed = fsm.GetAction<ListenForCast>("Inactive", 1).wasPressed;
            }
            else
            {
                fsm.GetAction<ListenForCast>("Inactive", 1).isPressed = fsm.GetAction<ListenForCast>("Inactive", 1).wasReleased;
            }
        }

        private void SetDreamNailBuffer(bool value)
        {
            var fsm = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");

            if (value)
            {
                fsm.GetAction<ListenForDreamNail>("Inactive", 0).isPressed = fsm.GetAction<ListenForDreamNail>("Inactive", 0).wasPressed;
            }
            else
            {
                fsm.GetAction<ListenForDreamNail>("Inactive", 0).isPressed = fsm.GetAction<ListenForDreamNail>("Inactive", 0).wasReleased;
            }
        }

        private void SetSuperdashBuffer(bool value)
        {
            var fsm = HeroController.instance.superDash;

            if (value)
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