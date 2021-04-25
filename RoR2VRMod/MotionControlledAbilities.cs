using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static VRMod.MotionControls;

namespace VRMod
{
    internal class MotionControlledAbilities
    {
        private static CharacterBody localBody;

        private static bool preventBowPull = false;

        private static List<Type> nonDominantHandStateTypes = new List<Type>()
        {
            typeof(EntityStates.Commando.CommandoWeapon.FireFMJ),
            typeof(EntityStates.Toolbot.AimStunDrone),
            typeof(EntityStates.Mage.Weapon.PrepWall)
        };

        private static Dictionary<Type, bool> forceAimRaySideTypes = new Dictionary<Type, bool>()
        {
            { typeof(RoR2.PingerController), false },
            { typeof(RoR2.InteractionDriver), false }
        };

        internal static void Init()
        {
            IL.RoR2.PlayerCharacterMasterController.FixedUpdate += SprintBreakDirection;
            On.RoR2.PlayerCharacterMasterController.CheckPinging += PingFromHand;
            On.RoR2.CameraRigController.ModifyAimRayIfApplicable += CancelModifyIfLocal;
            On.RoR2.EquipmentSlot.GetAimRay += GetLeftAimRay;

            On.EntityStates.BaseState.GetAimRay += EditAimray;
            On.EntityStates.GenericBulletBaseState.GenerateBulletAttack += CheckGenericBulletMuzzle;
            On.EntityStates.GenericProjectileBaseState.FireProjectile += FireProjectileOverride;

            On.EntityStates.Commando.CommandoWeapon.FirePistol2.FireBullet += CheckPistolBulletMuzzle;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += PlayBarrageShootAnimation;
            On.EntityStates.Commando.CommandoWeapon.FireFMJ.PlayAnimation += PlayFMJShootAnimation;

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += AnimatePrimaryBowPull;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FireOrbArrow += AnimatePrimaryBowShoot;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnExit += PreventBowPull;
            On.EntityStates.Huntress.BlinkState.GetBlinkVector += GetNonDominantVector;
            On.EntityStates.Huntress.BaseArrowBarrage.OnEnter += AnimateBarrageBowPull;
            On.EntityStates.Huntress.BaseArrowBarrage.HandlePrimaryAttack += AnimateBarrageBowShoot;
            On.EntityStates.Huntress.AimArrowSnipe.HandlePrimaryAttack += AnimateSnipeBow;
            On.EntityStates.Huntress.BaseArrowBarrage.OnExit += HideArrowCluster;

            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += SetNailgunMuzzle;
            On.EntityStates.Toolbot.FireGrenadeLauncher.OnEnter += SetScrapMuzzle;
            On.EntityStates.Toolbot.FireSpear.FireBullet += SetRebarMuzzle;
            IL.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += SetGrenadeMuzzle;

            On.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += DestroyOffHandEffect;
        }

        private static Ray GetLeftAimRay(On.RoR2.EquipmentSlot.orig_GetAimRay orig, EquipmentSlot self)
        {
            return GetHandRayBySide(true);
        }

        private static Ray CancelModifyIfLocal(On.RoR2.CameraRigController.orig_ModifyAimRayIfApplicable orig, Ray originalAimRay, GameObject target, out float extraRaycastDistance)
        {
            if (IsLocalPlayer(target.GetComponent<CharacterBody>()))
            {
                extraRaycastDistance = 0;

                StackTrace stackTrace = new StackTrace();
                Type callerType = stackTrace.GetFrame(2).GetMethod().DeclaringType;

                bool useLeftHand;
                if (forceAimRaySideTypes.TryGetValue(callerType, out useLeftHand))
                {
                    return GetHandRayBySide(useLeftHand);
                }
                
                return originalAimRay;
            }

            return orig(originalAimRay, target, out extraRaycastDistance);
        }

        private static void PingFromHand(On.RoR2.PlayerCharacterMasterController.orig_CheckPinging orig, PlayerCharacterMasterController self)
        {
            if (!self.isLocalPlayer)
            {
                orig(self);
                return;
            }

            if (self.hasEffectiveAuthority && self.body && self.bodyInputs && self.bodyInputs.ping.justPressed)
            {
                self.pingerController.AttemptPing(GetHandRayBySide(false), self.body.gameObject);
            }
        }

        private static void SetNailgunMuzzle(On.EntityStates.Toolbot.BaseNailgunState.orig_FireBullet orig, EntityStates.Toolbot.BaseNailgunState self, Ray aimRay, int bulletCount, float spreadPitchScale, float spreadYawScale)
        {
            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);

            string origName = EntityStates.Toolbot.BaseNailgunState.muzzleName;
            EntityStates.Toolbot.BaseNailgunState.muzzleName = ((EntityStates.Toolbot.IToolbotPrimarySkillState)self).muzzleName;
            orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);
            EntityStates.Toolbot.BaseNailgunState.muzzleName = origName;
        }

        private static void SetGrenadeMuzzle(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdstr("MuzzleNailgun")
            );

            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<EntityStates.Toolbot.RecoverAimStunDrone, string>>((self) =>
            {
                return IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) ? "DualWieldMuzzleR" : "MuzzleNailgun";
            });
        }

        private static void SetRebarMuzzle(On.EntityStates.Toolbot.FireSpear.orig_FireBullet orig, EntityStates.Toolbot.FireSpear self, Ray aimRay)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                ((EntityStates.GenericBulletBaseState)self).muzzleName = ((EntityStates.Toolbot.IToolbotPrimarySkillState)self).muzzleName;
            }
            orig(self, aimRay);
        }

        private static void SetScrapMuzzle(On.EntityStates.Toolbot.FireGrenadeLauncher.orig_OnEnter orig, EntityStates.Toolbot.FireGrenadeLauncher self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                self.targetMuzzle = (self as EntityStates.Toolbot.IToolbotPrimarySkillState).muzzleName;
            }
        }

        private static Vector3 GetNonDominantVector(On.EntityStates.Huntress.BlinkState.orig_GetBlinkVector orig, EntityStates.Huntress.BlinkState self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                return GetHandMuzzle(false).transform.forward;
            }
            return orig(self);
        }

        private static Ray EditAimray(On.EntityStates.BaseState.orig_GetAimRay orig, EntityStates.BaseState self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (self.outer.customName == "Weapon2" || nonDominantHandStateTypes.Contains(self.GetType()))
                    return GetHandRayByDominance(false);

                if (self is EntityStates.GenericProjectileBaseState)
                {
                    EntityStates.GenericProjectileBaseState projSelf = (EntityStates.GenericProjectileBaseState)self;

                    if (projSelf.targetMuzzle.Contains("Left") || projSelf.targetMuzzle == "DualWieldMuzzleR")
                        return GetHandRayByDominance(false);
                }

                if (self is EntityStates.GenericBulletBaseState && ((EntityStates.GenericBulletBaseState)self).muzzleName.Contains("Left"))
                    return GetHandRayByDominance(false);
            }
            return orig(self);
        }

        private static void HideArrowCluster(On.EntityStates.Huntress.BaseArrowBarrage.orig_OnExit orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetBool("ShowCluster", false);
            }
            orig(self);
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
                GetHandAnimator(true).SetBool("ShowCluster", true);
                GetHandAnimator(true).SetTrigger("Pull");
            }
            orig(self);
        }

        private static void PreventBowPull(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnExit orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) orig(self);

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
                if (self is EntityStates.Commando.CommandoWeapon.FireFMJ)
                    self.targetMuzzle = "MuzzleLeft";
            }
            orig(self);
        }

        private static BulletAttack CheckGenericBulletMuzzle(On.EntityStates.GenericBulletBaseState.orig_GenerateBulletAttack orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) && self is EntityStates.Commando.CommandoWeapon.FireShotgunBlast)
            {
                if (self.muzzleName.Contains("Left"))
                {
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
                    self.aimRay = GetHandRayByDominance(false);

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
