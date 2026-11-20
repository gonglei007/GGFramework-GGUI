using System;
using UnityEngine;

namespace GGFramework.GGUI
{
    public static class UIImplementationResolver
    {
        // Only the legacy project installs its singleton fallback. Hosted packages
        // require an explicit instance and do not create a hidden global UI system.
        public static Func<IUIImplementation> LegacyDefault;
        public static IUIImplementation Resolve(Component target)
        {
            var host=target.GetComponentInParent<UIHost>(true);
            var implementation=host?host.Implementation:LegacyDefault?.Invoke();
            return implementation ?? throw new InvalidOperationException("Configure a GGUI UIHost or legacy UI implementation before creating a tweener.");
        }
    }
}
