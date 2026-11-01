using Code.Commands;
using Code.CoreSystem;

namespace Code.GameEvents
{
    public struct CommandSelectEvent : IEvent
    {
        public BaseCommandSO Command { get; }

        public CommandSelectEvent(BaseCommandSO command)
        {
            Command = command;
        }
    }
}