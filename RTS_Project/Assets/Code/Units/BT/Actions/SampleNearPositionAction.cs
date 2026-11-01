using Code.Units;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Sample near position", story: "Set [TargetLocation] to the Closest point on the navmesh to [Unit]", category: "Action/Navigation", id: "066c029792462efd6ab57b1485340f25")]
public partial class SampleNearPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
    [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;

    [SerializeReference] public BlackboardVariable<float> Radius = new BlackboardVariable<float>(5f);

    protected override Status OnStart()
    {
        if (Unit.Value == null || Unit.Value.Agent == null) return Status.Failure;

        NavMeshAgent agent = Unit.Value.Agent;

        NavMeshQueryFilter queryFilter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (NavMesh.SamplePosition(Unit.Value.transform.position, out NavMeshHit navMeshHit, Radius.Value, queryFilter))
        {
            TargetLocation.Value = navMeshHit.position;
            return Status.Success;
        }

        return Status.Failure;
    }

}

