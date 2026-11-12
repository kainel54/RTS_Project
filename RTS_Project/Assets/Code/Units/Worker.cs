using Code.Commands;
using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using Code.Units.BT;
using Code.Units.BT.Events;
using Unity.Behavior;
using UnityEngine;

namespace Code.Units
{
    public class Worker : AbstractUnit, IBuildingConstructor
    {
        [SerializeField] private GameObject backPackObject;
        [SerializeField] private BaseCommandSO[] cancelConstructionCommandSet;
        public bool HasSupplies => backPackObject.activeSelf;
        public int CurrentSupplyAmount => TryGetBTVariable<int>(BTVariables.SupplyAmountHeld,
            out BlackboardVariable<int> amountVariable) ? amountVariable.Value : 0;
        public BaseBuilding AssignedBuilding { get; private set; }
        private StorageBuilding _reservedStorage;
        private int _reservedStorageAmount;
        public bool IsBuilding=>TryGetBTVariable(BTVariables.Command,out BlackboardVariable<UnitCommands> commandValue)&&
            commandValue.Value==UnitCommands.ConstructBuilding;

        public override void MoveTo(Vector3 position)
        {
            ReleaseStorageReservation();
            base.MoveTo(position);
        }

        private void Update()
        {
            if (AssignedBuilding == null || Agent == null || Agent.pathPending)
                return;

            bool hasArrived = Agent.isOnNavMesh
                ? Agent.hasPath
                    ? Agent.remainingDistance <= Mathf.Max(Agent.stoppingDistance, 0.2f)
                    : Vector3.Distance(transform.position, AssignedBuilding.transform.position) <= 2f
                : Vector3.Distance(transform.position, AssignedBuilding.transform.position) <= 2f;

            if (!hasArrived)
                return;

            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }

            gameObject.SetActive(false);
        }

        public bool AssignToBuilding(BaseBuilding building)
        {
            if (building == null) return false;
            ReleaseStorageReservation();
            return building.TryAssignWorker(this);
        }

        public void UnassignFromBuilding()
        {
            if (AssignedBuilding != null)
                AssignedBuilding.RemoveWorker(this);
        }

        internal void SetAssignedBuilding(BaseBuilding building, bool reactivateWhenUnassigned = true)
        {
            AssignedBuilding = building;
            if (building == null)
            {
                gameObject.SetActive(reactivateWhenUnassigned);
                return;
            }

            gameObject.SetActive(true);
            MoveTo(building.transform.position);
        }

        protected override void OnDestroy()
        {
            ReleaseStorageReservation();
            UnassignFromBuilding();
            base.OnDestroy();
        }

        protected override void Start()
        {
            base.Start();
            if (TryGetBTVariable<SupplyEventChannel>(BTVariables.SupplyEventChannel, out var evtChannel))
            {
                evtChannel.Value.Event += HandleSupplyEvent;
            }

            if (TryGetBTVariable<int>(BTVariables.SupplyAmountHeld, out var amountVariable))
            {
                amountVariable.OnValueChanged += () => backPackObject.SetActive(amountVariable.Value > 0);
            }
        }


        private void HandleSupplyEvent(AbstractUnit unit, int amount, SupplySO supplyType)
        {
            Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supplyType, _reservedStorage));
            ReleaseStorageReservation();
        }

        public void Gather(GatherableSupply supply)
        {
            ReleaseStorageReservation();
            UnassignFromBuilding();
            SetBTVariable(BTVariables.GatherableSupply, supply);
            SetBTVariable(BTVariables.TargetGameObject, supply.gameObject);
            SetBTVariable(BTVariables.Command, UnitCommands.Gather);
        }

        public void ReturnSupplies(GameObject targetGameObject)
        {
            StorageBuilding requestedStorage = targetGameObject != null
                ? targetGameObject.GetComponent<StorageBuilding>()
                : null;
            StorageBuilding destination = null;
            if (requestedStorage != null && TryReserveStorage(requestedStorage, CurrentSupplyAmount))
            {
                destination = requestedStorage;
            }
            else
            {
                float closestDistance = float.MaxValue;
                foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
                {
                    if (storage == null || storage == requestedStorage
                        || storage.AvailableCapacity < CurrentSupplyAmount) continue;

                    float distance = (storage.transform.position - transform.position).sqrMagnitude;
                    if (distance >= closestDistance) continue;
                    closestDistance = distance;
                    destination = storage;
                }

                if (destination == null || !TryReserveStorage(destination, CurrentSupplyAmount))
                {
                    Debug.LogWarning($"{name}: 수용 가능한 저장소가 없습니다.");
                    return;
                }
            }

            UnassignFromBuilding();
            SetBTVariable(BTVariables.TargetGameObject, destination.gameObject);
            SetBTVariable(BTVariables.Command, UnitCommands.ReturnSupplies);
        }

        public bool TryReserveStorage(StorageBuilding storage, int amount)
        {
            ReleaseStorageReservation();
            if (storage == null || !storage.TryReserveCapacity(amount)) return false;

            _reservedStorage = storage;
            _reservedStorageAmount = amount;
            return true;
        }

        public void ReleaseStorageReservation()
        {
            if (_reservedStorage != null)
                _reservedStorage.ReleaseReservedCapacity(_reservedStorageAmount);

            _reservedStorage = null;
            _reservedStorageAmount = 0;
        }



        public GameObject ConstructBuilding(BuildingSO building, Vector3 targetLocation)
        {
            ReleaseStorageReservation();
            UnassignFromBuilding();
            GameObject instance = Instantiate(building.ConstructionDummy, targetLocation, Quaternion.identity);
            if (!instance.TryGetComponent(out ConstructionDummy dummy))
            {
                Debug.LogError($"건설 더미 컴포넌트를 찾을 수 없음 : {instance.name}");
                return null;
            }
            dummy.StartPlacementGhost(this);

            SetBTVariable(BTVariables.ConstructBuildingSO, building);
            SetBTVariable(BTVariables.TargetLocation, targetLocation);
            SetBTVariable(BTVariables.ConstructionDummy, dummy);
            SetBTVariable(BTVariables.Command, UnitCommands.ConstructBuilding);

            SetCommandOverrides(cancelConstructionCommandSet);

            return instance;
        }

        public void CancelConstruction()
        {
            if (TryGetBTVariable(BTVariables.ConstructionDummy, out BlackboardVariable<ConstructionDummy> constructionDummy))
            {
                Destroy(constructionDummy.Value.gameObject);
                constructionDummy.Value = null;
            }

            SetCommandOverrides(null); //원래대로 커맨드 돌려준다.
            Stop();

        }

        public void ResumeConstruction(ConstructionDummy building)
        {
            ReleaseStorageReservation();
            UnassignFromBuilding();
            SetBTVariable(BTVariables.TargetLocation, building.transform.position);
            SetBTVariable(BTVariables.ConstructionDummy, building);
            SetBTVariable(BTVariables.ConstructBuildingSO, building.BuildingSo);
            SetBTVariable(BTVariables.Command, UnitCommands.ConstructBuilding);

            SetCommandOverrides(cancelConstructionCommandSet);
        }

        public override void DeSelect()
        {
            decalProjector.gameObject.SetActive(false);
            IsSelected = false;
            if (IsBuilding == false)
            {
                SetCommandOverrides(null); //선택해제시 메뉴 원상복귀해야함.
            }

            Bus<UnitDeSelectEvent>.Raise(new UnitDeSelectEvent(this));
        }

    }
}
