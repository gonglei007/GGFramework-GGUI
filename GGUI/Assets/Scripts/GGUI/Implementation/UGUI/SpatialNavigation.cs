using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI.UGUI
{
    public static class SpatialNavigation
    {
        /// <summary>Find the lowest-scoring target ahead of the origin. Ties keep enumeration order.</summary>
        public static T Find<T>(IEnumerable<T> targets, Vector2 origin, Vector2 direction,
            Func<T, Vector2> point, float minimumForward = 1, double lateralWeight = 2,
            double angularWeight = 1) where T : class
        {
            if (direction == Vector2.zero) return null;
            T best = null;
            double minimum = double.PositiveInfinity;
            foreach (var target in targets)
            {
                var offset = point(target) - origin;
                double forward = Vector2.Dot(offset, direction);
                if (forward <= minimumForward) continue;
                double lateral = Math.Abs(offset.x * direction.y - offset.y * direction.x);
                double distance = Math.Sqrt((double)offset.x * offset.x + (double)offset.y * offset.y);
                double score = distance + lateral * lateralWeight + lateral * lateral / forward * angularWeight;
                if (score >= minimum) continue;
                minimum = score;
                best = target;
            }
            return best;
        }
    }
}
