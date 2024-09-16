using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static VRMod.MotionControls;

namespace VRMod
{
    internal class MotionControlledAbilities
    {
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
            typeof(EntityStates.Loader.ThrowPylon),
            typeof(EntityStates.Loader.FireHook),
            typeof(EntityStates.Loader.FireYankHook),
            typeof(EntityStates.Croco.FireSpit),
            typeof(EntityStates.Croco.Bite),
            typeof(EntityStates.Croco.Leap),
            typeof(EntityStates.Croco.ChainableLeap),
            typeof(EntityStates.Captain.Weapon.FireTazer),
            typeof(EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary),
            typeof(EntityStates.Railgunner.Weapon.FireMineConcussive),
            typeof(EntityStates.Railgunner.Weapon.FireMineBlinding),
            typeof(EntityStates.VoidSurvivor.Weapon.FireMegaBlasterBig),
            typeof(EntityStates.VoidSurvivor.Weapon.FireMegaBlasterSmall),
            typeof(EntityStates.VoidSurvivor.Weapon.FireCorruptDisks)
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

        private static bool isSwapping;

        private struct ScaledEffect
        {
            internal ScaledEffect(GameObject prefab, float scale)
            {
                effectPrefab = prefab;
                effectScale = scale;
            }

            internal GameObject effectPrefab;
            internal float effectScale;
        }

        internal static void Init()
        {
            List<ScaledEffect> scaledMuzzleFlashes = new List<ScaledEffect>()
            {
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Muzzleflash1"), 0.4f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashFMJ"), 0.4f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashBarrage"), 0.4f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashHuntress"), 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashEngiGrenade"), 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageFire"), 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageLightning"), 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageLightningLarge").transform.Find("Particles").gameObject, 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageIceLarge").transform.Find("Particles").gameObject, 0.5f),
                new ScaledEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageLightningLargeWithTrail").transform.Find("Particles").gameObject, 0.5f),
                new ScaledEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab").WaitForCompletion(), 0.4f),
                new ScaledEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab").WaitForCompletion(), 0.6f),
                new ScaledEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorReadyMegaBlaster.prefab").WaitForCompletion(), 0.6f),
                new ScaledEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeCrushCorruption.prefab").WaitForCompletion(), 0.6f),
                new ScaledEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeCrushHealth.prefab").WaitForCompletion(), 0.6f)
            };

            foreach (var muzzleFlash in scaledMuzzleFlashes)
            {
                foreach (Transform child in muzzleFlash.effectPrefab.transform)
                {
                    if (child.name.ToLower().Contains("light")) continue;
                    child.localScale *= muzzleFlash.effectScale;
                }
            }

            GameObject banditSmokeBomb = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Bandit2SmokeBomb");
            banditSmokeBomb.transform.Find("Core/Dust, CenterTube").gameObject.SetActive(false);

            GameObject voidFiendCorruptBeam = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamCorrupt.prefab").WaitForCompletion();
            Transform beamOffset = voidFiendCorruptBeam.transform.Find("Offset");
            beamOffset.localPosition = new Vector3(0, 0, 0.1f);
            beamOffset.localScale = new Vector3(0.6f, 0.6f, 1);

            IL.RoR2.PlayerCharacterMasterController.FixedUpdate += SprintBreakDirection;
            On.RoR2.PlayerCharacterMasterController.CheckPinging += PingFromHand;
            On.RoR2.CameraRigController.ModifyAimRayIfApplicable += CancelModifyIfLocal;
            On.RoR2.EquipmentSlot.GetAimRay += GetLeftAimRay;
            On.EntityStates.BaseState.GetAimRay += EditAimray;

            On.EntityStates.GenericBulletBaseState.FireBullet += ForceShotgunMuzzle;

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
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += AnimateDashExtend;
            IL.EntityStates.Toolbot.RecoverAimStunDrone.OnEnter += SetGrenadeMuzzle;

            On.EntityStates.Engi.EngiWeapon.FireGrenades.FireGrenade += RemoveMuzzleFlash;
            On.EntityStates.Engi.EngiWeapon.ChargeGrenades.OnExit += AnimateGrenadeRelease;
            On.EntityStates.Engi.EngiMissilePainter.Paint.OnExit += AnimateHarpoonRelease;
            On.EntityStates.Engi.EngiWeapon.PlaceTurret.OnExit += AnimateBlueprintRelease;

            On.EntityStates.Mage.Weapon.FireFireBolt.FireGauntlet += SetFireboltMuzzle;
            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += ShrinkFireEffect;
            On.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += DestroyOffHandEffect;
            On.EntityStates.Mage.Weapon.Flamethrower.OnExit += StopCastAnimation;
            On.EntityStates.Mage.Weapon.BaseChargeBombState.OnEnter += ShrinkNovaBomb;
            On.EntityStates.Mage.Weapon.BaseThrowBombState.OnEnter += AnimateBombCast;
            On.EntityStates.Mage.Weapon.PrepWall.OnExit += AnimateWallCast;

            On.EntityStates.Merc.WhirlwindBase.OnEnter += ChangeWhirlwindDirection;
            On.EntityStates.Merc.WhirlwindBase.FixedUpdate += ForceWhirlwindDirection;
            On.EntityStates.Merc.Uppercut.OnEnter += ChangeUppercutDirection;
            On.EntityStates.Merc.Assaulter2.OnEnter += ForceDashDirection;
            On.EntityStates.Merc.FocusedAssaultDash.OnEnter += ForceFocusedDashDirection;

            On.EntityStates.Treebot.Weapon.FireSyringe.FixedUpdate += AnimateSyringeShoot;
            On.EntityStates.Treebot.Weapon.FireSyringe.OnExit += EndSyringeShoot;
            On.EntityStates.Treebot.Weapon.FireSonicBoom.OnEnter += AnimateSonicBoom;

            On.EntityStates.Loader.FireHook.OnEnter += AnimateHookEnter;
            On.EntityStates.Loader.FireHook.OnExit += AnimateHookExit;
            On.EntityStates.Loader.BaseChargeFist.OnEnter += ShowLoaderRay;
            On.EntityStates.Loader.BaseChargeFist.OnExit += HideLoaderRay;
            On.RoR2.Projectile.ProjectileGrappleController.BaseState.GetOwnerAimRay += GetGrappleArmRay;

            On.EntityStates.Croco.Bite.OnEnter += ChangeBiteDirection;
            On.EntityStates.Croco.BaseLeap.OnExit += AnimateAcridRest;

            On.EntityStates.Captain.Weapon.ChargeCaptainShotgun.OnEnter += ShrinkChargeEffect;
            On.EntityStates.Captain.Weapon.FireCaptainShotgun.OnEnter += AnimateShotgunShoot;
            On.EntityStates.Captain.Weapon.FireTazer.OnEnter += ChangeTazerMuzzleEnter;
            On.EntityStates.Captain.Weapon.FireTazer.Fire += ChangeTazerMuzzleShoot;

            On.EntityStates.Railgunner.Scope.BaseWindUp.OnEnter += SetScopeFOV;
            On.EntityStates.Railgunner.Scope.BaseScopeState.OnEnter += RemoveOverlay;
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnEnter += ChangeSniperMuzzle;

            On.EntityStates.VoidSurvivor.VoidBlinkBase.OnEnter += ChangeBlinkDirection;

            IL.EntityStates.GlobalSkills.LunarNeedle.FireLunarNeedle.OnEnter += ChangeNeedleMuzzle;
        }

        private static void ChangeSniperMuzzle(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_OnEnter orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self)
        {
            if (self.characterBody.IsLocalBody())
            {
                self.muzzleName = "MuzzleSniper";
            }

            orig(self);
        }

        private static void RemoveOverlay(On.EntityStates.Railgunner.Scope.BaseScopeState.orig_OnEnter orig, EntityStates.Railgunner.Scope.BaseScopeState self)
        {
            Transform overlay = self.scopeOverlayPrefab.transform.Find("ScopeOverlay");

            if (overlay) GameObject.Destroy(overlay.gameObject);

            orig(self);

            if (self.characterBody.IsLocalBody())
            {
                self.overlayController.onInstanceAdded += (controller, instance) =>
                {
                    GetHandByDominance(true).currentHand.GetComponent<SniperScopeController>().SetOverlay(instance);
                };
                return;
            }
        }

        private static void SetScopeFOV(On.EntityStates.Railgunner.Scope.BaseWindUp.orig_OnEnter orig, EntityStates.Railgunner.Scope.BaseWindUp self)
        {
            orig(self);

            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).currentHand.GetComponent<SniperScopeController>().SetFOV(self.cameraParams.data.fov.value / ModConfig.RailgunnerZoomMultiplier.Value);
                GetHandByDominance(true).animator.SetBool("DisableRayOnScope", ModConfig.RailgunnerDisableScopeRay.Value);
            }
        }

        private static void ChangeBlinkDirection(On.EntityStates.VoidSurvivor.VoidBlinkBase.orig_OnEnter orig, EntityStates.VoidSurvivor.VoidBlinkBase self)
        {
            orig(self);

            if (self.characterBody.IsLocalBody())
            {
                Vector3 direction = GetHandByDominance(false).muzzle.forward;
                direction.y = 0;
                direction.Normalize();
                self.forwardVector = direction;
            }
        }

        private static Ray GetGrappleArmRay(On.RoR2.Projectile.ProjectileGrappleController.BaseState.orig_GetOwnerAimRay orig, EntityStates.BaseState self)
        {
            RoR2.Projectile.ProjectileGrappleController.BaseState state = self as RoR2.Projectile.ProjectileGrappleController.BaseState;

            CharacterBody body = state.owner.characterBody;

            if (body && body.IsLocalBody() && body.gameObject.name.Contains("LoaderBody"))
            {
                return GetHandByDominance(false).aimRay;
            }
            else
            {
                return orig(self);
            }
        }

        private static void ShrinkNovaBomb(On.EntityStates.Mage.Weapon.BaseChargeBombState.orig_OnEnter orig, EntityStates.Mage.Weapon.BaseChargeBombState self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody() && self is EntityStates.Mage.Weapon.ChargeNovabomb && self.chargeEffectInstance)
            {
                ObjectScaleCurve scaleCurve = self.chargeEffectInstance.GetComponent<ObjectScaleCurve>();

                if (scaleCurve)
                {
                    Keyframe key = scaleCurve.overallCurve.keys[1];
                    key.value = 0.5f;
                    scaleCurve.overallCurve.MoveKey(1, key);
                }
            }
        }

        private static void AnimateDashExtend(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                if (isSwapping)
                {
                    isSwapping = false;
                    RoR2Application.instance.StartCoroutine(ExtendAfterDelay());
                }
                else
                {
                    Animator animator = GetHandByDominance(false).animator;

                    if (animator.GetInteger("ToolID") != 0)
                    {
                        animator.SetFloat("RetoolSpeed", 4f);
                        animator.SetTrigger("RetoolRetract");
                        RoR2Application.instance.StartCoroutine(ExtendAfterDelay());
                    }
                }
            }
        }

        private static void ForceShotgunMuzzle(On.EntityStates.GenericBulletBaseState.orig_FireBullet orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            if (!ModConfig.CommandoDualWield.Value && self.characterBody.IsLocalBody() && self is EntityStates.Commando.CommandoWeapon.FireShotgunBlast)
            {
                self.muzzleName = "MuzzleLeft";
            }
            orig(self, aimRay);
        }

        private static void ShrinkChargeEffect(On.EntityStates.Captain.Weapon.ChargeCaptainShotgun.orig_OnEnter orig, EntityStates.Captain.Weapon.ChargeCaptainShotgun self)
        {
            foreach (Transform child in EntityStates.Captain.Weapon.ChargeCaptainShotgun.holdChargeVfxPrefab.transform)
            {
                if (child.name.ToLower().Contains("light")) continue;
                child.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            if (self.characterBody.IsLocalBody())
            {
                foreach (Transform child in EntityStates.Captain.Weapon.ChargeCaptainShotgun.chargeupVfxPrefab.transform)
                {
                    if (child.name.ToLower().Contains("light")) continue;
                    child.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            orig(self);
        }

        private static void ShrinkFireEffect(On.EntityStates.Mage.Weapon.Flamethrower.orig_OnEnter orig, EntityStates.Mage.Weapon.Flamethrower self)
        {
            Transform effect1 = self.flamethrowerEffectPrefab.transform.GetChild(4);
            Transform effect2 = self.flamethrowerEffectPrefab.transform.GetChild(5);

            effect1.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            effect2.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            effect1.localPosition = Vector3.zero;
            effect2.localPosition = Vector3.zero;

            orig(self);
        }

        private static void AnimateShotgunShoot(On.EntityStates.Captain.Weapon.FireCaptainShotgun.orig_OnEnter orig, EntityStates.Captain.Weapon.FireCaptainShotgun self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetFloat("ShootSpeed", 1f / self.duration);
                GetHandByDominance(true).animator.SetTrigger("Shoot");
            }
        }

        private static void AnimateAcridRest(On.EntityStates.Croco.BaseLeap.orig_OnExit orig, EntityStates.Croco.BaseLeap self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Rest");
                GetHandByDominance(false).animator.SetTrigger("Rest");
            }
        }

        private static void HideLoaderRay(On.EntityStates.Loader.BaseChargeFist.orig_OnExit orig, EntityStates.Loader.BaseChargeFist self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetBool("Ray", false);
            }
        }

        private static void ShowLoaderRay(On.EntityStates.Loader.BaseChargeFist.orig_OnEnter orig, EntityStates.Loader.BaseChargeFist self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetBool("Ray", true);
            }
        }

        private static void AnimateHookExit(On.EntityStates.Loader.FireHook.orig_OnExit orig, EntityStates.Loader.FireHook self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetBool("Hook", false);
            }
        }

        private static void AnimateHookEnter(On.EntityStates.Loader.FireHook.orig_OnEnter orig, EntityStates.Loader.FireHook self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetBool("Hook", true);
            }
        }

        private static void AnimateSonicBoom(On.EntityStates.Treebot.Weapon.FireSonicBoom.orig_OnEnter orig, EntityStates.Treebot.Weapon.FireSonicBoom self)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetTrigger("Push");
            }
            orig(self);
        }

        private static void EndSyringeShoot(On.EntityStates.Treebot.Weapon.FireSyringe.orig_OnExit orig, EntityStates.Treebot.Weapon.FireSyringe self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetInteger("SyringeShots", 0);

                if (self.projectilesFired < EntityStates.Treebot.Weapon.FireSyringe.projectileCount)
                {
                    GetHandByDominance(true).animator.SetTrigger("ForceReload");
                }
            }
        }

        private static void AnimateSyringeShoot(On.EntityStates.Treebot.Weapon.FireSyringe.orig_FixedUpdate orig, EntityStates.Treebot.Weapon.FireSyringe self)
        {
            if (!self.characterBody.IsLocalBody())
            {
                orig(self);
                return;
            }

            if (self.projectilesFired <= 0)
            {
                GetHandByDominance(true).animator.SetFloat("ReloadSpeed", self.attackSpeedStat);
            }
            orig(self);
            GetHandByDominance(true).animator.SetInteger("SyringeShots", self.projectilesFired);
        }

        private static void AnimateWallCast(On.EntityStates.Mage.Weapon.PrepWall.orig_OnExit orig, EntityStates.Mage.Weapon.PrepWall self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetTrigger("Cast");
            }
        }

        private static void AnimateBombCast(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_OnEnter orig, EntityStates.Mage.Weapon.BaseThrowBombState self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody() && (self is EntityStates.Mage.Weapon.ThrowNovabomb || self is EntityStates.Mage.Weapon.ThrowIcebomb))
            {
                GetHandByDominance(false).animator.SetTrigger("Cast");
            }
        }

        private static void StopCastAnimation(On.EntityStates.Mage.Weapon.Flamethrower.orig_OnExit orig, EntityStates.Mage.Weapon.Flamethrower self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.ResetTrigger("Cast");
                GetHandByDominance(true).animator.SetBool("HoldCast", false);
            }
        }

        private static void AnimateBlueprintRelease(On.EntityStates.Engi.EngiWeapon.PlaceTurret.orig_OnExit orig, EntityStates.Engi.EngiWeapon.PlaceTurret self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Release");
            }
        }

        private static void AnimateHarpoonRelease(On.EntityStates.Engi.EngiMissilePainter.Paint.orig_OnExit orig, EntityStates.Engi.EngiMissilePainter.Paint self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Release");
            }
        }

        private static void AnimateGrenadeRelease(On.EntityStates.Engi.EngiWeapon.ChargeGrenades.orig_OnExit orig, EntityStates.Engi.EngiWeapon.ChargeGrenades self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Release");
            }
        }

        private static void ForceBuzzsawMuzzle(On.EntityStates.Toolbot.BaseToolbotPrimarySkillState.orig_OnEnter orig, EntityStates.Toolbot.BaseToolbotPrimarySkillState self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                if (self.baseMuzzleName == "MuzzleBuzzsaw" && self.isInDualWield)
                {
                    self.muzzleName = self.baseMuzzleName;

                    self.muzzleTransform = GetHandByDominance(self.activatorSkillSlot == self.skillLocator.primary).GetMuzzleByIndex(1);
                }
            }
        }

        private static void AnimateDualWieldEnd(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnExit orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetFloat("RetoolSpeed", 4f);
                RoR2Application.instance.StartCoroutine(ExtendAfterDelay());
            }
        }

        private static IEnumerator ExtendAfterDelay()
        {
            yield return new WaitForSeconds(0.25f);
            GetHandByDominance(false).animator.SetInteger("ToolID", 0);
            GetHandByDominance(false).animator.SetTrigger("RetoolExtend");
        }

        private static void AnimateDualWieldExtend(On.EntityStates.Toolbot.ToolbotDualWieldStart.orig_FixedUpdate orig, EntityStates.Toolbot.ToolbotDualWieldStart self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                if (halfTime.hasPassed && isSwapping)
                {
                    isSwapping = false;

                    GenericSkill currentSkill = self.primary2Slot;

                    if (currentSkill)
                    {
                        GetHandByDominance(false).animator.SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                        GetHandByDominance(false).animator.SetTrigger("RetoolExtend");
                    }
                }
            }
        }

        private static void AnimateDualWieldRetract(On.EntityStates.Toolbot.ToolbotDualWieldStart.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWieldStart self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                isSwapping = true;
                halfTime = Run.FixedTimeStamp.now + (self.duration / 2);
                GetHandByDominance(false).animator.SetFloat("RetoolSpeed", 2f / self.duration);
                GetHandByDominance(false).animator.SetTrigger("RetoolRetract");
            }
        }

        private static void AnimateSawSlowdown(On.EntityStates.Toolbot.FireBuzzsaw.orig_OnExit orig, EntityStates.Toolbot.FireBuzzsaw self)
        {
            orig(self);

            CharacterBody body = self.characterBody;
            if (body.IsLocalBody())
            {
                GetHandByDominance(body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary).animator.SetTrigger("BuzzsawSlowdown");
            }
        }

        private static void AnimateGrenadeThrow(On.EntityStates.Toolbot.AimStunDrone.orig_OnExit orig, EntityStates.Toolbot.AimStunDrone self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.ResetTrigger("AimGrenade");
                GetHandByDominance(false).animator.SetTrigger("ThrowGrenade");
            }
        }

        private static void AnimateChargeSpear(On.EntityStates.Toolbot.CooldownSpear.orig_OnEnter orig, EntityStates.Toolbot.CooldownSpear self)
        {
            orig(self);

            CharacterBody body = self.characterBody;
            if (body.IsLocalBody())
            {
                bool dominant = body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary;
                GetHandByDominance(dominant).animator.SetFloat("SpearChargeSpeed", 1f / self.duration);
                GetHandByDominance(dominant).animator.SetTrigger("SpearCharge");
            }
        }

        private static void SetInitialTool(CharacterBody body)
        {
            if (body.name == "ToolbotBody(Clone)")
            {
                GenericSkill currentSkill = body.skillLocator.primary;

                if (currentSkill)
                {
                    GetHandByDominance(true).animator.SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                }
            }
        }

        private static void AnimateRetoolExtend(On.EntityStates.Toolbot.ToolbotStanceSwap.orig_FixedUpdate orig, EntityStates.Toolbot.ToolbotStanceSwap self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                if (halfTime.hasPassed && !isSwapping)
                {
                    isSwapping = true;

                    GenericSkill currentSkill = self.previousStanceState == typeof(EntityStates.Toolbot.ToolbotStanceA) ? self.GetPrimarySkill2() : self.GetPrimarySkill1();

                    if (currentSkill)
                    {
                        GetHandByDominance(true).animator.SetInteger("ToolID", Array.IndexOf(multPrimarySkills, currentSkill.skillDef.skillName));
                        GetHandByDominance(true).animator.SetTrigger("RetoolExtend");
                    }
                }
            }
        }

        private static void AnimateRetoolRetract(On.EntityStates.Toolbot.ToolbotStanceSwap.orig_OnEnter orig, EntityStates.Toolbot.ToolbotStanceSwap self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                isSwapping = false;
                halfTime = Run.FixedTimeStamp.now + (self.baseDuration / (self.attackSpeedStat * 2));
                GetHandByDominance(true).animator.SetFloat("RetoolSpeed", 2f / (self.baseDuration / self.attackSpeedStat));
                GetHandByDominance(true).animator.SetTrigger("RetoolRetract");
            }
        }

        private static void AnimateSpinDown(On.EntityStates.Toolbot.FireNailgun.orig_OnExit orig, EntityStates.Toolbot.FireNailgun self)
        {
            orig(self);

            CharacterBody body = self.characterBody;
            if (body.IsLocalBody())
            {
                GetHandByDominance(body.skillLocator.FindSkillSlot(self.activatorSkillSlot) == SkillSlot.Primary).animator.SetTrigger("NailgunSlowdown");
            }
        }

        private static void DeleteEffect(On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.orig_OnEnter orig, EntityStates.Huntress.HuntressWeapon.ThrowGlaive self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody() && self.chargeEffect)
            {
                EntityStates.EntityState.Destroy(self.chargeEffect);
            }
        }

        private static void AnimateGlaiveThrow(On.EntityStates.Huntress.HuntressWeapon.ThrowGlaive.orig_FireOrbGlaive orig, EntityStates.Huntress.HuntressWeapon.ThrowGlaive self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody() && self.hasSuccessfullyThrownGlaive)
            {
                GetHandByDominance(false).animator.SetTrigger("ThrowGlaive");
            }
        }

        private static void PlayRevolverSpinAnimation(On.EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.orig_OnEnter orig, EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetFloat("SpinSpeed", 1f / self.duration);
                GetHandByDominance(true).animator.SetTrigger("RevolverSpin");
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
                return self.characterBody.IsLocalBody() ? "CurrentDominantMuzzle" : "Head";
            });
        }

        private static void ChangeTazerMuzzleShoot(On.EntityStates.Captain.Weapon.FireTazer.orig_Fire orig, EntityStates.Captain.Weapon.FireTazer self)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetTrigger("Rest");
                string muzzleString = EntityStates.Captain.Weapon.FireTazer.targetMuzzle;
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = "CurrentNonDominantMuzzle";
                orig(self);
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = muzzleString;
                return;
            }

            orig(self);
        }

        private static void ChangeTazerMuzzleEnter(On.EntityStates.Captain.Weapon.FireTazer.orig_OnEnter orig, EntityStates.Captain.Weapon.FireTazer self)
        {
            if (self.characterBody.IsLocalBody())
            {
                string muzzleString = EntityStates.Captain.Weapon.FireTazer.targetMuzzle;
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = "CurrentNonDominantMuzzle";
                orig(self);
                EntityStates.Captain.Weapon.FireTazer.targetMuzzle = muzzleString;
                return;
            }

            orig(self);
        }

        private static void ChangeBiteDirection(On.EntityStates.Croco.Bite.orig_OnEnter orig, EntityStates.Croco.Bite self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            self.swingEffectMuzzleString = "MuzzleHandL";
        }

        private static void ForceDashDirection(On.EntityStates.Merc.Assaulter2.orig_OnEnter orig, EntityStates.Merc.Assaulter2 self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            self.dashVector = GetHandByDominance(false).muzzle.forward;
        }

        private static void ForceFocusedDashDirection(On.EntityStates.Merc.FocusedAssaultDash.orig_OnEnter orig, EntityStates.Merc.FocusedAssaultDash self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            self.dashVector = GetHandByDominance(false).muzzle.forward;
        }

        private static void ForceWhirlwindDirection(On.EntityStates.Merc.WhirlwindBase.orig_FixedUpdate orig, EntityStates.Merc.WhirlwindBase self)
        {
            if (self.characterBody.IsLocalBody())
            {
                if (self.characterDirection)
                    self.characterDirection.forward = abilityDirection;
            }

            orig(self);
        }

        private static void ChangeSlashDirection(On.EntityStates.Bandit2.Weapon.SlashBlade.orig_OnEnter orig, EntityStates.Bandit2.Weapon.SlashBlade self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            abilityDirection = GetHandByDominance(false).muzzle.forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeUppercutDirection(On.EntityStates.Merc.Uppercut.orig_OnEnter orig, EntityStates.Merc.Uppercut self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            abilityDirection = GetHandByDominance(false).muzzle.forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void ChangeWhirlwindDirection(On.EntityStates.Merc.WhirlwindBase.orig_OnEnter orig, EntityStates.Merc.WhirlwindBase self)
        {
            orig(self);

            if (!self.characterBody.IsLocalBody()) return;

            abilityDirection = GetHandByDominance(false).muzzle.forward;

            if (self.characterDirection)
                self.characterDirection.forward = abilityDirection;
        }

        private static void SetFireboltMuzzle(On.EntityStates.Mage.Weapon.FireFireBolt.orig_FireGauntlet orig, EntityStates.Mage.Weapon.FireFireBolt self)
        {
            if (self.characterBody.IsLocalBody())
                self.muzzleString = "MuzzleRight";

            orig(self);
        }

        private static void RemoveMuzzleFlash(On.EntityStates.Engi.EngiWeapon.FireGrenades.orig_FireGrenade orig, EntityStates.Engi.EngiWeapon.FireGrenades self, string targetMuzzle)
        {
            if (self.characterBody.IsLocalBody())
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
            if (self.characterBody.IsLocalBody())
                return GetHandBySide(true).aimRay;
            else
                return orig(self);
        }

        private static Ray CancelModifyIfLocal(On.RoR2.CameraRigController.orig_ModifyAimRayIfApplicable orig, Ray originalAimRay, GameObject target, out float extraRaycastDistance)
        {
            CharacterBody targetBody = target.GetComponent<CharacterBody>();
            if (targetBody && targetBody.IsLocalBody())
            {
                extraRaycastDistance = 0;

                StackTrace stackTrace = new StackTrace();
                Type callerType = stackTrace.GetFrame(2).GetMethod().DeclaringType;

                bool useLeftHand;
                if (forceAimRaySideTypes.TryGetValue(callerType, out useLeftHand))
                {
                    return GetHandBySide(useLeftHand).aimRay;
                }

                return originalAimRay;
            }

            return orig(originalAimRay, target, out extraRaycastDistance);
        }

        private static void PingFromHand(On.RoR2.PlayerCharacterMasterController.orig_CheckPinging orig, PlayerCharacterMasterController self)
        {
            if (!self.body.IsLocalBody())
            {
                orig(self);
                return;
            }

            if (self.hasEffectiveAuthority && self.body && self.bodyInputs && self.bodyInputs.ping.justPressed)
            {
                self.pingerController.AttemptPing(GetHandBySide(false).aimRay, self.body.gameObject);
            }
        }

        private static void SetNailgunMuzzle(On.EntityStates.Toolbot.BaseNailgunState.orig_FireBullet orig, EntityStates.Toolbot.BaseNailgunState self, Ray aimRay, int bulletCount, float spreadPitchScale, float spreadYawScale)
        {
            if (!self.characterBody.IsLocalBody()) orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);

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
                return self.characterBody.IsLocalBody() ? "CurrentNonDominantMuzzle" : "MuzzleNailgun";
            });
        }

        private static void SetRebarMuzzle(On.EntityStates.Toolbot.FireSpear.orig_FireBullet orig, EntityStates.Toolbot.FireSpear self, Ray aimRay)
        {
            if (self.characterBody.IsLocalBody())
            {
                ((EntityStates.GenericBulletBaseState)self).muzzleName = ((EntityStates.Toolbot.IToolbotPrimarySkillState)self).muzzleName;
            }
            orig(self, aimRay);
        }

        private static void SetScrapMuzzle(On.EntityStates.Toolbot.FireGrenadeLauncher.orig_OnEnter orig, EntityStates.Toolbot.FireGrenadeLauncher self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                self.targetMuzzle = (self as EntityStates.Toolbot.IToolbotPrimarySkillState).muzzleName;
            }
        }

        private static Vector3 GetNonDominantVector(On.EntityStates.Huntress.BlinkState.orig_GetBlinkVector orig, EntityStates.Huntress.BlinkState self)
        {
            if (self.characterBody.IsLocalBody())
            {
                return GetHandByDominance(false).muzzle.forward;
            }
            return orig(self);
        }

        private static Ray EditAimray(On.EntityStates.BaseState.orig_GetAimRay orig, EntityStates.BaseState self)
        {
            if (self.characterBody.IsLocalBody())
            {
                bool isMULT = self.characterBody.name.Contains("ToolbotBody");

                if (nonDominantHandStateTypes.Contains(self.GetType()) || (isMULT && self.outer.customName == "Weapon2"))
                    return GetHandByDominance(false).aimRay;

                if (isMULT && self is EntityStates.GenericProjectileBaseState)
                {
                    EntityStates.GenericProjectileBaseState projSelf = (EntityStates.GenericProjectileBaseState)self;

                    if (isMULT && projSelf.targetMuzzle == "DualWieldMuzzleR")
                        return GetHandByDominance(false).aimRay;
                }
            }
            return orig(self);
        }

        private static void HideArrowCluster(On.EntityStates.Huntress.BaseArrowBarrage.orig_OnExit orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetBool("ShowCluster", false);
            }
            orig(self);
        }

        private static void AnimateSnipeBow(On.EntityStates.Huntress.AimArrowSnipe.orig_HandlePrimaryAttack orig, EntityStates.Huntress.AimArrowSnipe self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Primary");

                if (self.primarySkillSlot.stock > 0)
                    GetHandByDominance(true).animator.SetTrigger("Pull");
            }
        }

        private static void AnimateBarrageBowShoot(On.EntityStates.Huntress.BaseArrowBarrage.orig_HandlePrimaryAttack orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            orig(self);
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Primary");
            }
        }

        private static void AnimateBarrageBowPull(On.EntityStates.Huntress.BaseArrowBarrage.orig_OnEnter orig, EntityStates.Huntress.BaseArrowBarrage self)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetBool("ShowCluster", true);
                GetHandByDominance(true).animator.SetTrigger("Pull");
            }
            orig(self);
        }

        private static void PreventBowPull(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnExit orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            if (!self.characterBody.IsLocalBody()) orig(self);

            preventBowPull = true;
            orig(self);
            preventBowPull = false;
        }

        private static void AnimatePrimaryBowShoot(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_FireOrbArrow orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            int firedCount = self.firedArrowCount;
            orig(self);
            if (self.firedArrowCount <= firedCount) return;

            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Primary");

                if (!preventBowPull && self.firedArrowCount < self.maxArrowCount)
                    GetHandByDominance(true).animator.SetTrigger("Pull");
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
            c.Emit(OpCodes.Stloc_S, (byte)14);

            for (int i = 0; i < 4; i++)
            {
                c.GotoNext(x => x.MatchCallvirt(typeof(Player), "GetButton"));

                c.Remove();
                c.EmitDelegate<Func<Player, int, bool>>((player, button) =>
                {
                    return player.GetButton(button) || MeleeSkill.skillStates[button - 7];
                }
                );
            }
        }

        private static void PlayFMJShootAnimation(On.EntityStates.Commando.CommandoWeapon.FireFMJ.orig_PlayAnimation orig, EntityStates.Commando.CommandoWeapon.FireFMJ self, float duration)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(false).animator.SetTrigger("Primary");
            }
            orig(self, duration);
        }

        private static void PlayBarrageShootAnimation(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, EntityStates.Commando.CommandoWeapon.FireBarrage self)
        {
            if (self.characterBody.IsLocalBody())
            {
                GetHandByDominance(true).animator.SetTrigger("Primary");
            }
            orig(self);
        }

        private static void DestroyOffHandEffect(On.EntityStates.Mage.Weapon.Flamethrower.orig_FireGauntlet orig, EntityStates.Mage.Weapon.Flamethrower self, string muzzleString)
        {
            orig(self, muzzleString);

            if (!self.characterBody.IsLocalBody()) return;

            if (self.leftFlamethrowerTransform)
                GameObject.Destroy(self.leftFlamethrowerTransform.gameObject);

            GetHandByDominance(true).animator.SetBool("HoldCast", true);
            GetHandByDominance(true).animator.SetTrigger("Cast");
        }
        private static void SetFMJMuzzle(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            if (self.characterBody.IsLocalBody())
            {
                if (self is EntityStates.Commando.CommandoWeapon.FireFMJ)
                    self.targetMuzzle = "MuzzleLeft";
            }
            orig(self);
        }

        private static BulletAttack ChangeShotgunMuzzle(On.EntityStates.GenericBulletBaseState.orig_GenerateBulletAttack orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            if (self.characterBody.IsLocalBody() && self is EntityStates.Commando.CommandoWeapon.FireShotgunBlast)
            {
                if (!ModConfig.CommandoDualWield.Value)
                {
                    self.muzzleName = "MuzzleLeft";
                }

                if (self.muzzleName == "MuzzleLeft")
                {
                    aimRay = GetHandByDominance(false).aimRay;
                }

                Animator animator = GetHandByDominance(!self.muzzleName.Contains("Left") && ModConfig.CommandoDualWield.Value).animator;

                if (animator)
                    animator.SetTrigger("Primary");
            }

            return orig(self, aimRay);
        }

        private static void CheckPistolBulletMuzzle(On.EntityStates.Commando.CommandoWeapon.FirePistol2.orig_FireBullet orig, EntityStates.Commando.CommandoWeapon.FirePistol2 self, string targetMuzzle)
        {
            if (self.characterBody.IsLocalBody())
            {
                if (!ModConfig.CommandoDualWield.Value)
                    targetMuzzle = "MuzzleRight";

                if (targetMuzzle.Contains("Left"))
                {
                    self.aimRay = GetHandByDominance(false).aimRay;

                    Animator animator = GetHandByDominance(false).animator;

                    if (animator)
                        animator.SetTrigger("Primary");
                }
                else
                {
                    Animator animator = GetHandByDominance(true).animator;

                    if (animator)
                        animator.SetTrigger("Primary");
                }
            }

            orig(self, targetMuzzle);
        }
    }
}
