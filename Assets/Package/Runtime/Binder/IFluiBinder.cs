using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public interface IFluiBinder
    {
        bool Visited { get; }
        bool Visible { get; }
        bool Hidden { get; }
        bool Invisible { get; }
        VisualElement VisualElement { get; }
        string Query { get;  }
        object Context { get; }
        void PrepareVisit();
        void RemoveUnvisited();
        IEnumerable<IFluiBinder> GetChildren();
        int GetHierarchyChildCount();
    }
}