using Code.Units;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Cancel construction", menuName = "Units/Commands/Cancel construction", order = 0)]
    public class CancelConstructionCommandSO : BaseCommandSO
    {
        public override bool CanHandle(CommandContext context)
        {
            return context is { Commandable: IBuildingConstructor, MouseButton: MouseButton.Left };
        }

        public override void Handle(CommandContext context)
        {
            IBuildingConstructor constructor = context.Commandable as IBuildingConstructor;
            constructor.CancelConstruction();
        }
    }
}