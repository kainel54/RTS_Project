using Code.CoreSystem;
using Code.Environments;
using Code.Units;
using UnityEngine;

namespace Code.GameEvents
{
    public class SupplyEvent : IEvent
    {
        public int Amount { get; }

        public SupplySO SupplyType { get; }
        public StorageBuilding Destination { get; }

        public SupplyEvent(int amount, SupplySO supplyType, StorageBuilding destination = null)
        {
            Amount = amount;
            SupplyType = supplyType;
            Destination = destination;
        }
    }
}
