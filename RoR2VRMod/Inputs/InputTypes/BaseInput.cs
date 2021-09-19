namespace VRMod.Inputs
{
    internal abstract class BaseInput
    {
        internal abstract bool IsBound { get;  }

        internal abstract string BindingString { get; }

        internal abstract void UpdateValues(Rewired.CustomController vrControllers);
    }
}
