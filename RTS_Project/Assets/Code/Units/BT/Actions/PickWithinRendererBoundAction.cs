using Code.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Pick within renderer bound", story: "Set [TargetLocation] to a random paint within [ConstructionDummy]", category: "Action/Building", id: "ffc670b3f6c59cc1589eaa9c82167ebe")]
    public partial class PickWithinRendererBoundAction : Action
    {
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<ConstructionDummy> ConstructionDummy;

        protected override Status OnStart()
        {
            if (ConstructionDummy.Value == null) return Status.Failure;

            Renderer mainRenderer = ConstructionDummy.Value.MainRenderer;
            if (mainRenderer == null) return Status.Failure;

            Bounds bounds = mainRenderer.bounds;
            Vector3 extents = bounds.extents; //절반크기

            int edgeAxis = Random.Range(0, 2); //0 for x, 1 for z
            float x = 0, z = 0;
            if (edgeAxis == 0)
            {
                x = Random.value < 0.5f ? -extents.x : extents.x;
                z = Random.Range(-extents.z, extents.z);
            }
            else
            {
                x = Random.Range(-extents.x, extents.x);
                z = Random.value < 0.5f ? -extents.z : extents.z;
            }
            TargetLocation.Value = new Vector3(bounds.center.x + x, TargetLocation.Value.y, bounds.center.z + z);

            return Status.Success;
        }

    }
}