using UnityEngine;

namespace VRMod
{
    internal class Muzzle : MonoBehaviour
    {
        [SerializeField]
        internal string[] entriesToReplaceIfDominant;

        [SerializeField]
        internal string[] entriesToReplaceIfNonDominant;
    }
}
