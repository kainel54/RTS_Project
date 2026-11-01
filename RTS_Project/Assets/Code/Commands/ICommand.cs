namespace Code.Commands
{
    public interface ICommand
    {
        bool IsSingleUnitCommand { get; }
        bool CanHandle(CommandContext context);
        void Handle(CommandContext context);
    }
}