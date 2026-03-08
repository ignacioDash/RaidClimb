using System;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class UnitTargetController : MonoBehaviour
    {
        [SerializeField] private List<Transform> targets;

        private readonly HashSet<Transform> _usedTargets = new();

        private const float RADIUS = 0.5f;

        private void OnEnable()
        {
            var count = targets.Count;

            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;

                var offset = new Vector3(
                    Mathf.Cos(angle),
                    0f,
                    Mathf.Sin(angle)
                ) * RADIUS;

                targets[i].position = transform.position + offset;
            }
        }

        public Transform GetRandomTarget(Vector3 from)
        {
            Transform closestUnused = null;
            var closestUnusedDist = float.MaxValue;

            Transform closest = null;
            var closestDist = float.MaxValue;

            foreach (var t in targets)
            {
                if (!t) continue;

                var dist = (t.position - from).sqrMagnitude;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = t;
                }

                if (_usedTargets.Contains(t)) 
                    continue;

                if (dist < closestUnusedDist)
                {
                    closestUnusedDist = dist;
                    closestUnused = t;
                }
            }

            var chosen = closestUnused ? closestUnused : closest;

            if (chosen)
                _usedTargets.Add(chosen);

            return chosen;
        }
    }
}