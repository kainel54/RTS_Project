using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public readonly struct BuildingCompletedEvent : IEvent
    {
        public BuildingSO Building { get; }

        public BuildingCompletedEvent(BuildingSO building) => Building = building;
    }
}
