using RoR2;
using RoR2.Audio;
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

        private static bool scaleDownNextEffect = false;
        internal static void Init()
        {
            On.EntityStates.Commando.CommandoWeapon.FirePistol2.FireBullet += CheckPistolBulletMuzzle;

            On.EntityStates.GenericBulletBaseState.GenerateBulletAttack += CheckGenericBulletMuzzle;

            On.EntityStates.GenericProjectileBaseState.FireProjectile += FireProjectileOverride;

            On.RoR2.EffectManager.SpawnEffect_EffectIndex_EffectData_bool += SpawnEffectOverride;

            On.RoR2.EffectManager.SimpleMuzzleFlash += ReduceMuzzleFlash;
        }

        private static void ReduceMuzzleFlash(On.RoR2.EffectManager.orig_SimpleMuzzleFlash orig, GameObject effectPrefab, GameObject obj, string muzzleName, bool transmit)
        {
            if (IsLocalPlayer(obj.GetComponent<CharacterBody>()))
            {
                scaleDownNextEffect = true;
                orig(effectPrefab, obj, muzzleName, transmit);
                //scaleDownNextEffect = false;
                return;
            }
            orig(effectPrefab, obj, muzzleName, transmit);
        }

        private static void SpawnEffectOverride(On.RoR2.EffectManager.orig_SpawnEffect_EffectIndex_EffectData_bool orig, EffectIndex effectIndex, EffectData effectData, bool transmit)
        {
            if (transmit)
            {
                EffectManager.TransmitEffect(effectIndex, effectData, null);
                if (NetworkServer.active)
                {
                    return;
                }
            }
            if (NetworkClient.active)
            {
                if (effectData.networkSoundEventIndex != NetworkSoundEventIndex.Invalid)
                {
                    PointSoundManager.EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(effectData.networkSoundEventIndex), effectData.origin);
                }
                EffectDef effectDef = EffectCatalog.GetEffectDef(effectIndex);
                if (effectDef == null)
                {
                    return;
                }
                string spawnSoundEventName = effectDef.spawnSoundEventName;
                if (!string.IsNullOrEmpty(spawnSoundEventName))
                {
                    PointSoundManager.EmitSoundLocal((AkEventIdArg)spawnSoundEventName, effectData.origin);
                }
                SurfaceDef surfaceDef = SurfaceDefCatalog.GetSurfaceDef(effectData.surfaceDefIndex);
                if (surfaceDef != null)
                {
                    string impactSoundString = surfaceDef.impactSoundString;
                    if (!string.IsNullOrEmpty(impactSoundString))
                    {
                        PointSoundManager.EmitSoundLocal((AkEventIdArg)impactSoundString, effectData.origin);
                    }
                }
                if (!VFXBudget.CanAffordSpawn(effectDef.prefabVfxAttributes))
                {
                    return;
                }
                if (effectDef.cullMethod != null && !effectDef.cullMethod(effectData))
                {
                    return;
                }
                scaleDownNextEffect = true;
                if (scaleDownNextEffect)
                {
                    effectData.scale = 0.5f;
                }
                EffectData effectData2 = effectData.Clone();
                GameObject effectInstance = UnityEngine.Object.Instantiate<GameObject>(effectDef.prefab, effectData2.origin, effectData2.rotation);
                if (effectInstance)
                {
                    EffectComponent component = effectInstance.GetComponent<EffectComponent>();
                    if (component)
                    {
                        if (scaleDownNextEffect)
                        {
                            VRMod.StaticLogger.LogInfo("PEW");
                            component.applyScale = true;
                        }
                        component.effectData = effectData2.Clone();
                    }
                }
            }
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
                Ray aimRay = self.targetMuzzle.Contains("Left") ? GetHandRay(HandSide.NonDominant) : self.GetAimRay();
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
                    aimRay = GetHandRay(HandSide.NonDominant);
            }

            return orig(self, aimRay);
        }

        private static void CheckPistolBulletMuzzle(On.EntityStates.Commando.CommandoWeapon.FirePistol2.orig_FireBullet orig, EntityStates.Commando.CommandoWeapon.FirePistol2 self, string targetMuzzle)
        {
            if (IsLocalPlayer(self.outer.GetComponent<CharacterBody>()))
            {
                if (targetMuzzle.Contains("Left"))
                    self.aimRay = GetHandRay(HandSide.NonDominant);
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
