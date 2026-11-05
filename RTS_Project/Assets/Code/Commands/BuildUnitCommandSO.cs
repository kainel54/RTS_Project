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
            return Unit != null
                && context.Commandable is BaseBuilding { QueueSize: < BaseBuilding.MAX_QUEUE_SIZE }
                && UserSupplies.Instance != null
                && UserSupplies.Instance.HasEnoughSupplies(Unit.Cost)
                && PopulationSystem.Instance != null
                && PopulationSystem.Instance.CanQueue(Unit);
        }

        public override void Handle(CommandContext context)
        {
            if (Unit == null
                || context.Commandable is not BaseBuilding building
                || building.QueueSize >= BaseBuilding.MAX_QUEUE_SIZE
                || UserSupplies.Instance == null
                || PopulationSystem.Instance == null
                || !PopulationSystem.Instance.CanQueue(Unit)
                || !UserSupplies.Instance.TrySpendSupplies(Unit.Cost)) return;

            building.BuildUnit(Unit);
        }
    }
}
