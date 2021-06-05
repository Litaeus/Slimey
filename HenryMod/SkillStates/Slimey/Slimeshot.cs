using EntityStates;
using HenryMod.Modules;
using RoR2;
using UnityEngine;

namespace HenryMod.SkillStates.Slimey
{
    public class Slimeshot : BaseSkillState
    {
        public static float damageCoefficient = 0.75f; // Literal translation of 75% damage
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.125f; // Commando can do 6*100% = 600%/s with his primary, 8*75% = 600%/s just to match
        public static float force = 500f; // This is enough to push Wisps and Beetle around a bit, but not too much
        public static float recoil = 3f;
        public static float range = 256f;

        // If we had a custom bullet prefabs and sprites, we'd take care of them here
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        public static Sprite skillIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("texPrimaryIcon");
        public static GameObject hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = duration;
            base.characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle"; // This is the bone the bullet will come out of afaik

            // Once the animations are complete, add an animation here
            //base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
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

                base.characterBody.AddSpreadBloom(1.5f); // Holding fire will eventually cause 1.5 degrees of spread

                // This is where the effects and sound would go
                //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, muzzleString, false);
                //Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-0.2f * Shoot.recoil, -0.2f * Shoot.recoil, -0.1f * Shoot.recoil, 0.1f * Shoot.recoil); // Low recoil since the firerate is high

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.SlowOnHit, // Here's where the slow effect is applied to the bullet
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 1f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 0f,
                        spreadYawScale = 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = hitEffectPrefab,
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
