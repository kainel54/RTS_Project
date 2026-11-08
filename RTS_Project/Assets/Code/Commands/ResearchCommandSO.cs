using Code.Research;
using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Research command", menuName = "Buildings/Commands/Research")]
    public class ResearchCommandSO : BaseCommandSO
    {
        [field: SerializeField] public ResearchSO Research { get; private set; }

        public override bool CanHandle(CommandContext context)
        {
            return Research != null
                   && ResearchManager.Instance != null
                   && context.Commandable is BaseBuilding library
                   && ResearchManager.Instance.CanStartResearch(Research, library);
        }

        public override void Handle(CommandContext context)
        {
            if (context.Commandable is BaseBuilding library)
                ResearchManager.Instance?.TryStartResearch(Research, library);
        }
    }
}
