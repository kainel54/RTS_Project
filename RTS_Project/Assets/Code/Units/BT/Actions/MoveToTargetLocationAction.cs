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
    [NodeDescription(name: "Move to target location", story: "[Unit] move to [TargetLocation]", category: "Action/Navigation", id: "9ca9db13042aea8b8f4907c68995773d")]
    public partial class MoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

        [SerializeReference] public BlackboardVariable<ParamSO> SpeedParameter;

        private NavMeshAgent _agent;
        private UnitAnimator _animator;
        
        protected override Status OnStart()
        {
            if(Unit.Value == null || Unit.Value.Agent == null)
                return Status.Failure;
            
            _agent = Unit.Value.Agent;
            _animator = Unit.Value.UnitAnimator;
            if (Vector3.Distance(_agent.transform.position, TargetLocation.Value) < _agent.stoppingDistance)
            {
                return Status.Success; //이미 목적지에 있다면 도착 처리
            }

            _agent.SetDestination(TargetLocation.Value);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if(_animator != null)
            {
                _animator.SetParam(SpeedParameter.Value, _agent.velocity.magnitude);
            }
            

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                return Status.Success;
            }
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if(_animator != null)
            {
                _animator.SetParam(SpeedParameter, 0f);
            }
        }
    }
}

