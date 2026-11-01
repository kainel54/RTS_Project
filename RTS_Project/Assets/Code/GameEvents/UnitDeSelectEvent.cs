using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public struct UnitDeSelectEvent : IEvent
    {
        public ISelectable Unit { get; }

        public UnitDeSelectEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}