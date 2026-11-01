using UnityEngine;

namespace Code.Commands
{
    /// <summary>
    /// worker유닛이 빌드를 위한 새로운 페이지를 보여주기 위해 만든 클래스.
    /// Commands : 새로운 페이지의 커맨드들이 여기에 들어간다.
    /// </summary>
    [CreateAssetMenu(fileName = "Override commands", menuName = "Units/Commands/Override commands", order = 110)]
    public class OverrideCommandsCommandSO : BaseCommandSO
    {
        [field: SerializeField] public BaseCommandSO[] Commands { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is not null;
        }

        public override void Handle(CommandContext context)
        {
            context.Commandable.SetCommandOverrides(Commands);
        }
    }
}