using Code.Players;
using Code.Units;
using Code.Environments;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Build supply command", menuName = "Buildings/Commands/Build supply", order = 121)]
    public class BuildSupplyCommandSO : BaseCommandSO
    {
        [field: SerializeField] public SupplyCostSO Cost { get; private set; }
        [field: SerializeField] public SupplySO Supply { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding { SupplyQueueSize: < BaseBuilding.MAX_QUEUE_SIZE }
                   && UserSupplies.Instance != null
                   && Cost != null
                   && Supply != null
                   && UserSupplies.Instance.HasEnoughSupplies(Cost);
        }

        public override void Handle(CommandContext context)
        {
            if (context.Commandable is not BaseBuilding building
                || building.SupplyQueueSize >= BaseBuilding.MAX_QUEUE_SIZE
                || UserSupplies.Instance == null
                || Cost == null
                || Supply == null
                || !UserSupplies.Instance.TrySpendSupplies(Cost)) return;

            building.BuiltSupply(Supply);
        }
    }
}
