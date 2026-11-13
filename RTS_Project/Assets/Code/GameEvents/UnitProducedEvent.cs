using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public readonly struct UnitProducedEvent : IEvent
    {
        public UnitSO Unit { get; }

        public UnitProducedEvent(UnitSO unit) => Unit = unit;
    }
}
