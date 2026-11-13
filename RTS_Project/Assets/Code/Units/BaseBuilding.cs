using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public const int MAX_QUEUE_SIZE = 5;
        [SerializeField, Min(1f)] private float maxHealth = 500f;
        public float CurrentHealth { get; private set; }
        private List<UnitSO> _buildingQueue = new List<UnitSO>(MAX_QUEUE_SIZE);
        private List<SupplySO> _supplyQueue = new List<SupplySO>(MAX_QUEUE_SIZE);
        private readonly List<Worker> _assignedWorkers = new List<Worker>();
        private Coroutine _supplyCoroutine;
        private Coroutine _workerProductionCoroutine;
        public int QueueSize => _buildingQueue.Count;
        public int SupplyQueueSize => _supplyQueue.Count;
        public UnitSO[] UnitQueue => _buildingQueue.ToArray();
        public SupplySO[] SupplyQueue => _supplyQueue.ToArray();
        public Worker[] AssignedWorkers => _assignedWorkers.ToArray();
        public int AssignedWorkerCount => _assignedWorkers.Count;
        public bool ConsumesAssignedWorkerFor(UnitSO unit) => unit != null
            && AbstractUnitData is BuildingSO { ConsumeAssignedWorkerPerUnit: true };
        public bool CanQueueUnit(UnitSO unit) => unit != null
            && (AbstractUnitData is not BuildingSO { LimitUnitQueueByAssignedWorkers: true }
                || _buildingQueue.Count < AssignedWorkerCount);
        public int WorkerCapacity => AbstractUnitData is BuildingSO buildingData ? buildingData.WorkerCapacity : 0;
        public bool HasAvailableWorkerSlot => AssignedWorkerCount < WorkerCapacity;
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO UnitSOBeingBuilt { get; private set; }
        [field: SerializeField] public SupplySO SupplySOBeingBuilt { get; private set; }

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public delegate void SupplyQueueUpdatedEvent(SupplySO[] suppliesInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        public event SupplyQueueUpdatedEvent OnSupplyQueueUpdated;
        public event System.Action<Worker[]> AssignedWorkersUpdated;

        protected override void Awake()
        {
            base.Awake();
            CurrentHealth = maxHealth;
        }

        protected override void Start()
        {
            base.Start();
            PopulationSystem.Instance?.RegisterBuilding(this);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || CurrentHealth <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            if (CurrentHealth <= 0f)
                Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            PopulationSystem.Instance?.UnregisterBuilding(this);
            foreach (Worker worker in _assignedWorkers.ToArray())
                worker?.SetAssignedBuilding(null);
            _assignedWorkers.Clear();
            base.OnDestroy();
        }

        public bool TryAssignWorker(Worker worker)
        {
            if (worker == null || WorkerCapacity <= 0) return false;

            if (worker.AssignedBuilding == this) return true;
            if (!HasAvailableWorkerSlot) return false;

            worker.AssignedBuilding?.RemoveWorker(worker);
            _assignedWorkers.Add(worker);
            worker.SetAssignedBuilding(this);
            AssignedWorkersUpdated?.Invoke(AssignedWorkers);
            Bus<WorkerAssignedEvent>.Raise(new WorkerAssignedEvent(AbstractUnitData as BuildingSO));
            UpdateWorkerProduction();
            return true;
        }

        public void RemoveWorker(Worker worker)
        {
            RemoveWorker(worker, true);
        }

        private void RemoveWorker(Worker worker, bool reactivateWorker)
        {
            if (worker == null || !_assignedWorkers.Remove(worker)) return;

            if (worker.AssignedBuilding == this)
                worker.SetAssignedBuilding(null, reactivateWorker);

            AssignedWorkersUpdated?.Invoke(AssignedWorkers);
            UpdateWorkerProduction();
        }

        private void UpdateWorkerProduction()
        {
            if (AbstractUnitData is not BuildingSO buildingData || buildingData.WorkerProductionSupply == null
                || _assignedWorkers.Count == 0)
            {
                if (_workerProductionCoroutine != null)
                {
                    StopCoroutine(_workerProductionCoroutine);
                    _workerProductionCoroutine = null;
                }

                return;
            }

            if (_workerProductionCoroutine == null)
                _workerProductionCoroutine = StartCoroutine(ProduceWithWorkers(buildingData));
        }

        private IEnumerator ProduceWithWorkers(BuildingSO buildingData)
        {
            while (_assignedWorkers.Count > 0 && buildingData.WorkerProductionSupply != null)
            {
                yield return new WaitForSeconds(buildingData.WorkerProductionInterval);

                if (_assignedWorkers.Count > 0)
                {
                    int amount = buildingData.WorkerProductionAmount * _assignedWorkers.Count;
                    Bus<SupplyEvent>.Raise(new SupplyEvent(amount, buildingData.WorkerProductionSupply));
                }
            }

            _workerProductionCoroutine = null;
        }
        
        public void BuildUnit(UnitSO unitToBuild)
        {
            if (!CanQueueUnit(unitToBuild) || _buildingQueue.Count >= MAX_QUEUE_SIZE) return;
            _buildingQueue.Add(unitToBuild);
            if (_buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(UnitQueue);
            }
        }

        public void BuiltSupply(SupplySO supplyToBuild)
        {
            if (supplyToBuild == null || _supplyQueue.Count >= MAX_QUEUE_SIZE) return;

            _supplyQueue.Add(supplyToBuild);
            if (_supplyQueue.Count == 1)
                _supplyCoroutine = StartCoroutine(DoBuildSupplies());
            else
                OnSupplyQueueUpdated?.Invoke(SupplyQueue);
        }

        private IEnumerator DoBuildSupplies()
        {
            while (_supplyQueue.Count > 0)
            {
                SupplySOBeingBuilt = _supplyQueue[0];
                CurrentQueueStartTime = Time.time;
                OnSupplyQueueUpdated?.Invoke(SupplyQueue);

                yield return new WaitForSeconds(SupplySOBeingBuilt.BuildTime);

                Bus<SupplyEvent>.Raise(new SupplyEvent(SupplySOBeingBuilt.AmountPerGather, SupplySOBeingBuilt));
                _supplyQueue.RemoveAt(0);
            }

            SupplySOBeingBuilt = null;
            OnSupplyQueueUpdated?.Invoke(SupplyQueue);
            _supplyCoroutine = null;
        }

        public void CancelSupply(int idx)
        {
            if (idx < 0 || idx >= _supplyQueue.Count) return;

            _supplyQueue.RemoveAt(idx);
            if (idx == 0)
            {
                if (_supplyCoroutine != null)
                    StopCoroutine(_supplyCoroutine);

                SupplySOBeingBuilt = null;
                _supplyCoroutine = _supplyQueue.Count > 0 ? StartCoroutine(DoBuildSupplies()) : null;
            }

            OnSupplyQueueUpdated?.Invoke(SupplyQueue);
        }

        

        private IEnumerator DoBuildUnits()
        {
            while (_buildingQueue.Count > 0)
            {
                UnitSOBeingBuilt = _buildingQueue[0];
                CurrentQueueStartTime = Time.time;
                OnQueueUpdated?.Invoke(UnitQueue);
                
                yield return new WaitForSeconds(UnitSOBeingBuilt.BuildTime); //빌드시간만큼 대기하고

                Worker consumedWorker = null;
                if (ConsumesAssignedWorkerFor(UnitSOBeingBuilt))
                {
                    if (_assignedWorkers.Count == 0)
                    {
                        Debug.LogWarning($"{name}: 유닛 훈련 완료 시 소모할 배정 일꾼이 없습니다.", this);
                        _buildingQueue.RemoveAt(0);
                        continue;
                    }

                    consumedWorker = _assignedWorkers[0];
                }

                Vector3 spawnPosition = transform.position + transform.forward * 3f;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out var hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                    spawnPosition = hit.position;
                UnitSO producedUnit = UnitSOBeingBuilt;
                Instantiate(producedUnit.Prefab, spawnPosition, Quaternion.identity);
                if (consumedWorker != null)
                {
                    RemoveWorker(consumedWorker, false);
                    Destroy(consumedWorker.gameObject);
                }
                Bus<UnitProducedEvent>.Raise(new UnitProducedEvent(producedUnit));
                _buildingQueue.RemoveAt(0);
            }
            
            OnQueueUpdated?.Invoke(UnitQueue);
            UnitSOBeingBuilt = null;
        }

        

        public void CancelGenerate(int idx)
        {
            Debug.Assert(idx >= 0 && idx < _buildingQueue.Count, "인덱스 밖의 유닛을 취소하려고 하고 있습니다.");
            
            UnitSO unitToCancel = _buildingQueue[idx];
            
            //여기에 나중에 자원 반환 로직이 들어가야 합니다.
            _buildingQueue.RemoveAt(idx);

            if (idx == 0)
            {
                StopAllCoroutines(); //첫번째 유닛이 취소된거면 코루틴 다시 시작
                if (_buildingQueue.Count > 0)
                {
                    StartCoroutine(DoBuildUnits()); //아직 남아있으니 재시작
                }
                else
                {
                    OnQueueUpdated?.Invoke(UnitQueue); //큐가 비었다면 이벤트만 발행
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(UnitQueue);  //첫번째 유닛이 아닌경우 코루틴은 그냥 돌려도 된다.
            }
        }
    }
}
