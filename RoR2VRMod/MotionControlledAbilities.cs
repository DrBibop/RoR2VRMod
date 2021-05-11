using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using RoR2;
using System;
using System.Collections;
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
            { typeof(PingerController), false },
            { typeof(InteractionDriver), false }
        };

        private static string[] multPrimarySkills = new string[]
        {
            "Hand",
            "FireNailgun",
            "FireSpear",
            "FireGrenadeLauncher",
            "FireBuzzsaw"
        };

        private static Run.FixedTimeStamp halfTime;

        private static bool hasSwapped;

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

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FireOrbArrow += AnimatePrimaryBowShoot;
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnExit += PreventBowPull;
            On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.OnEnter += DeleteEffect;
            On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.FireOrbGlaive += AnimateGlaiveThrow;
            On.EntityStates.Huntress.BlinkState.GetBlinkVector += GetNonDominantVector;
            On.EntityStates.Huntress.BaseArrowBarrage.OnEnter += AnimateBarrageBowPull;
            On.EntityStates.Huntress.BaseArrowBarrage.HandlePrimaryAttack += AnimateBarrageBowShoot;
            On.EntityStates.Huntress.AimArrowSnipe.HandlePrimaryAttack += AnimateSnipeBow;
            On.EntityStates.Huntress.BaseArrowBarrage.OnExit += HideArrowCluster;

            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter += ChangeSlashDirection;
            On.EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.OnEnter += PlayRevolverSpinAnimation;

            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += SetNailgunMuzzle;
            On.EntityStates.Toolbot.FireGrenadeLauncher.OnEnter += SetScrapMuzzle;
            On.EntityStates.Toolbot.FireSpear.FireBullet += SetRebarMuzzle;
            On.EntityStates.Toolbot.CooldownSpear.OnEnter += AnimateChargeSpear;
            On.EntityStates.Toolbot.FireNailgun.OnExit += AnimateSpinDown;
            On.EntityStates.Toolbot.FireBuzzsaw.OnExit += AnimateSawSlowdown;
            On.EntityStates.Toolbot.BaseToolbotPrimarySkillState.OnEnter += ForceBuzzsawMuzzle;
            On.EntityStates.Toolbot.ToolbotStanceSwap.OnEnter += AnimateRetoolRetract;
            On.EntityStates.Toolbot.ToolbotStanceSwap.FixedUpdate += AnimateRetoolExtend;
            On.EntityStates.Toolbot.ToolbotDualWieldStart.OnEnter += AnimateDualWieldRetract;
            On.EntityStates.Toolbot.ToolbotDualWieldStart.FixedUpdate += AnimateDualWieldExtend;
            On.EntityStates.Toolbot.ToolbotDualWield.OnExit += AnimateDualWieldEnd;
            onHandPairSet += SetInitialTool;
            On.EntityStates.Toolbot.AimStunDrone.OnExit += AnimateGrenadeThrow;
            IL.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += SetGrenadeMuzzle;

            On.EntityStates.Engi.EngiWeapon.FireGrenades.FireGrenade += RemoveMuzzleFlash;
            On.EntityStates.Engi.EngiWeapon.ChargeGrenades.OnExit += AnimateGrenadeRelease;
            On.EntityStates.Engi.EngiMissilePainter.Paint.OnExit += AnimateHarpoonRelease;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.OnExit += AnimateBlueprintRelease;

            On.EntityStates.Mage.Weapon.FireFireBolt.FireGauntlet += SetFireboltMuzzle;
            On.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += DestroyOffHandEffect;
            On.EntityStates.Mage.Weapon.Flamethrower.OnExit += StopCastAnimation;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.OnEnter += AnimateBombCast;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += AnimateWallCast;

            On.EntityStates.Merc.WhirlwindBase.OnEnter += ChangeWhirlwindDirection;
            On.EntityStates.Merc.WhirlwindBase.FixedUpdate += ForceWhirlwindDirection;
            On.EntityStates.Merc.Uppercut.OnEnter += ChangeUppercutDirection;
            On.EntityStates.Merc.Assaulter2.OnEnter += ForceDashDirection;
            On.EntityStates.Merc.FocusedAssaultDash.OnEnter += ForceFocusedDashDirection;

            On.EntityStates.Croco.Bite.OnEnter += ChangeBiteDirection;

            On.EntityStates.Captain.Weapon.FireTazer.OnEnter += ChangeTazerMuzzleEnter;
            On.EntityStates.Captain.Weapon.FireTazer.Fire += ChangeTazerMuzzleShoot;

            IL.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.OnEnter += ChangeNeedleMuzzle;
        }

        private static void AnimateWallCast(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(false).SetTrigger("Cast");
            }
        }

        private static void AnimateBombCast(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_OnEnter orig, EntityStates.Mage.Weapon.BaseThrowBombState self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(false).SetTrigger("Cast");
            }
        }

        private static void StopCastAnimation(On.EntityStates.Mage.Weapon.Flamethrower.orig_OnExit orig, EntityStates.Mage.Weapon.Flamethrower self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).ResetTrigger("Cast");
                GetHandAnimator(true).SetBool("HoldCast", false);
            }
        }

        private static void AnimateBlueprintRelease(On.EntityStates.Engi.EngiWeapon.PlaceTurret.orig_OnExit orig, EntityStates.Engi.EngiWeapon.PlaceTurret self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Release");
            }
        }

        private static void AnimateHarpoonRelease(On.EntityStates.Engi.EngiMissilePainter.Paint.orig_OnExit orig, EntityStates.Engi.EngiMissilePainter.Paint self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Release");
            }
        }

        private static void AnimateGrenadeRelease(On.EntityStates.Engi.EngiWeapon.ChargeGrenades.orig_OnExit orig, EntityStates.Engi.EngiWeapon.ChargeGrenades self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetTrigger("Release");
            }
        }

        private static void ForceBuzzsawMuzzle(On.EntityStates.Toolbot.BaseToolbotPrimarySkillState.orig_OnEnter orig, EntityStates.Toolbot.BaseToolbotPrimarySkillState self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (self.baseMuzzleName == "MuzzleBuzzsaw" && self.isInDualWield)
                {
                    self.muzzleName = self.baseMuzzleName;

                    if (self.activatorSkillSlot == self.skillLocator.primary)
                    {
                        self.muzzleTransform = GetHandMuzzleByIndex(true, 1);
                    }
                    else
                    {
                        self.muzzleTransform = GetHandMuzzleByIndex(false, 1);
                    }
                }
            }
        }

        private static void AnimateDualWieldEnd(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnExit orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(false).SetFloat("RetoolSpeed", 4f);
                GetHandAnimator(false).SetTrigger("RetoolRetract");
                RoR2Application.instance.StartCoroutine(ExtendAfterDelay());
            }
        }

        private static IEnumerator ExtendAfterDelay()
        {
            yield return new WaitForSeconds(0.25f);
            GetHandAnimator(false).SetInteger("ToolID", 0);
            GetHandAnimator(false).SetTrigger("RetoolExtend");
        }

        private static void AnimateDualWieldExtend(On.EntityStates.Toolbot.ToolbotDualWieldStart.orig_FixedUpdate orig, EntityStates.Toolbot.ToolbotDualWieldStart self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (halfTime.hasPassed && !hasSwapped)
                {
                    hasSwapped = true;

                    GenericSkill currentSkill = self.primary2Slot;

                    if (currentSkill)
                    {
                        GetHandAnimator(false).SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                        GetHandAnimator(false).SetTrigger("RetoolExtend");
                    }
                }
            }
        }

        private static void AnimateDualWieldRetract(On.EntityStates.Toolbot.ToolbotDualWieldStart.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWieldStart self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                hasSwapped = false;
                halfTime = Run.FixedTimeStamp.now + (self.duration / 2);
                GetHandAnimator(false).SetFloat("RetoolSpeed", 2f / self.duration);
                GetHandAnimator(false).SetTrigger("RetoolRetract");
            }
        }

        private static void AnimateSawSlowdown(On.EntityStates.Toolbot.FireBuzzsaw.orig_OnExit orig, EntityStates.Toolbot.FireBuzzsaw self)
        {
            orig(self);

            CharacterBody body = self.outer.GetComponent<CharacterBody>();
            if (IsLocalPlayer(body))
            {
                GetHandAnimator(body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary).SetTrigger("BuzzsawSlowdown");
            }
        }

        private static void AnimateGrenadeThrow(On.EntityStates.Toolbot.AimStunDrone.orig_OnExit orig, EntityStates.Toolbot.AimStunDrone self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).ResetTrigger("AimGrenade");
                GetHandAnimator(false).SetTrigger("ThrowGrenade");
            }
        }

        private static void AnimateChargeSpear(On.EntityStates.Toolbot.CooldownSpear.orig_OnEnter orig, EntityStates.Toolbot.CooldownSpear self)
        {
            orig(self);

            CharacterBody body = self.outer.GetComponent<CharacterBody>();
            if (IsLocalPlayer(body))
            {
                bool dominant = body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary;
                GetHandAnimator(dominant).SetFloat("SpearChargeSpeed", 1f / self.duration);
                GetHandAnimator(dominant).SetTrigger("SpearCharge");
            }
        }

        private static void SetInitialTool(CharacterBody body)
        {
            if (body.name == "ToolbotBody(Clone)")
            {
                GenericSkill currentSkill = body.skillLocator.primary;

                if (currentSkill)
                {
                    GetHandAnimator(true).SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                }
            }
        }

        private static void AnimateRetoolExtend(On.EntityStates.Toolbot.ToolbotStanceSwap.orig_FixedUpdate orig, EntityStates.Toolbot.ToolbotStanceSwap self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (halfTime.hasPassed && !hasSwapped)
                {
                    hasSwapped = true;

                    GenericSkill currentSkill = self.previousStanceState == typeof(EntityStates.Toolbot.ToolbotStanceA) ? self.GetPrimarySkill2() : self.GetPrimarySkill1();

                    if (currentSkill)
                    {
                        GetHandAnimator(true).SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                        GetHandAnimator(true).SetTrigger("RetoolExtend");
                    }
                }
            }
        }

        private static void AnimateRetoolRetract(On.EntityStates.Toolbot.ToolbotStanceSwap.orig_OnEnter orig, EntityStates.Toolbot.ToolbotStanceSwap self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                hasSwapped = false;
                halfTime = Run.FixedTimeStamp.now + (self.baseDuration / (self.attackSpeedStat * 2));
                GetHandAnimator(true).SetFloat("RetoolSpeed", 2f / (self.baseDuration / self.attackSpeedStat));
                GetHandAnimator(true).SetTrigger("RetoolRetract");
            }
        }

        private static void AnimateSpinDown(On.EntityStates.Toolbot.FireNailgun.orig_OnExit orig, EntityStates.Toolbot.FireNailgun self)
        {
            orig(self);

            CharacterBody body = self.outer.GetComponent<CharacterBody>();
            if (IsLocalPlayer(body))
            {
                GetHandAnimator(body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary).SetTrigger("NailgunSlowdown");
            }
        }

        private static void DeleteEffect(On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.orig_OnEnter orig, EntityStates.Huntress.HuntressWeapon.ThrowGlaive self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) && self.chargeEffect)
            {
                EntityStates.EntityState.Destroy(self.chargeEffect);
            }
        }

        private static void AnimateGlaiveThrow(On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.orig_FireOrbGlaive orig, EntityStates.Huntress.HuntressWeapon.ThrowGlaive self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) && self.hasSuccessfullyThrownGlaive)
            {
                GetHandAnimator(false).SetTrigger("ThrowGlaive");
            }
        }

        private static void PlayRevolverSpinAnimation(On.EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.orig_OnEnter orig, EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState self)
        {
            orig(self);
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                GetHandAnimator(true).SetFloat("SpinSpeed", 1f / self.duration);
                GetHandAnimator(true).SetTrigger("RevolverSpin");
            }
        }

        private static void ChangeNeedleMuzzle(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdstr("Head")
                );

            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle, string>>((self) =>
            {
                return IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) ? "CurrentDominantMuzzle" : "Head";
            });
        }

        private static void ChangeTazerMuzzleShoot(On.EntityStates.Captain.Weapon.FireTazer.orig_Fire orig, EntityStates.Captain.Weapon.FireTazer self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                string muzzleString = EntityStates.Captain.Weapon.FireTazer.targetMuzzle;
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = "VRMuzzle_NonDominant";
                orig(self);
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = muzzleString;
                return;
            }

            orig(self);
        }

        private static void ChangeTazerMuzzleEnter(On.EntityStates.Captain.Weapon.FireTazer.orig_OnEnter orig, EntityStates.Captain.Weapon.FireTazer self)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                string muzzleString = EntityStates.Captain.Weapon.FireTazer.targetMuzzle;
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = "VRMuzzle_NonDominant";
                orig(self);
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = muzzleString;
                return;
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

            self.dashVector = GetHandCurrentMuzzle(false).forward;
        }

        private static void ForceFocusedDashDirection(On.EntityStates.Merc.FocusedAssaultDash.orig_OnEnter orig, EntityStates.Merc.FocusedAssaultDash self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            self.dashVector = GetHandCurrentMuzzle(false).forward;
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

            abilityDirection = GetHandCurrentMuzzle(false).forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeUppercutDirection(On.EntityStates.Merc.Uppercut.orig_OnEnter orig, EntityStates.Merc.Uppercut self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            abilityDirection = GetHandCurrentMuzzle(false).forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeWhirlwindDirection(On.EntityStates.Merc.WhirlwindBase.orig_OnEnter orig, EntityStates.Merc.WhirlwindBase self)
        {
            orig(self);

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            abilityDirection = GetHandCurrentMuzzle(false).forward;

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
                return IsLocalPlayer(self.outer.GetComponent<CharacterBody>()) ? "VRMuzzle_NonDominant" : "MuzzleNailgun";
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
                return GetHandCurrentMuzzle(false).transform.forward;
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

            for (int i = 0; i < 4; i++)
            {
                c.GotoNext(x => x.MatchCallvirt(typeof(Player), "GetButton"));

                c.Remove();
                c.EmitDelegate<Func<Player, int, bool>>((player, button) =>
                    {
                        return player.GetButton(button) || MeleeSwingAbility.skillStates[button - 7];
                    }
                );
            }
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

            if (!IsLocalPlayer(self.outer.GetComponent<CharacterBody>())) return;

            if (self.leftFlamethrowerTransform)
                GameObject.Destroy(self.leftFlamethrowerTransform.gameObject);

            GetHandAnimator(true).SetBool("HoldCast", true);
            GetHandAnimator(true).SetTrigger("Cast");
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
