using System;
using System.Collections;
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
        [SerializeField, Min(0.1f)] private float buildingClearanceRadius = 5f;
        [SerializeField, Min(1)] private int attemptsPerResource = 50;
        [SerializeField, Min(1f)] private float regenerationInterval = 60f;

        private static readonly Collider[] OverlapResults = new Collider[16];
        private int[] _targetCounts;
        private Vector3 _spawnOrigin;
        private int _buildingMask;
        private int _resourceMask;

        private void Start()
        {
            GameObject startingKeep = GameObject.Find("Keep");
            _spawnOrigin = startingKeep != null ? startingKeep.transform.position : transform.position;
            _buildingMask = LayerMask.GetMask("Building");
            _resourceMask = LayerMask.GetMask("Supplies");

            if (resources == null) return;

            _targetCounts = new int[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                ResourceSpawnEntry entry = resources[i];
                if (entry == null || entry.supply == null || entry.supply.ResourcePrefab == null) continue;

                int minCount = Mathf.Max(0, entry.minCount);
                int maxCount = Mathf.Max(minCount, entry.maxCount);
                int count = UnityEngine.Random.Range(minCount, maxCount + 1);
                int currentCount = CountResources(entry.supply);
                _targetCounts[i] = currentCount + count;
                int spawned = 0;

                for (int j = 0; j < count; j++)
                {
                    if (TrySpawnResource(entry.supply.ResourcePrefab))
                        spawned++;
                }

                if (spawned < count)
                    Debug.LogWarning($"{name}: {entry.supply.name} 자원 {count}개 중 {spawned}개만 생성했습니다. 주변 배치 공간이나 NavMesh를 확인하세요.", this);
            }

            StartCoroutine(RegenerateResources());
        }

        private IEnumerator RegenerateResources()
        {
            while (true)
            {
                yield return new WaitForSeconds(regenerationInterval);

                for (int i = 0; i < resources.Length; i++)
                {
                    ResourceSpawnEntry entry = resources[i];
                    if (entry == null || entry.supply == null || entry.supply.ResourcePrefab == null) continue;

                    int missingCount = _targetCounts[i] - CountResources(entry.supply);
                    for (int j = 0; j < missingCount; j++)
                    {
                        if (!TrySpawnResource(entry.supply.ResourcePrefab)) break;
                    }
                }
            }
        }

        private int CountResources(SupplySO supply)
        {
            int count = 0;
            GatherableSupply[] nodes = FindObjectsByType<GatherableSupply>(FindObjectsSortMode.None);
            foreach (GatherableSupply node in nodes)
            {
                if (node != null && node.SupplyData == supply)
                    count++;
            }

            return count;
        }

        private bool TrySpawnResource(GameObject prefab)
        {
            for (int attempt = 0; attempt < attemptsPerResource; attempt++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized
                                 * UnityEngine.Random.Range(minSpawnRadius, Mathf.Max(minSpawnRadius, maxSpawnRadius));
                Vector3 candidate = _spawnOrigin + new Vector3(offset.x, 0f, offset.y);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                Vector3 checkPosition = navHit.position + Vector3.up * 0.6f;
                int buildingCount = Physics.OverlapSphereNonAlloc(checkPosition, buildingClearanceRadius,
                    OverlapResults, _buildingMask, QueryTriggerInteraction.Ignore);
                if (buildingCount > 0) continue;

                int resourceCount = Physics.OverlapSphereNonAlloc(checkPosition, placementCheckRadius,
                    OverlapResults, _resourceMask, QueryTriggerInteraction.Ignore);
                if (resourceCount > 0) continue;

                Instantiate(prefab, navHit.position,
                    Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), transform);
                return true;
            }

            return false;
        }
    }
}
