using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Object = UnityEngine.Object;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Construction building", story: "[Unit] construct [BuildingSO] at [TargetLocation]", category: "Action/Building", id: "69d7f1a5d583d88c31805775e19b4dac")]
    public partial class ConstructionBuildingAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<BuildingSO> BuildingSO;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

        [SerializeReference] public BlackboardVariable<ConstructionDummy> DummyBuilding;

        private float _startBuildTime;
        private float _targetHealth;
        private Vector3 _finalPosition;

        protected override Status OnStart()
        {
            if (Unit.Value == null || BuildingSO.Value == null || BuildingSO.Value.Prefab == null)
                return Status.Failure;

            _startBuildTime = DummyBuilding.Value.ProgressData.StartTime;
            _finalPosition = TargetLocation.Value;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizeTime = (Time.time - _startBuildTime) / BuildingSO.Value.BuildTime;
            DummyBuilding.Value.UpdateConstructionProgress(normalizeTime);
            if (normalizeTime >= 1)
            {
                GameObject building = Object.Instantiate(BuildingSO.Value.Prefab, _finalPosition,Quaternion.identity);
                BaseBuilding baseBuilding = building.GetComponent<BaseBuilding>();
                if (DummyBuilding.Value.IsSelected)
                    baseBuilding.Select();

                DummyBuilding.Value.ConstructionComplete(); //순서 중요.
                DummyBuilding.Value = null;
            }

            return normalizeTime >= 1 ? Status.Success : Status.Running;
        }
    }
}

