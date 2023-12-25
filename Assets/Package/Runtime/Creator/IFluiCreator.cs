using UnityEngine.UIElements;

namespace Flui.Creator
{
    public interface IFluiCreator
    {
        string Name { get; }
        bool Visited { get; }
        VisualElement VisualElement { get; }
        void PrepareVisit();
        void RemoveUnvisited();
    }
}
