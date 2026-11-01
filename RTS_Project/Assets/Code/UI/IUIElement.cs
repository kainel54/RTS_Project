namespace Code.UI
{
    public interface IUIElement<in T>
    {
        void EnableFor(T item);
        void Disable();
    }
    
    public interface IUIElement<in T1, in T2>
    {
        void EnableFor(T1 item, T2 callbackAction);
        void Disable();
    }
    
    public interface IUIElement<in T1, in T2, in T3>
    {
        void EnableFor(T1 item, T2 context, T3 callbackAction);
        void Disable();
    }
}