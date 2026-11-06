using System;
using System.Collections.Generic;
using Code.CoreSystem;
using Code.GameEvents;
using Code.Players;
using UnityEngine;

namespace Code.Units
{
    public class PopulationSystem : MonoBehaviour
    {
        public static PopulationSystem Instance { get; private set; }

        [SerializeField, Min(0)] private int startingPopulationCap = 5;
        [SerializeField, Min(0.1f)] private float foodConsumptionInterval = 10f;
        [SerializeField, Min(0)] private int foodPerPopulationPerInterval = 1;
        [field: SerializeField] public Sprite PopulationIcon { get; private set; }

        private readonly HashSet<AbstractUnit> _livingUnits = new HashSet<AbstractUnit>();
        private readonly HashSet<BaseBuilding> _buildings = new HashSet<BaseBuilding>();

        public int CurrentPopulation
        {
            get
            {
                int population = 0;
                foreach (AbstractUnit unit in _livingUnits)
                {
                    if (unit != null && unit.AbstractUnitData is UnitSO unitData)
                        population += Mathf.Max(0, unitData.PopulationCost);
                }

                return population;
            }
        }
        public int AvailablePopulation => Mathf.Max(0, MaximumPopulation - CurrentPopulation - QueuedPopulation);
        public int MaximumPopulation
        {
            get
            {
                int maximum = startingPopulationCap;
                foreach (BaseBuilding building in _buildings)
                {
                    if (building != null && building.AbstractUnitData is BuildingSO buildingData)
                        maximum += Mathf.Max(0, buildingData.PopulationBonus);
                }

                return maximum;
            }
        }

        public int QueuedPopulation
        {
            get
            {
                int queued = 0;
                foreach (BaseBuilding building in _buildings)
                {
                    if (building == null) continue;

                    foreach (UnitSO unit in building.UnitQueue)
                    {
                        if (unit != null)
                            queued += Mathf.Max(0, unit.PopulationCost);
                    }
                }

                return queued;
            }
        }

        public event Action<int, int> PopulationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
            Bus<UnitDeSpawnEvent>.OnEvent += HandleUnitDeSpawn;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
            Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
            Instance = null;
        }

        private void Start()
        {
            StartCoroutine(ConsumeFoodOverTime());
        }

        private System.Collections.IEnumerator ConsumeFoodOverTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(foodConsumptionInterval);

                int foodCost = CurrentPopulation * foodPerPopulationPerInterval;
                if (foodCost > 0 && UserSupplies.Instance != null)
                    UserSupplies.Instance.ConsumeFood(foodCost);
            }
        }

        public bool CanQueue(UnitSO unit)
        {
            if (unit == null) return false;

            int requestedPopulation = Mathf.Max(0, unit.PopulationCost);
            return CurrentPopulation + QueuedPopulation + requestedPopulation <= MaximumPopulation;
        }

        public void RegisterBuilding(BaseBuilding building)
        {
            if (building != null && _buildings.Add(building))
                NotifyPopulationChanged();
        }

        public void UnregisterBuilding(BaseBuilding building)
        {
            if (building != null && _buildings.Remove(building))
                NotifyPopulationChanged();
        }

        private void HandleUnitSpawn(UnitSpawnEvent evt)
        {
            if (evt.Unit != null && _livingUnits.Add(evt.Unit))
                NotifyPopulationChanged();
        }

        private void HandleUnitDeSpawn(UnitDeSpawnEvent evt)
        {
            if (evt.Unit != null && _livingUnits.Remove(evt.Unit))
                NotifyPopulationChanged();
        }

        private void NotifyPopulationChanged()
        {
            PopulationChanged?.Invoke(CurrentPopulation, MaximumPopulation);
        }
    }
}
