using UnityEngine.UIElements;

namespace Flui.Binder
{
    public interface IFluiBinderRoot
    {
        TemplateContainer GetOrCreateTemplate(VisualElement element, string templateName);
    }
}