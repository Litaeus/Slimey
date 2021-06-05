using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SlimeyMod.SkillStates.BaseStates
{
    public class BaseMeleeAttack : BaseSkillState
    {
        public int swingIndex;

        protected string hitboxName = "Sword";

        protected DamageType damageType = DamageType.Generic;
        protected float damageCoefficient = 3.5f;
        protected float procCoefficient = 1f;
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseDuration = 1f;
        protected float attackStartTime = 0.2f;
        protected float attackEndTime = 0.4f;
        protected float baseEarlyExitTime = 0.4f;
        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;
        protected bool cancelled = false;

        protected string swingSoundString = "";
        protected string hitSoundString = "";
        protected string muzzleString = "SwingCenter";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        private float earlyExitTime;
        public float duration;
        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        protected bool inHitPause;
        private bool hasHopped;
        protected float stopwatch;
        protected Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            earlyExitTime = baseEarlyExitTime / attackSpeedStat;
            hasFired = false;
            animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + duration, false);
            base.characterBody.outOfCombatStopwatch = 0f;
            animator.SetBool("attacking", true);

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxName);
            }

            PlayAttackAnimation();

            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = damageCoefficient * damageStat;
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = impactSound;
        }

        protected virtual void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "Slash" + (1 + swingIndex), "Slash.playbackRate", duration, 0.05f);
        }

        public override void OnExit()
        {
            if (!hasFired && !cancelled) FireAttack();

            base.OnExit();

            animator.SetBool("attacking", false);
        }

        protected virtual void PlaySwingEffect()
        {
            EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, muzzleString, true);
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(hitSoundString, base.gameObject);

            if (!hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    base.SmallHop(base.characterMotor, hitHopVelocity);
                }

                hasHopped = true;
            }

            if (!inHitPause && hitStopDuration > 0f)
            {
                storedVelocity = base.characterMotor.velocity;
                hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, animator, "Slash.playbackRate");
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }

        private void FireAttack()
        {
            if (!hasFired)
            {
                hasFired = true;
                Util.PlayAttackSpeedSound(swingSoundString, base.gameObject, attackSpeedStat);

                if (base.isAuthority)
                {
                    PlaySwingEffect();
                    base.AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
                }
            }

            if (base.isAuthority)
            {
                if (attack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void SetNextState()
        {
            int index = swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            outer.SetNextState(new BaseMeleeAttack
            {
                swingIndex = index
            });
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            hitPauseTimer -= Time.fixedDeltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                base.ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
                inHitPause = false;
                base.characterMotor.velocity = storedVelocity;
            }

            if (!inHitPause)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (animator) animator.SetFloat("Swing.playbackRate", 0f);
            }

            if (stopwatch >= (duration * attackStartTime) && stopwatch <= (duration * attackEndTime))
            {
                FireAttack();
            }

            if (stopwatch >= (duration - earlyExitTime) && base.isAuthority)
            {
                if (base.inputBank.skill1.down)
                {
                    if (!hasFired) FireAttack();
                    SetNextState();
                    return;
                }
            }

            if (stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingIndex = reader.ReadInt32();
        }
    }
}