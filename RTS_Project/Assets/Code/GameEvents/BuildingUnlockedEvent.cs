using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public readonly struct BuildingUnlockedEvent : IEvent
    {
        public BuildingSO Building { get; }

        public BuildingUnlockedEvent(BuildingSO building) => Building = building;
    }
}
