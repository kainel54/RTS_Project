using Code.Units.Animations;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to target gameObject", story: "[Unit] move to [TargetGameObject]", category: "Action/Navigation", id: "4282db2dd053d6ba91c0ef047fc56d93")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        [SerializeReference] public BlackboardVariable<float> MoveThreshold = new(0.25f);
        [SerializeReference] public BlackboardVariable<ParamSO> SpeedParameter;

        private NavMeshAgent _agent;
        private Vector3 _lastPosition;
        private Collider _targetCollider;
        private UnitAnimator _animator;


        protected override Status OnStart()
        {
            _animator = Unit.Value.UnitAnimator;
            if (Unit.Value == null || 
                Unit.Value.Agent == null || 
                TargetGameObject.Value == null)
                return Status.Failure;
            
            _targetCollider = TargetGameObject.Value.GetComponent<Collider>();

            _agent = Unit.Value.Agent;
            Vector3 targetPosition = GetTargetPosition();

            if (Vector3.Distance(_agent.transform.position, targetPosition) < _agent.stoppingDistance)
                return Status.Success;

            _agent.SetDestination(targetPosition);
            _lastPosition = targetPosition;
            
            return Status.Running;
        }

        private Vector3 GetTargetPosition()
        {
            if (_targetCollider != null)
            {
                return _targetCollider.ClosestPoint(_agent.transform.position);
            }
            return TargetGameObject.Value.transform.position;
        }

        protected override Status OnUpdate()
        {
            if (_animator != null)
            {
                _animator.SetParam(SpeedParameter.Value, _agent.velocity.magnitude);
            }

            Vector3 newTargetPosition = GetTargetPosition();

            //타겟이 이동을 한거다.
            if (Vector3.Distance(newTargetPosition, _lastPosition) >= MoveThreshold.Value)
            {
                _agent.SetDestination(newTargetPosition);
                _lastPosition = newTargetPosition;
                return Status.Running;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                return Status.Success;
            }
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            //나중에 애니메이션에 써야 한다.
            if (_animator != null)
            {
                _animator.SetParam(SpeedParameter, 0f);
            }
        }
    }
}

