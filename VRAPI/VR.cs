using System;

namespace VRAPI
{
    public static class VR
    {
        private static bool? _enabled;

        /// <summary>
        /// Returns true if the user has the VR Mod installed.
        /// </summary>
        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DrBibop.VRMod");
                }
                return (bool)_enabled;
            }
        }

        /// <summary>
        /// Adds to the list of states that activates the confort vignette. Recommended for mobility skills.
        /// </summary>
        /// <param name="stateType">The state type that will use the vignette.</param>
        public static void AddVignetteState(Type stateType)
        {
            VRMod.ConfortVignette.AddVignetteState(stateType);
        }

        /// <summary>
		/// Prevents the mod from disabling the specified renderer in the body while in first person.
		/// </summary>
		/// <param name="bodyName">The name of the character body object.</param>
		/// <param name="rendererObjectName">The name of the renderer object.</param>
		public static void PreventRendererDisable(string bodyName, string rendererObjectName)
        {
            VRMod.CameraFixes.PreventRendererDisable(bodyName, rendererObjectName);
        }
    }
}
