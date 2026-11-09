using Code.CoreSystem;
using Code.Environments;

namespace Code.GameEvents
{
    public readonly struct SupplyAcceptedEvent : IEvent
    {
        public int Amount { get; }
        public SupplySO SupplyType { get; }

        public SupplyAcceptedEvent(int amount, SupplySO supplyType)
        {
            Amount = amount;
            SupplyType = supplyType;
        }
    }
}
