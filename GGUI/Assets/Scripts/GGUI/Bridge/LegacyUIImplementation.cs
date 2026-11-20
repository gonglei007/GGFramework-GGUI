using UnityEngine;

namespace GGFramework.GGUI
{
    internal static class LegacyUIImplementation
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register() { UIImplementationResolver.LegacyDefault=()=>UISystem.Instance.Implementation; }
    }
}
