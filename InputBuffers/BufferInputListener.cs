using System.Collections;
using UnityEngine;
using Vasi;
using HC = HeroController;
using IB = InputBuffers.InputBuffers;
using IH = InputHandler;
using RH = Modding.ReflectionHelper;

namespace InputBuffers
{
    internal class BufferInputListener : MonoBehaviour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        void Update()
        {
            if (GameManager.instance.isPaused
                || !GameManager.instance.IsGameplayScene()
                || GameManager.instance.inventoryFSM.ActiveStateName != "Closed") return;

            if (IH.Instance.inputActions.jump.WasPressed && IB.GS.BufferJump)
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished
                    && HC.instance.gameObject.LocateMyFSM("Surface Water").ActiveStateName != "Frame")
                {
                    //IB.Instance.Log("Jump buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Jump buffered while hard landing");
                    buffer = true;
                }
                else if (HC.instance.cState.falling
                    && !HC.instance.cState.wallSliding
                    && !RH.CallMethod<HC, bool>(HC.instance, "CanDoubleJump"))
                {
                    //InputBuffers.Instance.Log("Jump buffered while falling without double jump");
                    buffer = true;
                }
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Jump buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Jump buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.JUMP;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.dash.WasPressed && IB.GS.BufferDash)
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Dash buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Dash buffered while hard landing");
                    buffer = true;
                }
                else if (RH.GetField<HC, bool>(HC.instance, "airDashed"))
                {
                    
                    //InputBuffers.Instance.Log("Dash buffered after dash used in midair");
                    buffer = true;
                }
                else if (RH.GetField<HC, float>(HC.instance, "dashCooldownTimer") > 0f)
                {
                    //InputBuffers.Instance.Log("Dash buffered during dash cooldown");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Dash buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.DASH;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.attack.WasPressed && IB.GS.BufferAttack)
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Attack buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Attack buffered while hard landing");
                    buffer = true;
                    
                }
                else if (RH.GetField<HC, float>(HC.instance, "attack_cooldown") > 0f)
                {
                    //InputBuffers.Instance.Log("Attack buffered during attack cooldown");
                    buffer = true;
                }
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Attack buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Attack buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.ATTACK;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.cast.WasPressed
                && HasEnoughMP()
                && IB.GS.BufferCast)
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Cast buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Cast buffered while hard landing");
                    buffer = true;
                }
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Cast buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Cast buffered while recoiling");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.airborne)
                {
                    //InputBuffers.Instance.Log("Cast buffered while airborne");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.CAST;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.quickCast.WasPressed
                && HasEnoughMP()
                && IB.GS.BufferQuickCast
                && HC.instance.spellControl.ActiveStateName != "Fireball Antic"
                && HC.instance.spellControl.ActiveStateName != "Quake Antic"
                && HC.instance.spellControl.ActiveStateName != "Scream Antic1"
                && HC.instance.spellControl.ActiveStateName != "Scream Antic2")
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Quick cast buffered while control relinquished");
                    buffer = true;
                }
                // Spells will cancel a hard land by default, so we don't buffer them in that scenario
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Quick cast buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Quick cast buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.QUICK_CAST;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.dreamNail.WasPressed
                && IB.GS.BufferDreamNail
                && HC.instance.gameObject.LocateMyFSM("Dream Nail").ActiveStateName != "Start")
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Dream nail buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Dream nail buffered while hard landing");
                    buffer = true;
                }
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Dream nail buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Dream nail buffered while recoiling");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.airborne)
                {
                    //InputBuffers.Instance.Log("Dream nail buffered while airborne");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.DREAM_NAIL;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (IH.Instance.inputActions.superDash.WasPressed
                && IB.GS.BufferSuperdash
                && HC.instance.superDash.ActiveStateName != "Ground Charge"
                && HC.instance.superDash.ActiveStateName != "Wall Charge")
            {
                bool buffer = false;

                if (HC.instance.controlReqlinquished)
                {
                    //InputBuffers.Instance.Log("Superdash buffered while control relinquished");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.hard_landing)
                {
                    //InputBuffers.Instance.Log("Superdash buffered while hard landing");
                    buffer = true;
                }
                else if (HC.instance.cState.dashing)
                {
                    //InputBuffers.Instance.Log("Superdash buffered while dashing");
                    buffer = true;
                }
                else if (HC.instance.cState.recoilFrozen
                    || HC.instance.cState.recoiling)
                {
                    //InputBuffers.Instance.Log("Superdash buffered while recoiling");
                    buffer = true;
                }
                else if (HC.instance.hero_state == GlobalEnums.ActorStates.airborne)
                {
                    //InputBuffers.Instance.Log("Superdash buffered while airborne");
                    buffer = true;
                }

                if (buffer)
                {
                    IB.bufferedAction = BufferedAction.SUPERDASH;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }
        }
        private static bool HasEnoughMP()
        {
            return PlayerData.instance.GetInt("MPCharge") >= HC.instance.spellControl.GetOrCreateInt("MP Cost").Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        private IEnumerator WaitForBufferClear()
        {
            float time = IB.GS.BufferDuration switch
            {
                0 => 0.050f,
                1 => 0.100f,
                2 => 0.200f,
                3 => 0.300f,
                4 => 0.400f,
                5 => 0.500f,
                6 => 0.750f,
                7 => 1.000f,
                _ => 0.300f,
            };

            yield return new WaitForSeconds(time);

            IB.bufferedAction = BufferedAction.NONE;
        }
    }
}
