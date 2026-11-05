using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using Code.Players;
using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Iron Production command", menuName = "Buildings/Commands/Iron Production", order = 120)]
    public class IronProductionSO : BaseCommandSO
    {
        [field: SerializeField] public SupplyCostSO IronOre { get; private set; }
        [SerializeField] private SupplySO ingotSupplyType;

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding { SupplyQueueSize: < BaseBuilding.MAX_QUEUE_SIZE }
                && UserSupplies.Instance != null
                && UserSupplies.Instance.HasEnoughSupplies(IronOre);
        }

        public override void Handle(CommandContext context)
        {
            if (UserSupplies.Instance.HasEnoughSupplies(IronOre) == false) return;
            BaseBuilding building = context.Commandable as BaseBuilding;
            building.BuiltSupply(ingotSupplyType);
        }
    }
}

