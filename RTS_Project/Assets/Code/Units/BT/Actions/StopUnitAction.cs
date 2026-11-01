using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Stop unit", story: "[Unit] stop moving", category: "Action/Navigation", id: "dd2399f3589536fd8e866a1f1dc672f5")]
    public partial class StopUnitAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;

        protected override Status OnStart()
        {
            if (Unit.Value == null || Unit.Value.Agent == null)
                return Status.Failure;
            
            Unit.Value.Agent.ResetPath();
            return Status.Success;
        }
    }
}

