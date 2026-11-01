using Code.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set NavAgent enable", story: "[Unit] set NavAgent active status to [IsActive]", category: "Action/Navigation", id: "19ee54f68cad6e196bc362223d14af7a")]
    public partial class SetNavAgentEnableAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<bool> IsActive;

        [SerializeReference] public BlackboardVariable<bool> IsWarpTo = new BlackboardVariable<bool>(false);
        protected override Status OnStart()
        {
            if (Unit.Value == null || Unit.Value.Agent == null)
            {
                return Status.Failure;
            }
            Unit.Value.Agent.enabled = IsActive.Value;

            if (IsActive.Value && IsWarpTo.Value)
            {
                Unit.Value.Agent.Warp(Unit.Value.transform.position);
            }

            return Status.Success;
        }
    }
}