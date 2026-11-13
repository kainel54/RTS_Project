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
        [SerializeField, Min(0.1f)] private float starvationGracePeriod = 30f;
        [SerializeField, Min(0.1f)] private float starvationDamageInterval = 5f;
        [SerializeField, Min(0.1f)] private float starvationDamage = 1f;
        [field: SerializeField] public Sprite PopulationIcon { get; private set; }

        private readonly HashSet<AbstractUnit> _livingUnits = new HashSet<AbstractUnit>();
        private readonly HashSet<BaseBuilding> _buildings = new HashSet<BaseBuilding>();
        private bool _isStarving;
        private float _starvingDuration;
        private float _nextStarvationDamageAt;

        public int CurrentPopulation
        {
            get
            {
                int population = 0;
                foreach (AbstractUnit unit in _livingUnits)
                {
                    MeleeCombat combat = unit != null ? unit.GetComponent<MeleeCombat>() : null;
                    if (unit != null && (combat == null || combat.Faction == MeleeCombat.Team.Player)
                        && unit.AbstractUnitData is UnitSO unitData)
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
                        if (unit != null && !building.ConsumesAssignedWorkerFor(unit))
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

        private void Update()
        {
            if (!_isStarving) return;

            if (CurrentPopulation <= 0 || (UserSupplies.Instance != null && UserSupplies.Instance.FoodAmount > 0))
            {
                _isStarving = false;
                _starvingDuration = 0f;
                return;
            }

            _starvingDuration += Time.deltaTime;
            while (_starvingDuration >= _nextStarvationDamageAt)
            {
                DamagePlayerUnitsFromStarvation();
                _nextStarvationDamageAt += starvationDamageInterval;
            }
        }

        private System.Collections.IEnumerator ConsumeFoodOverTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(foodConsumptionInterval);

                int foodCost = CurrentPopulation * foodPerPopulationPerInterval;
                if (foodCost <= 0)
                {
                    _isStarving = false;
                    _starvingDuration = 0f;
                    continue;
                }

                UserSupplies supplies = UserSupplies.Instance;
                if (supplies == null) continue;

                supplies.ConsumeFood(foodCost);
                if (supplies.FoodAmount <= 0)
                    BeginStarvation();
                else
                {
                    _isStarving = false;
                    _starvingDuration = 0f;
                }
            }
        }

        private void BeginStarvation()
        {
            if (_isStarving) return;

            _isStarving = true;
            _starvingDuration = 0f;
            _nextStarvationDamageAt = starvationGracePeriod;
        }

        private void DamagePlayerUnitsFromStarvation()
        {
            foreach (AbstractUnit unit in _livingUnits)
            {
                if (unit == null) continue;

                MeleeCombat combat = unit.GetComponent<MeleeCombat>();
                if (combat != null && combat.Faction == MeleeCombat.Team.Player && !combat.IsDead)
                    combat.TakeDamage(starvationDamage);
            }
        }

        public bool CanQueue(UnitSO unit)
        {
            if (unit == null) return false;

            int requestedPopulation = Mathf.Max(0, unit.PopulationCost);
            return CurrentPopulation + QueuedPopulation + requestedPopulation <= MaximumPopulation;
        }

        public bool CanQueue(UnitSO unit, BaseBuilding building)
        {
            if (unit == null || building == null) return false;
            if (building.ConsumesAssignedWorkerFor(unit))
                return building.CanQueueUnit(unit);
            return building.CanQueueUnit(unit) && CanQueue(unit);
        }

        public void RegisterBuilding(BaseBuilding building)
        {
            if (building != null && _buildings.Add(building))
            {
                NotifyPopulationChanged();
                Bus<BuildingCompletedEvent>.Raise(new BuildingCompletedEvent(building.AbstractUnitData as BuildingSO));
            }
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
