using Code.CoreSystem;
using Code.Units;

namespace Code.GameEvents
{
    public readonly struct WorkerAssignedEvent : IEvent
    {
        public BuildingSO Building { get; }

        public WorkerAssignedEvent(BuildingSO building) => Building = building;
    }
}
