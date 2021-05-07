using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRMod
{
    internal class TwoHandedAimTarget : MonoBehaviour
    {
        [SerializeField]
        internal Transform guidingTransform;

        [SerializeField]
        internal GameObject objectToDisableWhenTwoHanded;
    }
}
