using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Rotate to target", story: "[Unit] rotate to [Target]", category: "Action/Units", id: "c84feb9ced6c6c695e6f2e58fbd36bb9")]
    public partial class RotateToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        protected override Status OnStart()
        {
            if (Unit.Value == null || Target.Value == null)
            {
                return Status.Failure;
            }
            Vector3 unitPosition = Unit.Value.transform.position;
            Vector3 targetPosition = Target.Value.position;

            Unit.Value.transform.forward = (targetPosition - unitPosition).normalized;
            return Status.Success;
        }
    }
}