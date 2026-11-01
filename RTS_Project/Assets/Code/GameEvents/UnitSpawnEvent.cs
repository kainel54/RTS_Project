using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public struct UnitSpawnEvent : IEvent
    {
        public AbstractUnit Unit { get; }

        public UnitSpawnEvent(AbstractUnit unit)
        {
            Unit = unit;
        }
    }
}