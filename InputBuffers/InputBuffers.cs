using HutongGames.PlayMaker;
using Modding;
using System.Collections.Generic;
using Vasi;
using BIL = InputBuffers.BufferInputListener;
using IH = InputHandler;
using HC = HeroController;
using RH = Modding.ReflectionHelper;

namespace InputBuffers
{
    public enum BufferedAction
    {
        NONE,
        JUMP,
        DASH,
        ATTACK,
        CAST,
        QUICK_CAST,
        DREAM_NAIL,
        SUPERDASH
    }

    public class InputBuffers : Mod, IMenuMod, IGlobalSettings<Settings>
    {
        public static InputBuffers Instance;
        public override string GetVersion() => "1.0.3";

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

        private void HeroController_Start(On.HeroController.orig_Start orig, HC self)
        {
            orig(self);

            if (self.GetComponent<BIL>() == null)
            {
                //Log("Adding BufferInputListener");
                self.gameObject.AddComponent<BIL>();
            }
        }

        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HC self)
        {
            if (GameManager.instance.isPaused || !GameManager.instance.IsGameplayScene())
            {
                orig(self);
                return;
            }

            // Clear buffer on hit.
            if(GS.ClearBufferOnHit && HC.instance.cState.recoilFrozen && bufferedAction != BufferedAction.JUMP)
            {
                bufferedAction = BufferedAction.NONE;
            }

            if (self.acceptingInput)
            {
                if (bufferedAction == BufferedAction.JUMP && !HC.instance.cState.dashing)
                {
                    if (IH.Instance.inputActions.jump.IsPressed)
                    {
                        if (RH.CallMethod<HC, bool>(self, "CanWallJump"))
                        {
                            //Log("Buffered wall jump performed");
                            RH.CallMethod(self, "DoWallJump");
                            RH.SetField(self, "doubleJumpQueuing", false);
                            return;
                        }
                        else if (RH.CallMethod<HC, bool>(self, "CanJump"))
                        {
                            //Log("Buffered jump performed");
                            RH.CallMethod(self, "HeroJump");
                            return;
                        }
                        else if (RH.CallMethod<HC, bool>(self, "CanDoubleJump"))
                        {
                            //Log("Buffered double jump performed");
                            RH.CallMethod(self, "DoDoubleJump");
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
                    && RH.CallMethod<HC, bool>(self, "CanDash"))
                {
                    //Log("Buffered dash performed");
                    RH.CallMethod(self, "HeroDash");
                    return;
                }

                if (bufferedAction == BufferedAction.ATTACK
                    && RH.CallMethod<HC, bool>(self, "CanAttack"))
                {
                    if (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered attack not performed yet");
                    }
                    else
                    {
                        //Log("Buffered attack performed");
                        RH.CallMethod(self, "DoAttack");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.CAST && HC.instance.spellControl.ActiveStateName != "Button Down")
                {
                    if (HC.instance.CanCast()
                        && !IH.Instance.inputActions.cast.IsPressed
                        && GS.BufferQuickCast)
                    {
                        if (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                        {
                            //Instance.Log("Knight not facing correctly, buffered quick cast not performed yet");
                        }
                        else
                        {
                            //Instance.Log("Buffered cast performed as spell");
                            HC.instance.spellControl.SendEvent("BUTTON DOWN");
                            return;
                        }
                    }
                    else if (HC.instance.CanFocus()
                        && IH.Instance.inputActions.cast.IsPressed
                        && GS.BufferCast)
                    {
                        //Instance.Log("Buffered cast performed as focus");
                        HC.instance.spellControl.SendEvent("BUTTON DOWN");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.QUICK_CAST && HC.instance.CanCast())
                {
                    if (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered quick cast not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered quick cast performed");
                        HC.instance.spellControl.SendEvent("QUICK CAST");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.DREAM_NAIL
                    && HC.instance.CanDreamNail()
                    && IH.Instance.inputActions.dreamNail.IsPressed)
                {
                    if (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                        || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight))
                    {
                        //Instance.Log("Knight not facing correctly, buffered dream nail not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered dream nail performed");
                        HC.instance.gameObject.LocateMyFSM("Dream Nail").SendEvent("BUTTON DOWN");
                        return;
                    }
                }

                if (bufferedAction == BufferedAction.SUPERDASH
                    && HC.instance.CanSuperDash()
                    && IH.Instance.inputActions.superDash.IsPressed)
                {
                    if (HC.instance.hero_state != GlobalEnums.ActorStates.airborne
                        && (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight)))
                    {
                        //Instance.Log("Knight not facing correctly, buffered superdash not performed yet");
                    }
                    else
                    {
                        //Instance.Log("Buffered superdash performed");
                        HC.instance.superDash.SendEvent("BUTTON DOWN");
                        return;
                    }
                }
            }

            if (GS.SuperdashRelease
                && ((HC.instance.superDash.ActiveStateName == "Ground Charged"
                        && (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && !self.cState.facingRight)
                            || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && self.cState.facingRight)))
                    || (HC.instance.superDash.ActiveStateName == "Wall Charged"
                        && (((IH.Instance.inputActions.left.IsPressed || IH.Instance.inputActions.left.WasPressed) && self.cState.facingRight)
                            || ((IH.Instance.inputActions.right.IsPressed || IH.Instance.inputActions.right.WasPressed) && !self.cState.facingRight)))))
            {
                //Instance.Log("Superdash released");
                HC.instance.superDash.SendEvent("BUTTON UP");
                return;
            }

            orig(self);
        }

        private void HeroController_DoWallJump(On.HeroController.orig_DoWallJump orig, HC self)
        {
            orig(self);

            //Log("Buffer cleared on wall jump");
            bufferedAction = BufferedAction.NONE;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HC self)
        {
            orig(self);

            //Log("Buffer cleared on jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HC self)
        {
            orig(self);

            //Log("Buffer cleared on double jump");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HC self)
        {
            orig(self);

            //Log("Buffer cleared on dash");
            bufferedAction = BufferedAction.NONE;
        }

        private void HeroController_DoAttack(On.HeroController.orig_DoAttack orig, HC self)
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