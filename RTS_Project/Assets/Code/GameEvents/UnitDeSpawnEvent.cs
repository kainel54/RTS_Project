using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public struct UnitDeSpawnEvent : IEvent
    {
        public AbstractUnit Unit { get; }

        public UnitDeSpawnEvent(AbstractUnit unit)
        {
            Unit = unit;
        }
    }
}