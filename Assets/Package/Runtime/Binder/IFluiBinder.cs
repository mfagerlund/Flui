using UnityEngine.UIElements;

namespace Flui.Binder
{
    public interface IFluiBinder
    {
        bool Visited { get; }
        VisualElement VisualElement { get; }
        void PrepareVisit();
        void RemoveUnvisited();
    }
}