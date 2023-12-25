// // ReSharper disable InconsistentNaming
//
// using System.Text;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace Flui.Creator
// {
//     public class FluiBuilderDocumentSource : MonoBehaviour
//     {
//         private UIDocument _document;
//         [SerializeField] private StyleSheet[] _styleSheets;
//
//         [SerializeField] private bool _generateCode;
//         [SerializeField, TextArea(20, 50)] private string _code;
//
//         private void Awake()
//         {
//             GenerateCode();
//         }
//
//         private void OnValidate()
//         {
//             if (_generateCode)
//             {
//                 _generateCode = false;
//                 GenerateCode();
//             }
//         }
//
//         private void GenerateCode()
//         {
//             var root = _document.rootVisualElement;
//             Debug.Log(root.childCount);
//
//             var sb = new StringBuilder();
//
//             var fakeNameCounter = 0;
//             Append(root, 0);
//             void Append(VisualElement ve, int indent)
//             {
//                 var name = !string.IsNullOrWhiteSpace(ve.name) ? ve.name : ("unnamed" + fakeNameCounter++.ToString());
//                 
//                 sb.Append($"{new string(' ', indent * 2)}.{ve.GetType().Name}(\"{name}\", \"{ve.GetClasses().SJoin()}\"");
//                 if (ve.childCount == 0)
//                 {
//                     sb.AppendLine(")");
//                 }
//                 else
//                 {
//                     sb.AppendLine($", {name}=>{name}");
//                     foreach (var child in ve.Children())
//                     {
//                         Append(child, indent + 1);
//                     }
//
//                     sb.Append(")");
//                 }
//             }
//
//             _code = sb.ToString();
//         }
//     }
// }