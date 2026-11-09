using System;
using Code.Environments;
using Code.Units.Animations;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Units.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Gather supplies", story: "[Unit] gather [Amount] supplies from [GatherableSupply] : [SupplyType]", category: "Action/Units", id: "12d2fd9e6a10bef491afc286ed4ade0d")]
    public partial class GatherSuppliesAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
        [SerializeReference] public BlackboardVariable<int> Amount;
        [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupply;
        [SerializeReference] public BlackboardVariable<SupplySO> SupplyType;

        [SerializeReference] public BlackboardVariable<ParamSO> AttackParameter;
        [SerializeReference] public BlackboardVariable<float> AttackPeriod = new(1f);

        private float _enterTime; //캐기 시작한 시간
        private float _currentAnimationTime;
        private UnitAnimator _animator;
        private bool _hasGatherLock;

        protected override Status OnStart()
        {
            if (Unit.Value == null || GatherableSupply.Value == null || GatherableSupply.Value.SupplyData == null
                || !GatherableSupply.Value.BeginGather())
                return Status.Failure;

            _hasGatherLock = true;
            _enterTime = Time.time;
            _animator = Unit.Value.UnitAnimator;
            _currentAnimationTime = 0;
            _animator?.SetParam(AttackParameter.Value);

            SupplyType.Value = GatherableSupply.Value.SupplyData; //캐기 시작한 광물의 타입을 기록
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (GatherableSupply.Value.SupplyData.BaseGatherTime + _enterTime <= Time.time)
            {
                return Status.Success; //캐는 시간이 다 지났다면 종료.
            }

            _currentAnimationTime += Time.deltaTime;

            if (_currentAnimationTime >= AttackPeriod.Value)
            {
                _currentAnimationTime = 0f;
                float remainTime = GatherableSupply.Value.SupplyData.BaseGatherTime - (Time.time - _enterTime);

                if (remainTime > AttackPeriod.Value)
                    _animator?.SetParam(AttackParameter.Value);
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (!_hasGatherLock || GatherableSupply.Value == null) return;

            if (CurrentStatus == Status.Success)
            {
                Amount.Value = GatherableSupply.Value.EndGather();
            }
            else
            {
                GatherableSupply.Value.AbortGather();
            }

            _hasGatherLock = false;
        }
    }
}

