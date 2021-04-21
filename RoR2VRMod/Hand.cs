using UnityEngine;

namespace VRMod
{
    public class Hand : MonoBehaviour
    {
        public Transform aimOrigin;

        public HandType handType;
    }

    public enum HandType
    {
        Pointer,
        Commando,
        HuntressBow,
        HuntressHand,
        BanditRifle,
        BanditRevolver,
        MULTNail,
        MULTRebar,
        MULTScrap,
        MULTSaw,
        MULTHand,
        Engi,
        Arti,
        MercSword,
        MercHand,
        Rex,
        Loader,
        Acrid,
        CaptainShotgun,
        CaptainHand
    }
}
