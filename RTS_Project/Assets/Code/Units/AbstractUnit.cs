using System;
using System.Collections.Generic;
using Code.CoreSystem;
using Code.GameEvents;
using Code.Units.Animations;
using Code.Units.BT;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        [SerializeField] private VariableSO[] btVariables;

        private Dictionary<BTVariables, SerializableGUID> _variableDict;
        
        public NavMeshAgent Agent { get; private set; }
        public BehaviorGraphAgent GraphAgent { get; private set; }
        public UnitAnimator UnitAnimator { get; private set; }
        public float AgentRadius => Agent.radius;

        protected override void Awake()
        {
            base.Awake();
            Agent = GetComponent<NavMeshAgent>();
            GraphAgent = GetComponent<BehaviorGraphAgent>();
            UnitAnimator = GetComponent<UnitAnimator>();
            Debug.Assert(Agent != null || GraphAgent != null&&UnitAnimator !=null, $"유닛은 필수 컴포넌트를 가져야 합니다.: {gameObject.name}");
        }

        protected override void Start()
        {
            _variableDict = new Dictionary<BTVariables, SerializableGUID>();
            foreach (VariableSO variable in btVariables)
            {
                if (!GraphAgent.GetVariableID(variable.VariableName.ToString(), out var guid))
                {
                    Debug.LogError($"필요한 BT변수가 유닛 블랙보드에 정의되어 있지 않습니다. : {name}- {variable.VariableName}");
                    continue;
                }
                _variableDict.Add(variable.VariableName, guid);
            }
            
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Bus<UnitDeSpawnEvent>.Raise(new UnitDeSpawnEvent(this));
        }

        public virtual void MoveTo(Vector3 position)
        {
            SetBTVariable(BTVariables.TargetLocation, position);
            SetBTVariable(BTVariables.Command, UnitCommands.Move);
        }

        public virtual void Stop()
        {
            SetBTVariable(BTVariables.TargetLocation, transform.position);
            SetBTVariable(BTVariables.Command, UnitCommands.Stop);
        }

        public void SetBTVariable<T>(BTVariables variables, T value)
        {
            if (variables == BTVariables.Command && value is UnitCommands command && command != UnitCommands.Attack)
                GetComponent<MeleeCombat>()?.CancelAttack();

            if (_variableDict.TryGetValue(variables, out var guid))
            {
                GraphAgent.SetVariableValue(guid, value);
                return;
            }
            Debug.LogError($"없는 블랙보드 변수의 값을 세팅하려 했습니다. : {name}- {variables}");
        }

        public bool TryGetBTVariable<T>(BTVariables varName,out BlackboardVariable<T> variable)
        {
            if (_variableDict.TryGetValue(varName, out SerializableGUID guid))
            {
                return GraphAgent.GetVariable(guid, out variable);
            }

            Debug.LogError($"없는 블랙보드 변수값을 가져오려 했습니다. : {name} - {varName}");
            variable = null;
            return false;
        }
    }
}
