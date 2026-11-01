using Code.Units.Animations;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set UnitAnimator trigger", story: "Set [Unit] Trigger [ParamSO]", category: "Action/Animation", id: "cab47c7f13d1d0e7cfd6a46919329415")]
    public partial class SetUnitAnimatorTriggerAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<ParamSO> ParamSO;

        protected override Status OnStart()
        {
            if(Unit.Value == null || Unit.Value.UnitAnimator == null)
            {
                return Status.Failure;
            }
            
            Unit.Value.UnitAnimator.SetParam(ParamSO.Value);
            return Status.Success;
        }
    }
}