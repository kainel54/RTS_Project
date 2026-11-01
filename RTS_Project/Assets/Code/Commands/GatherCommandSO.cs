using Code.Environments;
using Code.Units;
using System;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Gather command", menuName = "Units/Commands/Gather", order = 105)]
    public class GatherCommandSO : BaseCommandSO
    {
        [SerializeField] private UnitSO keepDataSO;

        public override bool CanHandle(CommandContext context)
        {
            //지금 명령수행하는 주체가 Worker이고 클릭한 곳이 GatherableSupply라면 가능하다.
            return context.Commandable is Worker
                   && context.Hit.collider != null
                   && IsGatherableSupplyOrKeep(context.Hit.collider);
        }

        public override void Handle(CommandContext context)
        {
            Worker worker = context.Commandable as Worker;

            if(context.Hit.collider.TryGetComponent(out GatherableSupply supply))
            {
                worker.Gather(supply);
            }
            else if(IsKeep(context.Hit.collider)&& worker.HasSupplies)
            {
                worker.ReturnSupplies(context.Hit.collider.gameObject);
            }
            else
            {
                worker.MoveTo(context.Hit.collider.transform.position);
            }
        }

        private bool IsGatherableSupplyOrKeep(Collider collider) => collider.TryGetComponent(out GatherableSupply _) || IsKeep(collider);

        private bool IsKeep(Collider collider) => collider.TryGetComponent(out BaseBuilding building)&&building.AbstractUnitData.Equals(keepDataSO);
    }
}