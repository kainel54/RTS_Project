using Code.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Assets.Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Reset Unit command override", story: "Reset [Unit] commands", category: "Action/Units", id: "d9437fdb0e706493a57b9ce12d023970")]
    public partial class ResetUnitCommandOverrideAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;

        protected override Status OnStart()
        {
            Unit.Value.SetCommandOverrides(null);
            return Status.Success;
        }
    }
}