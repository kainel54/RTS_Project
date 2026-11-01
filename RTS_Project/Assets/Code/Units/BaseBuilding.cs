using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public const int MAX_QUEUE_SIZE = 5;
        private List<UnitSO> _buildingQueue = new List<UnitSO>(MAX_QUEUE_SIZE);
        public int QueueSize => _buildingQueue.Count;
        public UnitSO[] Queue => _buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO SOBeingBuilt { get; private set; }

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;
        
        public void BuildUnit(UnitSO unitToBuild)
        {
            _buildingQueue.Add(unitToBuild);
            if (_buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(Queue);
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (_buildingQueue.Count > 0)
            {
                SOBeingBuilt = _buildingQueue[0];
                CurrentQueueStartTime = Time.time;
                OnQueueUpdated?.Invoke(Queue);
                
                yield return new WaitForSeconds(SOBeingBuilt.BuildTime); //빌드시간만큼 대기하고

                GameObject newUnit = Instantiate(SOBeingBuilt.Prefab, transform.position, Quaternion.identity);
                _buildingQueue.RemoveAt(0);
                //newUnit은 나중에 쓰인다.
            }
            
            OnQueueUpdated?.Invoke(Queue);
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
                    OnQueueUpdated?.Invoke(Queue); //큐가 비었다면 이벤트만 발행
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(Queue);  //첫번째 유닛이 아닌경우 코루틴은 그냥 돌려도 된다.
            }
        }
    }
}