using Code.Players;
using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Build unit command", menuName = "Buildings/Commands/Build unit", order = 120)]
    public class BuildUnitCommandSO : BaseCommandSO
    {
        [field: SerializeField] public UnitSO Unit { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding { QueueSize: < BaseBuilding.MAX_QUEUE_SIZE }
                && UserSupplies.Instance != null
                && UserSupplies.Instance.HasEnoughSupplies(Unit.Cost);
        }

        public override void Handle(CommandContext context)
        {
            if (UserSupplies.Instance.HasEnoughSupplies(Unit.Cost) == false) return;
            BaseBuilding building = context.Commandable as BaseBuilding;
            building.BuildUnit(Unit);
        }
    }
}