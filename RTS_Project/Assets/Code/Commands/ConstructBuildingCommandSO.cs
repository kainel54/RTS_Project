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
        public override SupplyCostSO GetSupplyCost() => BuildingData != null ? BuildingData.Cost : null;

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
            if (context.Commandable is not IBuildingConstructor buildingConstructor || BuildingData == null)
                return;

            if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out ConstructionDummy dummy))
            {
                buildingConstructor.ResumeConstruction(dummy);
                return;
            }

            if (BuildingData.ConstructionDummy == null
                || !BuildingData.ConstructionDummy.TryGetComponent<ConstructionDummy>(out _))
            {
                Debug.LogError($"{BuildingData.name}: ConstructionDummy prefab is missing its ConstructionDummy component.");
                return;
            }

            if (UserSupplies.Instance == null)
                return;

            bool suppliesSpent = UserSupplies.Instance.TrySpendSupplies(BuildingData.Cost);
            if (!suppliesSpent) return;

            buildingConstructor.ConstructBuilding(BuildingData, context.Hit.point);
        }

        public bool AllRestrictionPass(Vector3 point)
            =>Restrictions.Length == 0||Restrictions.All(restriction=>restriction.CanPlace(point));
    }
}
