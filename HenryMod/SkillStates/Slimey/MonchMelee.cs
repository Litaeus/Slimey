using EntityStates;
using RoR2;
using UnityEngine;

namespace SlimeyMod.SkillStates.Slimey
{
    public class MonchMelee : BaseSkillState
    {
        public static float damageCoefficient = 2.0f; // 200% damage
        public static float procCoefficient = 1.0f; // 100% proc multiplier
        public static float baseDuration = 0.25f; // Animation can happen 4 times/second
        public static float force = 1000f; // Twice the SlimeShot force

        private float fireTime;
        private float duration;
        private bool hasFired;


        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            base.characterBody.SetAimTimer(2f);
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

                if (base.isAuthority)
                {
                    Ray lookRay = new Ray(base.characterBody.corePosition, base.characterBody.transform.forward);
                    base.AddRecoil(-0.2f * Shoot.recoil, -0.2f * Shoot.recoil, -0.1f * Shoot.recoil, 0.1f * Shoot.recoil); // A bit of recoil to mimic Acrid's bite

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        attackerFiltering = AttackerFiltering.NeverHit, // Disable friendly/self-damage if Chaos is active
                        baseDamage = base.characterBody.damage * damageCoefficient,
                        baseForce = force,
                        bonusForce = new Vector3(),
                        crit = base.RollCrit(),
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BlastAttack.FalloffModel.None,
                        impactEffect = EffectIndex.Invalid, // Here's where the visual effect would play if there was one
                        losType = BlastAttack.LoSType.None,
                        position = lookRay.GetPoint(1f), // The blast will be centered 1 units down the aimRay
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.5f, // If this matches the position distance, then it can hit directly in front of you
                        teamIndex = base.GetTeam()
                    }.Fire();
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
