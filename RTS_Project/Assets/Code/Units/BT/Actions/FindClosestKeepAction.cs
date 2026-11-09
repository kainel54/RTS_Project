using System;
using System.Collections.Generic;
using Code.Util;
using Code.Units;
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
        
        protected override Status OnStart()
        {
            if (Unit.Value == null)
                return Status.Failure;
            
            if (Unit.Value is not Worker worker || TargetBuildingTypes?.Value?.buildingSOList == null)
                return Status.Failure;

            var targetBuildingTypes = TargetBuildingTypes.Value.buildingSOList;
            List<StorageBuilding> candidates = new List<StorageBuilding>();
            foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
            {
                if (storage != null
                    && targetBuildingTypes.Exists(type => type == storage.AbstractUnitData)
                    && storage.AvailableCapacity >= worker.CurrentSupplyAmount)
                    candidates.Add(storage);
            }

            if (candidates.Count == 0) return Status.Failure;

            candidates.Sort(new ClosestMonoComparer<StorageBuilding>(Unit.Value.transform.position));
            foreach (StorageBuilding candidate in candidates)
            {
                if (!worker.TryReserveStorage(candidate, worker.CurrentSupplyAmount)) continue;
                Keep.Value = candidate.gameObject;
                return Status.Success;
            }

            return Status.Failure;
        }

    }
}

