using Code.Units;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Code.Commands
{
    public struct CommandContext
    {
        public AbstractCommandable Commandable { get; }
        public RaycastHit Hit { get; }
        public int UnitIndex { get; }
        public MouseButton MouseButton { get; }

        public CommandContext(AbstractCommandable commandable, RaycastHit hit,
            int unitIndex = 0, MouseButton button = MouseButton.Left)
        {
            Commandable = commandable;
            Hit = hit;
            UnitIndex = unitIndex;
            MouseButton = button;
        }
    }
}