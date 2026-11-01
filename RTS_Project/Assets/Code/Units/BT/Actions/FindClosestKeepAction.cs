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
        [SerializeReference] public BlackboardVariable<BuildingSO> TargetBuildingType;
        
        private Collider[] _results = new Collider[20]; //미리 배열을 할당해서 힙 할당을 최소화한다.

        protected override Status OnStart()
        {
            if (Unit.Value == null)
                return Status.Failure;
            
            //레이어마스크는 현재 BT에서 블랙보드 직렬화가 지원되지 않는다. (BT 1.0.12 기준)
            LayerMask buildingMask = LayerMask.GetMask("Building");
            int cnt = Physics.OverlapSphereNonAlloc(Unit.Value.transform.position, SearchRadius.Value, _results,
                buildingMask);
            
            //근거리에 있는 건물중에서 Keep을 모두 가져와서 리스트에 넣어준다.
            List<BaseBuilding> nearByKeep = new List<BaseBuilding>();
            for (int i = 0; i < cnt; i++)
            {
                if (_results[i].TryGetComponent(out BaseBuilding building)
                    && building.AbstractUnitData.Equals(TargetBuildingType.Value))
                {
                    nearByKeep.Add(building);
                }
            }
            
            if(nearByKeep.Count == 0)
                return Status.Failure;
            
            nearByKeep.Sort(new ClosestMonoComparer<BaseBuilding>(Unit.Value.transform.position));
            Keep.Value = nearByKeep[0].gameObject;
            
            return Status.Success;
        }

    }
}

