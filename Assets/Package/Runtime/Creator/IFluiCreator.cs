using UnityEngine.UIElements;

namespace Flui.Creator
{
    public interface IFluiCreator
    {
        string Name { get; }
        bool Visited { get; }
        VisualElement VisualElement { get; }
        object Context { get; }
        void PrepareVisit();
        void RemoveUnvisited();
        int GetHierarchyChildCount();
    }
}
