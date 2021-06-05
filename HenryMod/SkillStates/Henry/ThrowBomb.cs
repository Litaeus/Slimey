using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace SlimeyMod.SkillStates
{
    public class ThrowBomb : BaseSkillState
    {
        public static float damageCoefficient = 16f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.65f;
        public static float throwForce = 80f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = ThrowBomb.baseDuration / attackSpeedStat;
            fireTime = 0.35f * duration;
            base.characterBody.SetAimTimer(2f);
            animator = base.GetModelAnimator();

            base.PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;
                Util.PlaySound("HenryBombThrow", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();

                    ProjectileManager.instance.FireProjectile(HenryModules.Projectiles.bombPrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject,
                        ThrowBomb.damageCoefficient * damageStat,
                        4000f,
                        base.RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        ThrowBomb.throwForce);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= fireTime)
            {
                Fire();
            }

            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}