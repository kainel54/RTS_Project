using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Assign worker to building", menuName = "Units/Commands/Assign worker to building", order = 106)]
    public class AssignWorkerToBuildingCommandSO : BaseCommandSO
    {
        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not Worker worker || context.Hit.collider == null)
                return false;

            BaseBuilding building = context.Hit.collider.GetComponentInParent<BaseBuilding>();
            return building != null
                   && (worker.AssignedBuilding == building || building.HasAvailableWorkerSlot);
        }

        public override void Handle(CommandContext context)
        {
            if (context.Commandable is not Worker worker || context.Hit.collider == null)
                return;

            BaseBuilding building = context.Hit.collider.GetComponentInParent<BaseBuilding>();
            if (building != null)
                worker.AssignToBuilding(building);
        }
    }
}
