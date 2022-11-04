using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod.Haptics
{
    abstract class GenericHapticsController
    {
        internal abstract bool Initialized();

        internal abstract void Update(bool isHealthLow);

        internal abstract void OnFallDamageTaken(float healthPercent);

        internal abstract void OnHealthSpentOnInteractable(float healthPercent);

        internal abstract void OnDirectionalDamageTaken(Vector3 localDirection, float healthPercent);

        internal abstract void OnEnvironmentDamageTaken(float healthPercent);

        internal abstract void OnDeath();
    }
}
