using System;
using Flui.Creator;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    [ExecuteAlways]
    public class FluiBinderStatsUi : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] private string _labelName = "FluiBinderStats";

        private FluiBinderRoot<FluiBinderStatsUi, VisualElement> _root;

        private void Update()
        {
            if (_document == null)
            {
                _document = GetComponentInParent<UIDocument>();
            }

            if (_document == null)
            {
                Debug.LogError("UIDocument not found on FluiBinderStatsUi");
                return;
            }

            _root ??= new FluiBinderRoot<FluiBinderStatsUi, VisualElement>();
            _root.BindGui(
                this, _document.rootVisualElement,
                x => x.Label(_labelName, ctx => 
                    FluiBinderStats.Describe()+Environment.NewLine+
                    FluiCreatorStats.Describe()+Environment.NewLine+
                    ValueBindingStats.Describe()));
        }
    }
}