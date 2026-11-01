using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Code.Units.Animations;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Translate position", story: "[Unit] Move to [TargetLocation] at [Speed]", category: "Action/Units", id: "975c9acdbb28fcc3369bc0082c40f4b6")]
    public partial class TranslatePositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<float> Speed;

        [SerializeReference] public BlackboardVariable<ParamSO> SpeedParameter;

        private UnitAnimator _animator;
        private float _endTime;
        private Vector3 _direction;
        private Transform _selfTransform;

        protected override Status OnStart()
        {
            if(Unit.Value == null) return Status.Failure;

            _animator = Unit.Value.UnitAnimator;
            _selfTransform = Unit.Value.transform;

            float distance = Vector3.Distance(_selfTransform.position, TargetLocation.Value);
            _endTime = Time.time + distance / Speed.Value;
            _direction = (TargetLocation.Value - _selfTransform.position).normalized;
            _selfTransform.forward = _direction;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if(Time.time>_endTime) return Status.Success;
            _animator?.SetParam(SpeedParameter.Value,Speed.Value);
            _selfTransform.position += Speed.Value * Time.deltaTime * _direction;
            return Status.Success;
        }

        protected override void OnEnd()
        {
            _animator?.SetParam(SpeedParameter.Value, 0f);
        }
    }
}

