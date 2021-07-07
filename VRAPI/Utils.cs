using RoR2;

namespace VRAPI
{
    public static class Utils
    {
        private static CharacterMaster _cachedMaster;

        private static CharacterMaster localCharacterMaster
        {
            get
            { 
                if (!_cachedMaster)
                {
                    _cachedMaster = LocalUserManager.GetFirstLocalUser().cachedMaster;
                }
                return _cachedMaster;
            }
        }

        /// <summary>
        /// Returns true if the state's owner is playing with the VR mod.
        /// </summary>
        /// <param name="state">The entity state that contains the owner.</param>
        /// <returns></returns>
        public static bool IsInVR(this EntityStates.EntityState state)
        {
            return state.characterBody.master.IsInVR();
        }

        /// <summary>
        /// Returns true if the player controlling the character is playing with the VR mod.
        /// </summary>
        /// <param name="body">The player's character body.</param>
        /// <returns></returns>
        public static bool IsInVR(this CharacterBody body)
        {
            return body.master.IsInVR();
        }

        /// <summary>
        /// Returns true if the player controlling the character is playing with the VR mod.
        /// </summary>
        /// <param name="master">The player's character master.</param>
        /// <returns></returns>
        public static bool IsInVR(this CharacterMaster master)
        {
            return VR.enabled && master == localCharacterMaster;
        }

        /// <summary>
        /// Returns true if the state's owner is playing in VR with motion controls.
        /// </summary>
        /// <param name="state">The entity state that contains the owner.</param>
        /// <returns></returns>
        public static bool IsUsingMotionControls(this EntityStates.EntityState state)
        {
            return state.characterBody.master.IsInVR() && MotionControls.enabled;
        }

        /// <summary>
        /// Returns true if the player controlling the character in VR with motion controls.
        /// </summary>
        /// <param name="body">The player's character body.</param>
        /// <returns></returns>
        public static bool IsUsingMotionControls(this CharacterBody body)
        {
            return body.master.IsInVR() && MotionControls.enabled;
        }

        /// <summary>
        /// Returns true if the player controlling the character is playing in VR with motion controls.
        /// </summary>
        /// <param name="master">The player's character master.</param>
        /// <returns></returns>
        public static bool IsUsingMotionControls(this CharacterMaster master)
        {
            return master.IsInVR() && MotionControls.enabled;
        }
    }
}
