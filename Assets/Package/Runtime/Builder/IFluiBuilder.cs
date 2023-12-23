using UnityEngine.UIElements;

namespace Flui.Builder
{
    public interface IFluiBuilder
    {
        string Name { get; }
        bool Visited { get; }
        VisualElement VisualElement { get; }
        void PrepareVisit();
        void RemoveUnvisited();
    }
}
