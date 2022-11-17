using RoR2;
using UnityEngine;

namespace VRMod.Haptics
{
    class HapticsManager
    {
        private static GenericHapticsController hapticsController;

        private static Vector3 approximateChestRotation;

        internal static void Init()
        {
            if (ModConfig.HapticsSuit.Value == "Shockwave")
                hapticsController = new ShockwaveController();
            else if (ModConfig.HapticsSuit.Value == "Bhaptics")
                hapticsController = new BHapticsController();
            else
                return;

            if (!hapticsController.Initialized()) return;

            GlobalEventManager.onClientDamageNotified += OnClientDamage;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += CheckForHealthCost;
            RoR2Application.onUpdate += Update;
            On.RoR2.CharacterMaster.OnBodyDeath += OnDeath;
        }

        private static void OnDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            if (self == Utils.localMaster) hapticsController.OnDeath();

            orig(self, body);
        }

        private static void Update()
        {
            hapticsController.Update(Utils.localBody ? Utils.localBody.healthComponent.isHealthLow && Utils.localBody.healthComponent.alive : false);

            if (!Utils.localCameraRig) return;

            Transform camTransform = Utils.localCameraRig.sceneCam.transform;

            Vector3 flatHeadVector = Vector3.Cross(camTransform.right, Vector3.up);

            Vector3 flatLeftVector = MotionControls.GetHandBySide(true).transform.position - camTransform.position;
            flatLeftVector.y = 0;

            Vector3 flatRightVector = MotionControls.GetHandBySide(false).transform.position - camTransform.position;
            flatRightVector.y = 0;

            float leftAngle = Vector3.SignedAngle(flatHeadVector, flatLeftVector, Vector3.up);
            float rightAngle = Vector3.SignedAngle(flatHeadVector, flatRightVector, Vector3.up);

            Vector3 eulerOffset = new Vector3(0, (leftAngle + rightAngle) / 3, 0);

            Vector3 newLookRotation = Quaternion.Euler(eulerOffset) * flatHeadVector;

            approximateChestRotation = Quaternion.LookRotation(newLookRotation).eulerAngles;
        }

        private static void CheckForHealthCost(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!self.CanBeAffordedByInteractor(activator))
            {
                return;
            }
            if (self.costType == CostTypeIndex.PercentHealth)
                hapticsController.OnHealthSpentOnInteractable(self.cost / 100f);

            orig(self, activator);
        }

        private static void OnClientDamage(DamageDealtMessage dmgMessage)
        {
            if (!dmgMessage.victim) return;

            HealthComponent victimHealth = dmgMessage.victim.GetComponent<HealthComponent>();

            if (!victimHealth.body.IsLocalBody()) return;

            if ((dmgMessage.damageType & DamageType.FallDamage) > DamageType.Generic)
            {
                hapticsController.OnFallDamageTaken(dmgMessage.damage / victimHealth.fullHealth);
            }
            else if (dmgMessage.attacker == null)
            {
                hapticsController.OnEnvironmentDamageTaken(dmgMessage.damage / victimHealth.fullHealth);
            }
            else if (dmgMessage.attacker.GetComponent<PurchaseInteraction>() == null)
            {
                Vector3 worldDirection = (dmgMessage.victim.transform.TransformPoint(dmgMessage.victim.GetComponent<CapsuleCollider>().center) - dmgMessage.position).normalized;
                Vector3 localDirection = Quaternion.Euler(Quaternion.LookRotation(worldDirection).eulerAngles - approximateChestRotation) * Vector3.forward;
                hapticsController.OnDirectionalDamageTaken(localDirection, dmgMessage.damage / victimHealth.fullHealth);
            }
        }
    }
}
