using System;
using System.Collections.Generic;
using Code.Util;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Find closest Keep", story: "[Unit] find closest [Keep]", category: "Action/Units", id: "851dd0dab3593b7e6ced276cdfde99e4")]
    public partial class FindClosestKeepAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> Keep;
        
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10f);
        [SerializeReference] public BlackboardVariable<BuildingSOList> TargetBuildingTypes;
        
        private Collider[] _results = new Collider[20]; //미리 배열을 할당해서 힙 할당을 최소화한다.

        protected override Status OnStart()
        {
            if (Unit.Value == null)
                return Status.Failure;
            
            LayerMask buildingMask = LayerMask.GetMask("Building");
            int cnt = Physics.OverlapSphereNonAlloc(Unit.Value.transform.position, SearchRadius.Value, _results,
                buildingMask);
            var targetBuildingTypes = TargetBuildingTypes.Value.buildingSOList;
            //근거리에 있는 건물중에서 Keep을 모두 가져와서 리스트에 넣어준다.
            List<StorageBuilding> nearByKeep = new List<StorageBuilding>();
            for (int i = 0; i < cnt; i++)
            {
                if (_results[i].TryGetComponent(out StorageBuilding building)
                    && targetBuildingTypes.Exists(type => type == building.AbstractUnitData))
                {
                    nearByKeep.Add(building);
                }
            }
            
            if(nearByKeep.Count == 0)
                return Status.Failure;
            
            nearByKeep.Sort(new ClosestMonoComparer<StorageBuilding>(Unit.Value.transform.position));
            Keep.Value = nearByKeep[0].gameObject;
            
            return Status.Success;
        }

    }
}

