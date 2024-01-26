namespace Flui
{
    public interface IValueBinding
    {
        bool HasError { get; }
        ValueBindingChange ChangeOnLastCheck { get; }
        void Update();
    }
}