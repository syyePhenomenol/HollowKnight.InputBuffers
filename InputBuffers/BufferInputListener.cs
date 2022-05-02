using System.Collections;
using UnityEngine;
using Vasi;

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

            if (InputHandler.Instance.inputActions.jump.WasPressed && InputBuffers.GS.BufferJump)
            {
                bool buffer = false;

                if (HeroController.instance.controlReqlinquished)
                {
                    InputBuffers.Instance.Log("Jump buffered while control relinquished");
                    buffer = true;
                }

                if (HeroController.instance.cState.falling
                    && !HeroController.instance.cState.wallSliding
                    && !(bool) Reflection.CanDoubleJump.Invoke(HeroController.instance, null))
                {
                    InputBuffers.Instance.Log("Jump buffered while falling without double jump");
                    buffer = true;
                }

                if (HeroController.instance.cState.dashing)
                {
                    InputBuffers.Instance.Log("Jump buffered while dashing");
                    buffer = true;
                }

                if (HeroController.instance.cState.recoilFrozen
                    || HeroController.instance.cState.recoiling)
                {
                    InputBuffers.Instance.Log("Jump buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    InputBuffers.bufferedAction = BufferedAction.JUMP;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (InputHandler.Instance.inputActions.dash.WasPressed && InputBuffers.GS.BufferDash)
            {
                bool buffer = false;

                if (HeroController.instance.controlReqlinquished)
                {
                    InputBuffers.Instance.Log("Dash buffered while control relinquished");
                    buffer = true;
                }

                if ((bool) Reflection.airDashed.GetValue(HeroController.instance))
                {
                    InputBuffers.Instance.Log("Dash buffered after dash used in midair");
                    buffer = true;
                }

                if ((float) Reflection.dashCooldownTimer.GetValue(HeroController.instance) > 0f)
                {
                    InputBuffers.Instance.Log("Dash buffered during dash cooldown");
                    buffer = true;
                }

                if (HeroController.instance.cState.recoilFrozen
                    || HeroController.instance.cState.recoiling)
                {
                    InputBuffers.Instance.Log("Dash buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    InputBuffers.bufferedAction = BufferedAction.DASH;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if (InputHandler.Instance.inputActions.attack.WasPressed && InputBuffers.GS.BufferAttack)
            {
                bool buffer = false;

                if (HeroController.instance.controlReqlinquished)
                {
                    InputBuffers.Instance.Log("Attack buffered while control relinquished");
                    buffer = true;
                }

                if ((float) Reflection.attack_cooldown.GetValue(HeroController.instance) > 0f)
                {
                    InputBuffers.Instance.Log("Attack buffered during attack cooldown");
                    buffer = true;
                }

                if (HeroController.instance.cState.dashing)
                {
                    InputBuffers.Instance.Log("Attack buffered while dashing");
                    buffer = true;
                }

                if (HeroController.instance.cState.recoilFrozen
                    || HeroController.instance.cState.recoiling)
                {
                    InputBuffers.Instance.Log("Attack buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    InputBuffers.bufferedAction = BufferedAction.ATTACK;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }

            if ((InputHandler.Instance.inputActions.cast.WasPressed
                    || InputHandler.Instance.inputActions.quickCast.WasPressed)
                && HasEnoughMP()
                && InputBuffers.GS.BufferSpellCast)
            {
                bool buffer = false;

                if (HeroController.instance.controlReqlinquished
                    && HeroController.instance.spellControl.ActiveStateName != "Fireball Antic"
                    && HeroController.instance.spellControl.ActiveStateName != "Scream Antic1"
                    && HeroController.instance.spellControl.ActiveStateName != "Scream Antic2"
                    && HeroController.instance.spellControl.ActiveStateName != "Quake Antic")
                {
                    InputBuffers.Instance.Log("Cast buffered while control relinquished");
                    buffer = true;
                }

                if (HeroController.instance.cState.dashing)
                {
                    InputBuffers.Instance.Log("Cast buffered while dashing");
                    InputBuffers.bufferedAction = BufferedAction.CAST_WAIT;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }

                if (HeroController.instance.cState.recoilFrozen
                    || HeroController.instance.cState.recoiling)
                {
                    InputBuffers.Instance.Log("Cast buffered while recoiling");
                    buffer = true;
                }

                if (buffer)
                {
                    InputBuffers.bufferedAction = BufferedAction.CAST;
                    StopAllCoroutines();
                    StartCoroutine("WaitForBufferClear");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        private IEnumerator WaitForBufferClear()
        {
            float time = InputBuffers.GS.BufferDuration switch
            {
                0 => 0.050f,
                1 => 0.100f,
                2 => 0.200f,
                3 => 0.300f,
                4 => 0.400f,
                5 => 0.500f,
                6 => 0.750f,
                7 => 1.000f,
            };

            yield return new WaitForSeconds(time);

            InputBuffers.bufferedAction = BufferedAction.NONE;
        }

        private static bool HasEnoughMP()
        {
            return PlayerData.instance.GetInt("MPCharge") >= HeroController.instance.spellControl.GetOrCreateInt("MP Cost").Value;
        }
    }
}
