namespace Code.Units
{
    public interface ISelectable
    {
        bool IsSelected { get; }
        void Select();
        void DeSelect();
    }
}