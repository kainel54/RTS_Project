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
                && (Unit.Cost == null || UserSupplies.Instance.HasEnoughSupplies(Unit.Cost))
                && context.Commandable is BaseBuilding building && building.CanQueueUnit(Unit)
                && PopulationSystem.Instance != null
                && PopulationSystem.Instance.CanQueue(Unit, building);
        }

        public override void Handle(CommandContext context)
        {
            if (Unit == null
                || context.Commandable is not BaseBuilding building
                || building.QueueSize >= BaseBuilding.MAX_QUEUE_SIZE
                || UserSupplies.Instance == null
                || PopulationSystem.Instance == null
                || !building.CanQueueUnit(Unit)
                || !PopulationSystem.Instance.CanQueue(Unit, building)
                || (Unit.Cost != null && !UserSupplies.Instance.TrySpendSupplies(Unit.Cost))) return;

            building.BuildUnit(Unit);
        }
    }
}
