using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

namespace Flui.Builder
{
    [RequireComponent(typeof(FluiBuilderDocumentSource))]
    public class FluiBuilderDocument : MonoBehaviour
    {
        [FormerlySerializedAs("_fluiDocumentSource")] [SerializeField] private FluiBuilderDocumentSource fluiBuilderDocumentSource;
  

        // private void OnValidate()
        // {
        //     if (Application.isPlaying) return;
        //     StartCoroutine(Generate());
        // }
        //
        // private void Update()
        // {
        //     StartCoroutine(Generate());
        // }
        //
        // private FluiRoot<Square, VisualElement> _flui;
        //
        // IEnumerator Generate()
        // {
        //     if (pause)
        //     {
        //         yield break;
        //     }
        //
        //     yield return null;
        //
        //     var root = document.rootVisualElement;
        //     // root.Clear();
        //     root.styleSheets.Add(styleSheet);
        //
        //     if (_flui == null || rebuild)
        //     {
        //         _flui = new FluiRoot<Square, VisualElement>();
        //         rebuild = false;
        //     }
        //
        //     _flui.BuildGui(square, root, fl => fl
        //         .VisualElement("container", "container", container => container
        //             .VisualElement("viewBox", "view-box, bordered-box")
        //             .VisualElement("controlBox", "control-box, bordered-box", cb => cb
        //                 .Button("spinButton", "NO", "control-box, bordered-box", ctx => ctx.Spin(), updateAction: b => { b.Element.text = spinButtonTitle; })
        //                 .Slider("slider", "", "", 0.25f, 5f, x => x.targetScale, (sq, value) => sq.targetScale = value)
        //                 .Label("label", c => $"{c.targetScale:0.00}", "label")
        //             )
        //             .VisualElement("controlBox2", "control-box, bordered-box, row", cb => cb
        //                 .Toggle("showTimeToggle", "Show Time", "", ctx => ctx.showTime, (ctx, value) => ctx.showTime = value)
        //                 .TextField("nonsenseText", "Nonsense Text", "", ctx => ctx.nonsenseText, (ctx, value) => ctx.nonsenseText = value)
        //             )
        //             .VisualElement("statBox", "bordered-box, stat-box", sb => sb
        //                 .VisualElement("left", "column", l => l
        //                     .Label("created", _ => $"Created: {FluiStats.FluisCreated}", "")
        //                     .Label("removed", _ => $"Removed: {FluiStats.FluisRemoved}", "")
        //                 )
        //                 .VisualElement("right", "column", r => r
        //                     .Label("destroyed", _ => $"Destroyed: {FluiStats.FluisDestroyed}", "")
        //                     .Label("unparented", _ => $"Unparented Removed: {FluiStats.UnparentedVisualElementsRemoved}", "")
        //                 )
        //             )
        //             .Optional(c => c.showTime, cnt => cnt
        //                 .VisualElement("time", "control-box, bordered-box", cb => cb
        //                     .Label("label", c => $"{DateTime.Now:HH:mm:ss}", "label")
        //                 )
        //             )
        //         )
        //     );
        // }
    }
}