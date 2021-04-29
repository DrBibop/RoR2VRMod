using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.EntityStates;
using RoR2;
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

        private static Vector3 abilityDirection = Vector3.zero;

        private static List<Type> nonDominantHandStateTypes = new List<Type>()
        {
            typeof(EntityStates.Commando.CommandoWeapon.FireFMJ),
            typeof(EntityStates.Bandit2.Weapon.Bandit2FireShiv),
            typeof(EntityStates.Toolbot.AimStunDrone),
            typeof(EntityStates.Engi.EngiWeapon.FireMines),
            typeof(EntityStates.Engi.EngiWeapon.FireSpiderMine),
            typeof(EntityStates.Engi.EngiWeapon.FireBubbleShield),
            typeof(EntityStates.Mage.Weapon.PrepWall),
            typeof(EntityStates.Mage.Weapon.ThrowNovabomb),
            typeof(EntityStates.Mage.Weapon.ThrowIcebomb),
            typeof(EntityStates.Merc.PrepAssaulter2),
            typeof(EntityStates.Merc.FocusedAssaultPrep),
            typeof(EntityStates.Treebot.Weapon.AimMortar2),
            typeof(EntityStates.Treebot.Weapon.AimMortarRain),
            typeof(EntityStates.Treebot.Weapon.FireSonicBoom),
            typeof(EntityStates.Treebot.Weapon.FirePlantSonicBoom),
            typeof(EntityStates.Loader.SwingChargedFist),
            typeof(EntityStates.Loader.SwingZapFist),
            typeof(EntityStates.Loader.FireHook),
            typeof(EntityStates.Loader.FireYankHook),
            typeof(EntityStates.Croco.FireSpit),
            typeof(EntityStates.Croco.Bite),
            typeof(EntityStates.Croco.Leap),
            typeof(EntityStates.Croco.ChainableLeap),
            typeof(EntityStates.Captain.Weapon.FireTazer),
            typeof(EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary)
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
            On.EntityStates.GenericBulletBaseState.GenerateBulletAttack += ChangeShotgunMuzzle;

            On.EntityStates.GenericProjectileBaseState.FireProjectile += SetFMJMuzzle;
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

            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter += ChangeSlashDirection;

            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += SetNailgunMuzzle;
            On.EntityStates.Toolbot.FireGrenadeLauncher.OnEnter += SetScrapMuzzle;
            On.EntityStates.Toolbot.FireSpear.FireBullet += SetRebarMuzzle;
            IL.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += SetGrenadeMuzzle;

            On.EntityStates.Engi.EngiWeapon.FireGrenades.FireGrenade += RemoveMuzzleFlash;

            On.EntityStates.Mage.Weapon.FireFireBolt.FireGauntlet += SetFireboltMuzzle;
            On.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += DestroyOffHandEffect;

            On.EntityStates.Merc.WhirlwindBase.OnEnter += ChangeWhirlwindDirection;
            On.EntityStates.Merc.WhirlwindBase.FixedUpdate += ForceWhirlwindDirection;
            On.EntityStates.Merc.Uppercut.OnEnter += ChangeUppercutDirection;
            On.EntityStates.Merc.Assaulter2.OnEnter += ForceDashDirection;
            On.EntityStates.Merc.FocusedAssaultDash.OnEnter += ForceFocusedDashDirection;

            On.EntityStates.Croco.Bite.OnEnter += ChangeBiteDirection;

            On.EntityStates.Captain.Weapon.FireTazer.OnEnter += ChangeTazerMuzzle;
        }

        private static void ChangeTazerMuzzle(On.EntityStates.Captain.Weapon.FireTazer.orig_OnEnter orig, EntityStates.Captain.Weapon.FireTazer self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = "MuzzleLeft";
            }

            orig(self);
        }

        private static void ChangeBiteDirection(On.EntityStates.Croco.Bite.orig_OnEnter orig, EntityStates.Croco.Bite self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            self.swingEffectMuzzleString = "MuzzleHandL";
        }

        private static void ForceDashDirection(On.EntityStates.Merc.Assaulter2.orig_OnEnter orig, EntityStates.Merc.Assaulter2 self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            self.dashVector = GetHandMuzzle(false).forward;
        }

        private static void ForceFocusedDashDirection(On.EntityStates.Merc.FocusedAssaultDash.orig_OnEnter orig, EntityStates.Merc.FocusedAssaultDash self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            self.dashVector = GetHandMuzzle(false).forward;
        }

        private static void ForceWhirlwindDirection(On.EntityStates.Merc.WhirlwindBase.orig_FixedUpdate orig, EntityStates.Merc.WhirlwindBase self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (self.characterDirection)
                    self.characterDirection.forward = abilityDirection;
            }

            orig(self);
        }

        private static void ChangeSlashDirection(On.EntityStates.Bandit2.Weapon.SlashBlade.orig_OnEnter orig, EntityStates.Bandit2.Weapon.SlashBlade self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            abilityDirection = GetHandMuzzle(false).forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeUppercutDirection(On.EntityStates.Merc.Uppercut.orig_OnEnter orig, EntityStates.Merc.Uppercut self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            abilityDirection = GetHandMuzzle(false).forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeWhirlwindDirection(On.EntityStates.Merc.WhirlwindBase.orig_OnEnter orig, EntityStates.Merc.WhirlwindBase self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            abilityDirection = GetHandMuzzle(false).forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void SetFireboltMuzzle(On.EntityStates.Mage.Weapon.FireFireBolt.orig_FireGauntlet orig, EntityStates.Mage.Weapon.FireFireBolt self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
                self.muzzleString = "MuzzleRight";

            orig(self);
        }

        private static void RemoveMuzzleFlash(On.EntityStates.Engi.EngiWeapon.FireGrenades.orig_FireGrenade orig, EntityStates.Engi.EngiWeapon.FireGrenades self, string targetMuzzle)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GameObject prefab = EntityStates.Engi.EngiWeapon.FireGrenades.effectPrefab;
                EntityStates.Engi.EngiWeapon.FireGrenades.effectPrefab = null;
                orig(self, targetMuzzle);
                EntityStates.Engi.EngiWeapon.FireGrenades.effectPrefab = prefab;
                return;
            }

            orig(self, targetMuzzle);
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
        private static void SetFMJMuzzle(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (self is EntityStates.Commando.CommandoWeapon.FireFMJ)
                    self.targetMuzzle = "MuzzleLeft";
            }
            orig(self);
        }

        private static BulletAttack ChangeShotgunMuzzle(On.EntityStates.GenericBulletBaseState.orig_GenerateBulletAttack orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) && self is EntityStates.Commando.CommandoWeapon.FireShotgunBlast)
            {
                Animator animator = GetHandAnimator(!self.muzzleName.Contains("Left"));

                if (animator)
                    animator.SetTrigger("Primary");
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
