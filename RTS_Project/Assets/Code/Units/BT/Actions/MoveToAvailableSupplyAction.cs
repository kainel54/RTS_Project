using Code.Environments;
using Code.Units;
using Code.Units.Animations;
using Code.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move to available supply", story: "[Unit] move to [GatherableSupply] or nearby not busy supply", category: "Action/Navigation", id: "dad443c61d9e7d6a3d5fa0f12546be54")]
public partial class MoveToAvailableSupplyAction : Action
{
    [SerializeReference] public BlackboardVariable<AbstractUnit> Unit;
    [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupply;
    
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(7f);
    [SerializeReference] public BlackboardVariable<ParamSO> SpeedParameter;


    private NavMeshAgent _agent;
    private LayerMask _supplyMask;
    private SupplySO _supplyData;
    private Collider[] _results = new Collider[15];
    private List<GatherableSupply> _notBusySupplies = new List<GatherableSupply>(15);
    private Collider _collider;
    private UnitAnimator _animator;


    protected override Status OnStart()
    {
        _supplyMask = LayerMask.GetMask("Supplies");
        _animator = Unit.Value.UnitAnimator;
        if(!HasValidInputs())
            return Status.Failure;
        
        //여기로 왔으면 정상적으로 모든 것을 갖추고 있고. 내가 캐러갈 오브젝트가 설정된 상태야. 
        _collider = GatherableSupply.Value.GetComponent<Collider>();
        Vector3 targetPosition = GetTargetPosition();
        _agent.SetDestination(targetPosition);
        return Status.Running;
    }

    private Vector3 GetTargetPosition()
    {
        if (_collider != null)
            return _collider.ClosestPoint(_agent.transform.position);
        return GatherableSupply.Value.transform.position;
    }

    private bool HasValidInputs()
    {
        //서플라이 데이터와 GatherableSupply 둘다 없는건 문제가 있다. (무엇을 캐야할지에 대한 정보는 필요하다.)
        if (Unit.Value == null || Unit.Value.Agent == null || 
            (GatherableSupply.Value == null && _supplyData == null))
            return false;

        _agent = Unit.Value.Agent;
        if (GatherableSupply.Value != null)
        {
            _supplyData = GatherableSupply.Value.SupplyData;
        }
        else
        {
            return UpdateNearByNotBusySupply();
        }

        return true;
    }

    private bool UpdateNearByNotBusySupply()
    {
        int cnt = Physics.OverlapSphereNonAlloc(_agent.transform.position, SearchRadius, _results, _supplyMask);
        _notBusySupplies.Clear();

        for (int i = 0; i < cnt; i++)
        {
            //근처에 내가 찾고자 하는 자원이면서 Busy하지 않은 자원을 찾는다.
            if (_results[i].TryGetComponent(out GatherableSupply supply)
                && supply.SupplyData.Equals(_supplyData)
                && supply.IsBusy == false)
            {
                _notBusySupplies.Add(supply);
            }
        }
        
        //그 자원들 중에서 가장 가까운 녀석을 가져온다.
        if (_notBusySupplies.Count > 0)
        {
            _notBusySupplies.Sort(new ClosestMonoComparer<GatherableSupply>(_agent.transform.position));
            GatherableSupply.Value = _notBusySupplies.First();
            return true;
        }

        return false; //못찾았다면 false
    }

    protected override Status OnUpdate()
    {
        if (_animator != null)
        {
            _animator.SetParam(SpeedParameter.Value, _agent.velocity.magnitude);
        }

        if (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
            return Status.Running;
        
        //여기까지 왔다면 목표지에 도착했다는 뜻.
        
        if(GatherableSupply.Value != null 
           && GatherableSupply.Value.IsBusy == false 
           && GatherableSupply.Value.Amount > 0)
            return Status.Success;
        
        //여기까지 왔다면 간신히 찾아왔는데 이미 그것도 없어졌거나 누가 캐고 있다는거.
        if (UpdateNearByNotBusySupply())
        {
            _collider = GatherableSupply.Value.GetComponent<Collider>();
            Vector3 targetPosition = GetTargetPosition();
            _agent.SetDestination(targetPosition);
            return Status.Running;
        }
        
        return Status.Failure;
    }

    protected override void OnEnd()
    {
        if (_animator != null)
        {
            _animator.SetParam(SpeedParameter, 0f);
        }
    }
}

