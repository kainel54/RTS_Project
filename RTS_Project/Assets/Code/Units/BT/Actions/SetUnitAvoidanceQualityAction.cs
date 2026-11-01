using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set unit avoidance quality", story: "Set [Unit] avoidance quality to [NewValue]", category: "Action/Navigation", id: "7719a0c92a391eaf42a14ad169581afb")]
    public partial class SetUnitAvoidanceQualityAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<int> NewValue;
        
        protected override Status OnStart()
        {
            if(NewValue.Value > 4 || NewValue.Value < 0 || Unit.Value == null || Unit.Value.Agent == null)
                return Status.Failure;
            
            Unit.Value.Agent.obstacleAvoidanceType = (ObstacleAvoidanceType) NewValue.Value;
            return Status.Success;
        }
    }
}

