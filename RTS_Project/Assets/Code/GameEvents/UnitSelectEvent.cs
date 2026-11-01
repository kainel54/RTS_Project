using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public struct UnitSelectEvent : IEvent
    {
        public ISelectable Unit { get; private set; }

        public UnitSelectEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}