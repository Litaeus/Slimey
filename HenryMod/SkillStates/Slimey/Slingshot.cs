using EntityStates;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SlimeyMod.SkillStates
{
    public class Slingshot : BaseSkillState
    {
        public static float duration = 0.5f;
        public static float initialSpeedCoefficient = 5f;
        public static float finalSpeedCoefficient = 2.5f;

        public static string dodgeSoundString = "SlimeySlingshot";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        private float SlingshotSpeed;
        private Vector3 forwardDirection;
        private Animator animator;
        private Vector3 previousPosition;
        private float damageCoefficient;
        private float force;
        private float procCoefficient;
        private object lookRay;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();

            if (base.isAuthority && base.inputBank && base.characterDirection)
            {
                forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            }

            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : forwardDirection;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);

            float num = Vector3.Dot(forwardDirection, rhs);
            float num2 = Vector3.Dot(forwardDirection, rhs2);

            RecalculateSlingshotSpeed();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity = forwardDirection * SlingshotSpeed;
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            previousPosition = base.transform.position - b;

            base.PlayAnimation("FullBody, Override", "Slingshot", "Slingshot.playbackRate", Slingshot.duration);
            Util.PlaySound(Slingshot.dodgeSoundString, base.gameObject);

            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(HenryModules.Buffs.armorBuff, 3f * Slingshot.duration);
                base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f * Slingshot.duration);
            }
        }

        private void RecalculateSlingshotSpeed()
        {
            SlingshotSpeed = moveSpeedStat * Mathf.Lerp(Slingshot.initialSpeedCoefficient, Slingshot.finalSpeedCoefficient, base.fixedAge / Slingshot.duration);
        }

      

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateSlingshotSpeed();

            if (base.characterDirection) base.characterDirection.forward = forwardDirection;
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Slingshot.dodgeFOV,
                60f,
                base.fixedAge / Slingshot.duration);

            Vector3 normalized = (base.transform.position - previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * SlingshotSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                vector.y = 0f;

                base.characterMotor.velocity = vector;
            }
            previousPosition = base.transform.position;

            if (base.isAuthority && base.fixedAge >= Slingshot.duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
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
                position = lookRay.GetPoint(5f), // The blast will be centered 1 units down the aimRay
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                radius = 0.5f, // If this matches the position distance, then it can hit directly in front of you
                teamIndex = base.GetTeam()
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }

            base.OnExit();

            base.characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
    }
}