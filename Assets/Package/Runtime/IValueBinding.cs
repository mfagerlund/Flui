namespace Flui
{
    public interface IValueBinding
    {
        bool HasError { get; }
        void Update();
    }
}