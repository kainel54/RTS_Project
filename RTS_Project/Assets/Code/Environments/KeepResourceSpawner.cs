using System;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Environments
{
    public class KeepResourceSpawner : MonoBehaviour
    {
        [Serializable]
        private class ResourceSpawnEntry
        {
            public SupplySO supply;
            [Min(0)] public int minCount = 5;
            [Min(0)] public int maxCount = 8;
        }

        [SerializeField] private ResourceSpawnEntry[] resources;
        [SerializeField, Min(0f)] private float minSpawnRadius = 15f;
        [SerializeField, Min(1f)] private float maxSpawnRadius = 40f;
        [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;
        [SerializeField, Min(0.1f)] private float placementCheckRadius = 3f;
        [SerializeField, Min(1)] private int attemptsPerResource = 50;

        private static readonly Collider[] OverlapResults = new Collider[16];

        private void Start()
        {
            GameObject startingKeep = GameObject.Find("Keep");
            Vector3 spawnOrigin = startingKeep != null ? startingKeep.transform.position : transform.position;

            if (resources == null) return;

            foreach (ResourceSpawnEntry entry in resources)
            {
                if (entry == null || entry.supply == null || entry.supply.ResourcePrefab == null) continue;

                int minCount = Mathf.Max(0, entry.minCount);
                int maxCount = Mathf.Max(minCount, entry.maxCount);
                int count = UnityEngine.Random.Range(minCount, maxCount + 1);
                int spawned = 0;

                for (int i = 0; i < count; i++)
                {
                    if (TrySpawnResource(entry.supply.ResourcePrefab, spawnOrigin))
                        spawned++;
                }

                if (spawned < count)
                    Debug.LogWarning($"{name}: {entry.supply.name} 자원 {count}개 중 {spawned}개만 생성했습니다. 주변 배치 공간이나 NavMesh를 확인하세요.", this);
            }
        }

        private bool TrySpawnResource(GameObject prefab, Vector3 spawnOrigin)
        {
            int occupiedMask = LayerMask.GetMask("Building", "Supplies");
            for (int attempt = 0; attempt < attemptsPerResource; attempt++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized
                                 * UnityEngine.Random.Range(minSpawnRadius, Mathf.Max(minSpawnRadius, maxSpawnRadius));
                Vector3 candidate = spawnOrigin + new Vector3(offset.x, 0f, offset.y);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                int overlapCount = Physics.OverlapSphereNonAlloc(navHit.position + Vector3.up * 0.6f,
                    placementCheckRadius, OverlapResults, occupiedMask, QueryTriggerInteraction.Ignore);
                if (overlapCount > 0) continue;

                Instantiate(prefab, navHit.position,
                    Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), transform);
                return true;
            }

            return false;
        }
    }
}
