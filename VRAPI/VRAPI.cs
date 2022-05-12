using BepInEx;
using BepInEx.Logging;

namespace VRAPI
{
    [BepInPlugin("com.DrBibop.VRAPI", "VRAPI", "1.1.0")]
    [BepInDependency("com.DrBibop.VRMod", BepInDependency.DependencyFlags.SoftDependency)]
    public class VRAPI : BaseUnityPlugin
    {
        internal static ManualLogSource StaticLogger;

        private void Awake()
        {
            StaticLogger = Logger;
        }
    }
}
