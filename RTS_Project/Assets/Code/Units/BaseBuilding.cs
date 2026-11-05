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
        private List<UnitSO> _buildingQueue = new List<UnitSO>(MAX_QUEUE_SIZE);
        private List<SupplySO> _supplyQueue = new List<SupplySO>(MAX_QUEUE_SIZE);
        private Coroutine _supplyCoroutine;
        public int QueueSize => _buildingQueue.Count;
        public int SupplyQueueSize => _supplyQueue.Count;
        public UnitSO[] UnitQueue => _buildingQueue.ToArray();
        public SupplySO[] SupplyQueue => _supplyQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO UnitSOBeingBuilt { get; private set; }
        [field: SerializeField] public SupplySO SupplySOBeingBuilt { get; private set; }

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public delegate void SupplyQueueUpdatedEvent(SupplySO[] suppliesInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        public event SupplyQueueUpdatedEvent OnSupplyQueueUpdated;

        protected override void Start()
        {
            base.Start();
            PopulationSystem.Instance?.RegisterBuilding(this);
        }

        protected override void OnDestroy()
        {
            PopulationSystem.Instance?.UnregisterBuilding(this);
            base.OnDestroy();
        }
        
        public void BuildUnit(UnitSO unitToBuild)
        {
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

                GameObject newUnit = Instantiate(UnitSOBeingBuilt.Prefab, transform.position, Quaternion.identity);
                _buildingQueue.RemoveAt(0);
            }
            
            OnQueueUpdated?.Invoke(UnitQueue);
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
