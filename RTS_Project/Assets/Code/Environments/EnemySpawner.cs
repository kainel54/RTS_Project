using System.Collections;
using Code.Quests;
using Code.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Environments
{
    /// <summary>Spawns enemies on the NavMesh outside the Keep resource spawning ring.</summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private KeepResourceSpawner resourceSpawner;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField, Min(0)] private int initialEnemyCount = 1;
        [SerializeField, Min(1)] private int maxAliveEnemies = 5;
        [SerializeField, Min(0.1f)] private float waveInterval = 35f;
        [SerializeField, Min(1)] private int enemiesPerWave = 1;
        [SerializeField, Min(0f)] private float innerResourceBuffer = 8f;
        [SerializeField, Min(1f)] private float outerResourceBuffer = 25f;
        [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 4f;
        [SerializeField, Min(0.1f)] private float spawnClearanceRadius = 2f;
        [SerializeField, Min(1)] private int attemptsPerEnemy = 40;

        private static readonly Collider[] OverlapResults = new Collider[32];
        private Transform _keep;
        private int _blockingMask;
        private bool _spawningStarted;

        private void Awake()
        {
            if (resourceSpawner == null)
                resourceSpawner = GetComponent<KeepResourceSpawner>();
            if (questManager == null)
                questManager = FindObjectOfType<QuestManager>();
        }

        private void Start()
        {
            GameObject keepObject = GameObject.Find("Keep");
            _keep = keepObject != null ? keepObject.transform : null;
            _blockingMask = LayerMask.GetMask("Unit", "Building", "Supplies");

            if (_keep == null || resourceSpawner == null || questManager == null || enemyPrefab == null)
            {
                Debug.LogError($"{name}: Keep, 스포너, 퀘스트 매니저 또는 적 프리팹을 찾지 못해 적 생성을 시작할 수 없습니다.", this);
                return;
            }

            questManager.QuestSequenceCompleted += HandleQuestSequenceCompleted;
            if (questManager.IsCompleted)
                BeginSpawning();
            else
                Debug.Log($"{name}: 퀘스트 시퀀스 완료 후 적 스폰이 시작됩니다.", this);
        }

        private void OnDestroy()
        {
            if (questManager != null)
                questManager.QuestSequenceCompleted -= HandleQuestSequenceCompleted;
        }

        private void HandleQuestSequenceCompleted()
        {
            BeginSpawning();
        }

        private void BeginSpawning()
        {
            if (_spawningStarted) return;

            _spawningStarted = true;
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            SpawnEnemies(initialEnemyCount);

            while (true)
            {
                yield return new WaitForSeconds(waveInterval);
                SpawnEnemies(enemiesPerWave);
            }
        }

        private void SpawnEnemies(int count)
        {
            for (int i = 0; i < count && FindObjectsOfType<EnemyUnitAI>().Length < maxAliveEnemies; i++)
            {
                if (!TryGetSpawnPosition(out Vector3 spawnPosition))
                {
                    Debug.LogWarning($"{name}: 자원 스폰 영역 바깥에서 적을 생성할 NavMesh 위치를 찾지 못했습니다.", this);
                    return;
                }

                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                if (enemy.TryGetComponent(out EnemyUnitAI enemyAI))
                    enemyAI.SetKeepTarget(_keep);
            }
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            float resourceRadius = resourceSpawner.MaxSpawnRadius;
            float minRadius = resourceRadius + innerResourceBuffer;
            float maxRadius = resourceRadius + Mathf.Max(innerResourceBuffer, outerResourceBuffer);

            for (int attempt = 0; attempt < attemptsPerEnemy; attempt++)
            {
                Vector2 direction = Random.insideUnitCircle.normalized;
                float radius = Random.Range(minRadius, maxRadius);
                Vector3 candidate = _keep.position + new Vector3(direction.x * radius, 0f, direction.y * radius);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                Vector3 flatOffset = navHit.position - _keep.position;
                flatOffset.y = 0f;
                if (flatOffset.magnitude < minRadius) continue;

                Vector3 overlapPosition = navHit.position + Vector3.up * 0.6f;
                if (Physics.OverlapSphereNonAlloc(overlapPosition, spawnClearanceRadius,
                        OverlapResults, _blockingMask, QueryTriggerInteraction.Ignore) > 0)
                    continue;

                position = navHit.position;
                return true;
            }

            position = default;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            GameObject keepObject = _keep != null ? _keep.gameObject : GameObject.Find("Keep");
            if (keepObject == null) return;

            float resourceRadius = resourceSpawner != null ? resourceSpawner.MaxSpawnRadius : 40f;
            Gizmos.color = new Color(1f, 0.25f, 0.15f, 0.9f);
            Gizmos.DrawWireSphere(keepObject.transform.position, resourceRadius + innerResourceBuffer);
            Gizmos.DrawWireSphere(keepObject.transform.position,
                resourceRadius + Mathf.Max(innerResourceBuffer, outerResourceBuffer));
        }
    }
}
