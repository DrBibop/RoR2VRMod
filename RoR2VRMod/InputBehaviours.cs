using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;

namespace VRMod
{
    internal static class InputBehaviours
    {
        private static GameObject vrManager;

        internal static void Init()
        {
            RoR2.RoR2Application.onLoad += AddRecenterInput;
        }

        private static void AddRecenterInput()
        {
            if (vrManager)
                return;
            
            vrManager = new GameObject("VRManager");

            GameObject vrInputs = new GameObject("VRInputs");
            vrInputs.transform.SetParent(vrManager.transform);
            vrInputs.AddComponent<MPEventSystemProvider>().fallBackToMainEventSystem = true;

            InputResponse inputResponse = vrInputs.AddComponent<InputResponse>();
            inputResponse.inputActionNames = new string[] { "RecenterHMD" };
            inputResponse.onPress = new UnityEvent();
            inputResponse.onPress.AddListener(UnityEngine.XR.InputTracking.Recenter);

            Object.DontDestroyOnLoad(vrManager);
        }
    }
}
