namespace Code.Environments
{
    public interface IGatherable
    {
        SupplySO SupplyData { get; }
        int Amount { get; }
        bool IsBusy { get; }

        bool BeginGather();
        int EndGather();
        void AbortGather();
    }
}