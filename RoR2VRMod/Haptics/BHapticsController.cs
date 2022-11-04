using UnityEngine;

namespace VRMod.Haptics
{
    class BHapticsController : GenericHapticsController
    {
        internal BHapticsController()
        {
            // Initialize the suit here
        }

        internal override bool Initialized()
        {
            // Whether the suit has initialized successfully or not
            return false;
        }

        internal override void Update(bool isHealthLow)
        {
            // Handle low health stuff here, like adding a hearth beat effect.
        }

        internal override void OnHealthSpentOnInteractable(float healthPercent)
        {
            // Send pulse on hearth
        }

        internal override void OnFallDamageTaken(float healthPercent)
        {
            // Send pulse on feet if possible. Otherwise, not sure. Either ignore or send a pulse on the hips.
        }

        internal override void OnDirectionalDamageTaken(Vector3 localDirection, float healthPercent)
        {
            // Send pulse on the appropriate area depending on the incoming direction of damage. Maybe include the face as well?
            // The direction starts from the attack position towards the center of the player's body.
            // For example, taking damage from the front would give (0, 0, -1). You can refer to the Shockwave code for help.
        }

        internal override void OnEnvironmentDamageTaken(float healthPercent)
        {
            // Send pulse on hearth
        }

        internal override void OnDeath()
        {
            // On the Shockwave suit, I send haptics on the whole body going from the shoulders all the way to the legs, but you're welcome to be creative in other ways :)
        }
    }
}
