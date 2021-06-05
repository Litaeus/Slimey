using RoR2;
using SlimeyMod.SkillStates.BaseStates;
using UnityEngine;

namespace SlimeyMod.SkillStates
{
    public class SlashCombo : BaseMeleeAttack
    {
        public override void OnEnter()
        {
            hitboxName = "Sword";

            damageType = DamageType.Generic;
            damageCoefficient = HenryModules.StaticValues.swordDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;
            attackStartTime = 0.2f;
            attackEndTime = 0.4f;
            baseEarlyExitTime = 0.4f;
            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;

            swingSoundString = "HenrySwordSwing";
            hitSoundString = "";
            muzzleString = swingIndex % 2 == 0 ? "SwingLeft" : "SwingRight";
            swingEffectPrefab = HenryModules.Assets.swordSwingEffect;
            hitEffectPrefab = HenryModules.Assets.swordHitImpactEffect;

            impactSound = HenryModules.Assets.swordHitSoundEvent.index;

            base.OnEnter();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayAttackAnimation();
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void SetNextState()
        {
            int index = swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            outer.SetNextState(new SlashCombo
            {
                swingIndex = index
            });
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}