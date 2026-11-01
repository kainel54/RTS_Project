using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Start construction", story: "[Unit] start to [constructionDummy]", category: "Action/Builiding", id: "d19514c2e995e200ea8807619706c5b3")]
    public partial class StartConstructionAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<ConstructionDummy> ConstructionDummy;

        protected override Status OnStart()
        {
            if(ConstructionDummy.Value == null || !(Unit.Value is IBuildingConstructor constructor)) 
                return Status.Failure;

            ConstructionDummy.Value.StartConstruction(constructor);

            return Status.Success;
        }
    }
}