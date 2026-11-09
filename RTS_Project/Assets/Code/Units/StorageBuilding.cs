using System.Collections.Generic;
using Code.Players;
using UnityEngine;

namespace Code.Units
{
    public class StorageBuilding : BaseBuilding
    {
        private static readonly HashSet<StorageBuilding> ActiveStorageSet = new();
        private int _storedAmount;
        private int _reservedAmount;

        public static IEnumerable<StorageBuilding> ActiveStorages => ActiveStorageSet;
        public int Capacity => AbstractUnitData is BuildingSO buildingData ? buildingData.StorageCapacity : 0;
        public int StoredAmount => _storedAmount;
        public int ReservedAmount => _reservedAmount;
        public int UsedCapacity => _storedAmount + _reservedAmount;
        public int AvailableCapacity => Mathf.Max(0, Capacity - UsedCapacity);

        private void OnEnable()
        {
            ActiveStorageSet.Add(this);
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
        }

        private void OnDisable()
        {
            ActiveStorageSet.Remove(this);
            _reservedAmount = 0;
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
        }

        public bool TryReserveCapacity(int amount)
        {
            if (amount <= 0 || AvailableCapacity < amount) return false;

            _reservedAmount += amount;
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
            return true;
        }

        public void ReleaseReservedCapacity(int amount)
        {
            if (amount <= 0 || _reservedAmount <= 0) return;

            _reservedAmount = Mathf.Max(0, _reservedAmount - amount);
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
        }

        public int StoreReservedCapacity(int amount)
        {
            if (amount <= 0 || _reservedAmount <= 0) return 0;

            int stored = Mathf.Min(amount, _reservedAmount);
            _reservedAmount -= stored;
            _storedAmount += stored;
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
            return stored;
        }

        public int StoreUnreservedCapacity(int amount)
        {
            int stored = Mathf.Min(Mathf.Max(0, amount), AvailableCapacity);
            if (stored <= 0) return 0;

            _storedAmount += stored;
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
            return stored;
        }

        public int RemoveStoredCapacity(int amount)
        {
            int removed = Mathf.Min(Mathf.Max(0, amount), _storedAmount);
            if (removed <= 0) return 0;

            _storedAmount -= removed;
            UserSupplies.Instance?.NotifyStorageCapacityChanged();
            return removed;
        }
    }
}
