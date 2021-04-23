using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static VRMod.MotionControls;

namespace VRMod
{
    internal class MotionControlledAbilities
    {
        private static CharacterBody localBody;

        private static bool preventBowPull = false;

        internal static void Init()
        {
            IL.RoR2.PlayerCharacterMasterController.FixedUpdate += SprintBreakDirection;

            On.EntityStates.GenericBulletBaseState.GenerateBulletAttack += CheckGenericBulletMuzzle;
            On.EntityStates.GenericProjectileBaseState.FireProjectile += FireProjectileOverride;

            On.EntityStates.Commando.CommandoWeapon.FirePistol2.FireBullet += CheckPistolBulletMuzzle;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += PlayBarrageShootAnimation;
            On.EntityStates.Commando.CommandoWeapon.FireFMJ.PlayAnimation += PlayFMJShootAnimation;

            On.EntityStates.Huntress.BlinkState.GetBlinkVector += GetNonDominantVector;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += AnimatePrimaryBowPull;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FireOrbArrow += AnimatePrimaryBowShoot;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnExit += PreventBowPull;
            On.EntityStates.Huntress.BaseArrowBarrage.OnEnter += AnimateBarrageBowPull;
            On.EntityStates.Huntress.BaseArrowBarrage.HandlePrimaryAttack += AnimateBarrageBowShoot;
            On.EntityStates.Huntress.AimArrowSnipe.HandlePrimaryAttack += AnimateSnipeBow;

            On.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += DestroyOffHandEffect;
        }

        private static void AnimateSnipeBow(On.EntityStates.Huntress.AimArrowSnipe.orig_HandlePrimaryAttack orig, EntityStates.Huntress.AimArrowSnipe self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Primary");

                if (self.primarySkillSlot.stock > 0)
                    GetHandAnimator(true).SetTrigger("Pull");
            }
        }

        private static void AnimateBarrageBowShoot(On.EntityStates.Huntress.BaseArrowBarrage.orig_HandlePrimaryAttack orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Primary");
            }
        }

        private static void AnimateBarrageBowPull(On.EntityStates.Huntress.BaseArrowBarrage.orig_OnEnter orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Pull");
            }
            orig(self);
        }

        private static void PreventBowPull(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnExit orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            preventBowPull = true;
            orig(self);
            preventBowPull = false;
        }

        private static void AnimatePrimaryBowShoot(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_FireOrbArrow orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            int firedCount = self.firedArrowCount;
            orig(self);
            if (self.firedArrowCount <= firedCount) return;

            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Primary");

                if (!preventBowPull && self.firedArrowCount < self.maxArrowCount)
                    GetHandAnimator(true).SetTrigger("Pull");
            }
        }

        private static void AnimatePrimaryBowPull(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnEnter orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            if(IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Pull");
            }
            orig(self);
        }

        private static void SprintBreakDirection(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerCharacterMasterController>("bodyInputs"),
                x => x.MatchCallvirt<InputBankTest>("get_aimDirection")
                );

            c.RemoveRange(4);

            c.Emit(OpCodes.Ldloc_S, (byte)11);
            c.EmitDelegate<Func<CameraRigController, Vector3>>((rig) =>
                {
                    return rig.sceneCam.transform.forward;
                }
            );
            c.Emit(OpCodes.Stloc_S, (byte)13);
        }

        private static Vector3 GetNonDominantVector(On.EntityStates.Huntress.BlinkState.orig_GetBlinkVector orig, EntityStates.Huntress.BlinkState self)
        {
            return GetHandRay(false).direction;
        }

        private static void PlayFMJShootAnimation(On.EntityStates.Commando.CommandoWeapon.FireFMJ.orig_PlayAnimation orig, EntityStates.Commando.CommandoWeapon.FireFMJ self, float duration)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(false).SetTrigger("Primary");
            }
            orig(self, duration);
        }

        private static void PlayBarrageShootAnimation(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, EntityStates.Commando.CommandoWeapon.FireBarrage self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Primary");
            }
            orig(self);
        }

        private static void DestroyOffHandEffect(On.EntityStates.Mage.Weapon.Flamethrower.orig_FireGauntlet orig, EntityStates.Mage.Weapon.Flamethrower self, string muzzleString)
        {
            orig(self, muzzleString);

            if (self.leftFlamethrowerTransform)
                GameObject.Destroy(self.leftFlamethrowerTransform.gameObject);
        }
        private static void FireProjectileOverride(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                orig(self);
                return;
            }

            if (self is EntityStates.Commando.CommandoWeapon.FireFMJ)
                self.targetMuzzle = "MuzzleLeft";

            if (self.isAuthority)
            {
                Ray aimRay = self.targetMuzzle.Contains("Left") ? GetHandRay(false) : self.GetAimRay();
                aimRay = self.ModifyProjectileAimRay(aimRay);
                aimRay.direction = Util.ApplySpread(aimRay.direction, self.minSpread, self.maxSpread, 1f, 1f, 0f, self.projectilePitchBonus);
                ProjectileManager.instance.FireProjectile(self.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), self.gameObject, self.damageStat * self.damageCoefficient, self.force, Util.CheckRoll(self.critStat, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }

        private static BulletAttack CheckGenericBulletMuzzle(On.EntityStates.GenericBulletBaseState.orig_GenerateBulletAttack orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (self.muzzleName.Contains("Left"))
                {
                    aimRay = GetHandRay(false);

                    Animator animator = GetHandAnimator(false);

                    if (animator)
                        animator.SetTrigger("Primary");
                }
                else
                {
                    Animator animator = GetHandAnimator(true);

                    if (animator)
                        animator.SetTrigger("Primary");
                }
            }

            return orig(self, aimRay);
        }

        private static void CheckPistolBulletMuzzle(On.EntityStates.Commando.CommandoWeapon.FirePistol2.orig_FireBullet orig, EntityStates.Commando.CommandoWeapon.FirePistol2 self, string targetMuzzle)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (targetMuzzle.Contains("Left"))
                {
                    self.aimRay = GetHandRay(false);

                    Animator animator = GetHandAnimator(false);

                    if (animator)
                        animator.SetTrigger("Primary");
                }
                else
                {
                    Animator animator = GetHandAnimator(true);

                    if (animator)
                        animator.SetTrigger("Primary");
                }
            }

            orig(self, targetMuzzle);
        }

        private static bool IsLocalPlayer(CharacterBody body)
        {
            if (!localBody) localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            return body == localBody;
        }
    }
}
