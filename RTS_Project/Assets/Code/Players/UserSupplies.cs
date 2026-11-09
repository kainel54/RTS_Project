using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using Code.Units;
using System;
using UnityEngine;

namespace Code.Players
{
    public class UserSupplies : MonoBehaviour
    {
        private const int DebugSupplyAmount = 100;

        public static UserSupplies Instance { get; private set; }

        [SerializeField] private SupplySO woodSO;
        [SerializeField] private SupplySO stoneSO;
        [SerializeField] private SupplySO ironOreSO;
        [SerializeField] private SupplySO ironIngotSO;
        [SerializeField] private SupplySO foodSO;

        [field: SerializeField] public int WoodAmount { get; private set; }
        [field: SerializeField] public int StoneAmount { get; private set; }
        [field: SerializeField] public int IronOreAmount { get; private set; }
        [field: SerializeField] public int IronIngotAmount { get; private set; }
        [field: SerializeField] public int FoodAmount { get; private set; }

        public int TotalStorageCapacity
        {
            get
            {
                int capacity = 0;
                foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
                {
                    if (storage != null)
                        capacity += storage.Capacity;
                }

                return capacity;
            }
        }

        public int UsedStorageCapacity
        {
            get
            {
                int used = 0;
                foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
                {
                    if (storage != null)
                        used += storage.UsedCapacity;
                }

                return used;
            }
        }

        public delegate void SupplyChanged(int amount, SupplySO supplyType);
        public event SupplyChanged OnSupplyChanged; 
        public event Action<int, int> OnStorageCapacityChanged;

        private void Awake()
        {
            if(Instance != null&& Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            WoodAmount = StoneAmount = IronOreAmount = IronIngotAmount = FoodAmount = 0;

            Bus<SupplyEvent>.OnEvent += HandleSupplyEvent;
        }

        private void Start()
        {
            Bus<UserSupplyReadyEvent>.Raise(new UserSupplyReadyEvent(this));
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F1)) AddDebugSupply(woodSO);
            if (Input.GetKeyDown(KeyCode.F2)) AddDebugSupply(stoneSO);
            if (Input.GetKeyDown(KeyCode.F3)) AddDebugSupply(ironOreSO);
            if (Input.GetKeyDown(KeyCode.F4)) AddDebugSupply(ironIngotSO);
            if (Input.GetKeyDown(KeyCode.F5)) AddDebugSupply(foodSO);
#endif
        }

        private void AddDebugSupply(SupplySO supply)
        {
            if (supply != null)
                Bus<SupplyEvent>.Raise(new SupplyEvent(DebugSupplyAmount, supply));
        }

        private void OnDestroy()
        {
            Bus<SupplyEvent>.OnEvent -= HandleSupplyEvent;
        }

        private void HandleSupplyEvent(SupplyEvent evt)
        {
            if (evt.Amount <= 0 || evt.SupplyType == null) return;

            SupplySO configuredSupply = GetConfiguredSupply(evt.SupplyType);
            if (configuredSupply == null) return;

            int acceptedAmount = StoreSupply(evt.Amount, evt.Destination);
            if (acceptedAmount <= 0) return;

            AddSupplyAmount(configuredSupply, acceptedAmount);
            OnSupplyChanged?.Invoke(GetSupplyAmount(configuredSupply), configuredSupply);
            Bus<SupplyAcceptedEvent>.Raise(new SupplyAcceptedEvent(acceptedAmount, configuredSupply));
        }

        private SupplySO GetConfiguredSupply(SupplySO supply)
        {
            if (supply == woodSO) return woodSO;
            if (supply == stoneSO) return stoneSO;
            if (supply == ironOreSO) return ironOreSO;
            if (supply == ironIngotSO) return ironIngotSO;
            if (supply == foodSO) return foodSO;
            return null;
        }

        private int StoreSupply(int amount, StorageBuilding destination)
        {
            int stored = 0;
            if (destination != null)
                stored = destination.StoreReservedCapacity(amount);

            if (stored < amount)
                stored += StoreAcrossAvailableStorage(amount - stored);

            return stored;
        }

        private int StoreAcrossAvailableStorage(int amount)
        {
            int stored = 0;
            foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
            {
                if (storage == null || stored >= amount) continue;
                stored += storage.StoreUnreservedCapacity(amount - stored);
            }

            return stored;
        }

        private void AddSupplyAmount(SupplySO supply, int amount)
        {
            if (supply == woodSO) WoodAmount += amount;
            else if (supply == stoneSO) StoneAmount += amount;
            else if (supply == ironOreSO) IronOreAmount += amount;
            else if (supply == ironIngotSO) IronIngotAmount += amount;
            else if (supply == foodSO) FoodAmount += amount;
        }

        private int GetSupplyAmount(SupplySO supply)
        {
            if (supply == woodSO) return WoodAmount;
            if (supply == stoneSO) return StoneAmount;
            if (supply == ironOreSO) return IronOreAmount;
            if (supply == ironIngotSO) return IronIngotAmount;
            if (supply == foodSO) return FoodAmount;
            return 0;
        }

        public void NotifyStorageCapacityChanged()
        {
            OnStorageCapacityChanged?.Invoke(UsedStorageCapacity, TotalStorageCapacity);
        }

        private void ReleaseStorageCapacity(int amount)
        {
            int remaining = Mathf.Max(0, amount);
            foreach (StorageBuilding storage in StorageBuilding.ActiveStorages)
            {
                if (storage == null || remaining <= 0) continue;
                remaining -= storage.RemoveStoredCapacity(remaining);
            }
        }

        public bool HasEnoughSupplies(SupplyCostSO cost)
        {
            return cost != null
                   && WoodAmount >= cost.Wood
                   && StoneAmount >= cost.Stone
                   && IronOreAmount >= cost.IronOre
                   && IronIngotAmount >= cost.IronIngot
                   && FoodAmount >= cost.Food;
        }

        public bool TrySpendSupplies(SupplyCostSO cost)
        {
            if (!HasEnoughSupplies(cost)) return false;

            WoodAmount -= cost.Wood;
            StoneAmount -= cost.Stone;
            IronOreAmount -= cost.IronOre;
            IronIngotAmount -= cost.IronIngot;
            FoodAmount -= cost.Food;

            ReleaseStorageCapacity(cost.Wood + cost.Stone + cost.IronOre + cost.IronIngot + cost.Food);

            if (cost.Wood > 0) OnSupplyChanged?.Invoke(WoodAmount, woodSO);
            if (cost.Stone > 0) OnSupplyChanged?.Invoke(StoneAmount, stoneSO);
            if (cost.IronOre > 0) OnSupplyChanged?.Invoke(IronOreAmount, ironOreSO);
            if (cost.IronIngot > 0) OnSupplyChanged?.Invoke(IronIngotAmount, ironIngotSO);
            if (cost.Food > 0) OnSupplyChanged?.Invoke(FoodAmount, foodSO);

            return true;
        }

        public int ConsumeFood(int requestedAmount)
        {
            if (requestedAmount <= 0 || FoodAmount <= 0) return 0;

            int consumedAmount = Mathf.Min(requestedAmount, FoodAmount);
            FoodAmount -= consumedAmount;
            ReleaseStorageCapacity(consumedAmount);
            OnSupplyChanged?.Invoke(FoodAmount, foodSO);
            return consumedAmount;
        }

    }
}
