using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public readonly struct BuildingDestroyedEvent : IEvent
    {
        public BaseBuilding Building { get; }

        public BuildingDestroyedEvent(BaseBuilding building) => Building = building;
    }
}
