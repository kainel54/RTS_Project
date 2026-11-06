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
        public BaseBuilding AssignedBuilding { get; private set; }
        public bool IsBuilding=>TryGetBTVariable(BTVariables.Command,out BlackboardVariable<UnitCommands> commandValue)&&
            commandValue.Value==UnitCommands.ConstructBuilding;

        public bool AssignToBuilding(BaseBuilding building)
        {
            if (building == null || !building.TryAssignWorker(this)) return false;

            MoveTo(building.transform.position);
            return true;
        }

        public void UnassignFromBuilding()
        {
            if (AssignedBuilding != null)
                AssignedBuilding.RemoveWorker(this);
        }

        internal void SetAssignedBuilding(BaseBuilding building) => AssignedBuilding = building;

        protected override void OnDestroy()
        {
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
            Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supplyType));
        }

        public void Gather(GatherableSupply supply)
        {
            UnassignFromBuilding();
            SetBTVariable(BTVariables.GatherableSupply, supply);
            SetBTVariable(BTVariables.TargetGameObject, supply.gameObject);
            SetBTVariable(BTVariables.Command, UnitCommands.Gather);
        }

        public void ReturnSupplies(GameObject targetGameObject)
        {
            UnassignFromBuilding();
            SetBTVariable(BTVariables.TargetGameObject, targetGameObject);
            SetBTVariable(BTVariables.Command, UnitCommands.ReturnSupplies);
        }



        public GameObject ConstructBuilding(BuildingSO building, Vector3 targetLocation)
        {
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
