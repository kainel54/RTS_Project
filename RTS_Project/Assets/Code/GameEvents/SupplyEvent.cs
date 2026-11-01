using Code.CoreSystem;
using Code.Environments;
using UnityEngine;

namespace Code.GameEvents
{
    public class SupplyEvent : IEvent
    {
        public int Amount { get; }

        public SupplySO SupplyType { get; }
        public SupplyEvent(int amount,SupplySO supplyType)
        {
            Amount = amount;
            SupplyType = supplyType;
        }
    }
}