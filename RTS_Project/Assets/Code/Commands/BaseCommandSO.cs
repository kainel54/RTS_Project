using UnityEngine;

namespace Code.Commands
{
    public abstract class BaseCommandSO : ScriptableObject, ICommand
    {
        [field: SerializeField] public string CommandName { get; private set; } = "Command";
        [field: SerializeField] public Sprite CommandIcon { get; private set; }
        [field: SerializeField] public bool IsSingleUnitCommand { get; private set; }
        [field: Range(-1, 8)] 
        [field: SerializeField] public int SlotIndex { get; private set; } = 0;
        [field: SerializeField] public bool RequireClickToActivate { get; private set; } = true;        

        public abstract bool CanHandle(CommandContext context);
        public abstract void Handle(CommandContext context);
    }
}