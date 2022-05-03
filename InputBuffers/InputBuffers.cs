using HutongGames.PlayMaker;
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
        QUICK_CAST,
        CAST,
        DREAM_NAIL,
        SUPERDASH
    }

    public class InputBuffers : Mod, IMenuMod, IGlobalSettings<Settings>
    {
        public static InputBuffers Instance;
        public override string GetVersion() => "1.0.1";

        public static Settings GS = new();
        public void OnLoadGlobal(Settings gs) => GS = gs;
        public Settings OnSaveGlobal() => GS;

        public bool ToggleButtonInsideMenu => false;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return MenuMod.Menu;
        }

        public static BufferedAction bufferedAction = BufferedAction.NONE;

        public override void Initialize()
        {
            Instance = this;

            On.HeroController.Start += HeroController_Start;
            On.HeroController.FixedUpdate += HeroController_FixedUpdate;

            On.HeroController.DoWallJump += HeroController_DoWallJump;
            On.HeroController.HeroJump += HeroController_HeroJump;
            On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
            On.HeroController.HeroDash += HeroController_HeroDash;
            On.HeroController.DoAttack += HeroController_DoAttack;

            On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        }

        private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            if (self.GetComponent<BufferInputListener>() == null)
            {
                //Log("Adding BufferInputListener");
                self.gameObject.AddComponent<BufferInputListener>();
            }
        }

        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            if (GameManager.instance.isPaused || !GameManager.instance.IsGameplayScene())
            {
                orig(self);
                return;
            }

            if (self.acceptingInput)
            {
                if (bufferedAction == BufferedAction.JUMP && !HeroController.instance.cState.dashing)
                {
                    if (InputHandler.Instance.inputActions.jump.IsPressed)
                    {
                        if ((bool)Reflection.CanWallJump.Invoke(self, null))
                        {
                            //Log("Buffered wall jump performed");
                            Reflection.DoWallJump.Invoke(self, null);
                            Reflection.doubleJumpQueuing.SetValue(self, false);
                            return;
                        }
                        else if ((bool)Reflection.CanJump.Invoke(self, null))
                        {
                            //Log("Buffered jump performed");
                            Reflection.HeroJump.Invoke(self, null);
                            return;
                        }
                        else if ((bool)Reflection.CanDoubleJump.Invoke(self, null))
                        {
                            //Log("Buffered double jump performed");
                            Reflection.DoDoubleJump.Invoke(self, null);
                            return;
                        }
                    }
                    else
                    {
                        //Log("Buffered jump cancelled");
                        bufferedAction = BufferedAction.NONE;
                    }
                }

                if (bufferedAction == BufferedAction.DASH
                    && (bool)Reflection.CanDash.Invoke(self, null))
                {
                    //Log("Buffered dash performed");
                    Reflection.HeroDash.Invoke(self, null);
                    return;
                }

                if (bufferedAction == BufferedAction.ATTACK
                    && (bool)Reflection.CanAttack.Invoke(self, null))
                {
                    if (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered attack not performed yet");
                    }
                    else
                    {
                        //Log("Buffered attack performed");
                        Reflection.DoAttack.Invoke(self, null);
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.CAST && HeroController.instance.spellControl.ActiveStateName != "Button Down")
                {
                    if (HeroController.instance.CanCast()
                        && !InputHandler.Instance.inputActions.cast.IsPressed
                        && GS.BufferQuickCast)
                    {
                        if (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                        {
                            //Instance.Log("Knight not facing correctly, buffered quick cast not performed yet");
                        }
                        else
                        {
                            //Instance.Log("Buffered cast performed as spell");
                            HeroController.instance.spellControl.SendEvent("BUTTON DOWN");
                            return;
                        }
                    }
                    else if (HeroController.instance.CanFocus()
                        && InputHandler.Instance.inputActions.cast.IsPressed
                        && GS.BufferCast)
                    {
                        //Instance.Log("Buffered cast performed as focus");
                        HeroController.instance.spellControl.SendEvent("BUTTON DOWN");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.QUICK_CAST && HeroController.instance.CanCast())
                {
                    if (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered quick cast not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered quick cast performed");
                        HeroController.instance.spellControl.SendEvent("QUICK CAST");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.DREAM_NAIL
                    && HeroController.instance.CanDreamNail()
                    && InputHandler.Instance.inputActions.dreamNail.IsPressed)
                {
                    if (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered dream nail not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered dream nail performed");
                        HeroController.instance.gameObject.LocateMyFSM("Dream Nail").SendEvent("BUTTON DOWN");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.SUPERDASH
                    && HeroController.instance.CanSuperDash()
                    && InputHandler.Instance.inputActions.superDash.IsPressed)
                {
                    if (HeroController.instance.hero_state != GlobalEnums.ActorStates.airborne
                        && (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight)))
                    {
                        //Instance.Log("Knight not facing correctly, buffered superdash not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered superdash performed");
                        HeroController.instance.superDash.SendEvent("BUTTON DOWN");
                        return;
                    }
                }
            }

            if (GS.SuperdashRelease
                && ((HeroController.instance.superDash.ActiveStateName == "Ground Charged"
                        && (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && !self.cState.facingRight)
                            || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && self.cState.facingRight)))
                    || (HeroController.instance.superDash.ActiveStateName == "Wall Charged"
                        && (((InputHandler.Instance.inputActions.left.IsPressed || InputHandler.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((InputHandler.Instance.inputActions.right.IsPressed || InputHandler.Instance.inputActions.right.WasPressed) && !self.cState.facingRight)))))
            {
                //Instance.Log("Superdash released");
                HeroController.instance.superDash.SendEvent("BUTTON UP");
                return;
            }

            orig(self);
        }

        private void HeroController_DoWallJump(On.HeroController.orig_DoWallJump orig, HeroController self)
        {
            orig(self);

            //Log("Buffer cleared on wall jump");
            bufferedAction = BufferedAction.NONE;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            orig(self);

            //Log("Buffer cleared on jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);

            //Log("Buffer cleared on double jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);

            //Log("Buffer cleared on dash");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoAttack(On.HeroController.orig_DoAttack orig, HeroController self)
        {
            orig(self);

            //Log("Buffer cleared on attack");
            bufferedAction = BufferedAction.NONE;
        }

        private static readonly Dictionary<string, int> SpellFSMStates = new()
        {
            { "Fireball Antic", 7 },
            { "Quake Antic", 13 },
            { "Scream Antic1", 7 },
            { "Scream Antic2", 7 },
            { "Button Down", 4 },
        };

        private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.FsmName == "Spell Control")
            {
                foreach (KeyValuePair<string, int> state in SpellFSMStates)
                {
                    if (self.GetState(state.Key).Actions.Length == state.Value)
                    {
                        self.GetState(state.Key).AddAction(new ClearBuffer());
                    }
                }
            }
            else if (self.FsmName == "Dream Nail")
            {
                if (self.GetState("Start").Actions.Length == 4)
                {
                    self.GetState("Start").AddAction(new ClearBuffer());
                }
            }
            else if (self.FsmName == "Superdash")
            {
                if (self.GetState("Ground Charge").Actions.Length == 22)
                {
                    self.GetState("Ground Charge").AddAction(new ClearBuffer());
                }

                if (self.GetState("Wall Charge").Actions.Length == 22)
                {
                    self.GetState("Wall Charge").AddAction(new ClearBuffer());
                }
            }
        }

        private class ClearBuffer : FsmStateAction
        {
            public override void OnEnter()
            {
                //Instance.Log("Buffer cleared on spell/focus/dream nail/superdash");
                bufferedAction = BufferedAction.NONE;

                base.OnEnter();
            }
        }
    }
}