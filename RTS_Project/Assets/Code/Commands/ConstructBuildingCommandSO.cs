using Code.Players;
using Code.Units;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Construct building", menuName = "Units/Commands/Construct building", order = 0)]
    public class ConstructBuildingCommandSO : BaseCommandSO, IConstructionCommand
    {
        [field : SerializeField] public BuildingSO BuildingData { get; private set; }
        [field : SerializeField] public GameObject GhostPrefab { get; private set; }
        [field: SerializeField] public ConstructionRestrictionSO[] Restrictions { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not IBuildingConstructor constructor) return false;

            if (context.Hit.collider != null && context.MouseButton == MouseButton.Right)
            {
                return context.Hit.collider.TryGetComponent(out ConstructionDummy dummy)
                       && dummy.BuildingSo == BuildingData
                       && dummy.ProgressData.State == BuildingState.Paused;
            }

            return UserSupplies.Instance != null
           && UserSupplies.Instance.HasEnoughSupplies(BuildingData.Cost)
           && AllRestrictionPass(context.Hit.point);

        }



        public override void Handle(CommandContext context)
        {
            IBuildingConstructor buildingConstructor = context.Commandable as IBuildingConstructor;
            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out ConstructionDummy dummy))
            {
                buildingConstructor.ResumeConstruction(dummy);
            }
            else if (UserSupplies.Instance != null
                && UserSupplies.Instance.HasEnoughSupplies(BuildingData.Cost))
            {
                buildingConstructor.ConstructBuilding(BuildingData, context.Hit.point);
            }
        }

        public bool AllRestrictionPass(Vector3 point)
            =>Restrictions.Length == 0||Restrictions.All(restriction=>restriction.CanPlace(point));
    }
}
