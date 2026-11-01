using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Stop command", menuName = "Units/Commands/Stop", order = 101)]
    public class StopCommandSO : BaseCommandSO
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = context.Commandable as AbstractUnit;
            unit.Stop();
        }
    }
}