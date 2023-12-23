// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Flui.Bootstrap
{
    public class BootstrapBuilder : MonoBehaviour
    {
        [SerializeField] private List<ColoredClass> _coloredClasses = new()
        {
            new ColoredClass { className = "primary", color = new SimpleColor("#007BFF").ToColor() },
            new ColoredClass { className = "secondary", color = new SimpleColor("#6C757D").ToColor() },
            new ColoredClass { className = "success", color = new SimpleColor("#28A745").ToColor() },
            new ColoredClass { className = "info", color = new SimpleColor("#17A2B8").ToColor() },
            new ColoredClass { className = "warning", color = new SimpleColor("#FFC107").ToColor() },
            new ColoredClass { className = "danger", color = new SimpleColor("#DC3545").ToColor() },
        };

        [SerializeField] private bool _generate;

        [SerializeField, TextArea(0, 10)] private string _buttonClasses;
        [SerializeField, TextArea(0, 10)] private string _textClasses;
        [SerializeField, TextArea(0, 10)] private string _tableClasses;

        [SerializeField, TextArea(0, 10)] private string _tableTemplate = @".row.primary { background-color: #B8DAFF; }
.th.primary { background-color: #B8DAFF; border-top-color: #7ABAFF; }
.td.primary { background-color: #B8DAFF; border-top-color: #7ABAFF; }
";

        [SerializeField, TextArea(0, 10)] private string _buttonTemplate =
            @"/* btn-primary */

VisualElement {
  %variables%
}

.btn-primary {
    color: white;
    background-color: var(--btn-primary-normal);
    border-color: var(--btn-primary-normal);
    border-radius: var(--button-border-radius);
    border-width: var(--button-border-width);
    transition: 0.25s all;
    height: 30px;
}
.btn-primary:focus {
    background-color: var(--btn-primary-hovered);
    border-color: var(--btn-primary-border);
}

.btn-primary:active, .btn-primary:checked  {
    background-color: var(--btn-primary-clicked);
    border-color: var(--btn-primary-border);
}

.btn-primary:disabled, .btn-primary.disabled {
    background-color: var(--btn-primary-disabled);
    border-color: var(--btn-primary-disabled);
}

.btn-primary.disabled:focus, .btn-primary.disabled:active,
.btn-primary:disabled:focus, .btn-primary:disabled:active {
    border-color: var(--btn-primary-disabled-border);
}";

        private void OnValidate()
        {
            if (_generate)
            {
                _generate = false;
                Generate();
            }
        }

        private void Generate()
        {
            GenerateTextClasses();
            GenerateButtonClasses();
            GenerateTableClasses();
        }

        private void GenerateTableClasses()
        {
            _tableClasses = "";
            foreach (var coloredClass in _coloredClasses.Where(x => x.enabled))
            {
                var normal = new SimpleColor(coloredClass.color);

                _tableClasses +=
                    _tableTemplate
                        .Replace("primary", coloredClass.className)
                        // Background
                        .Replace("#B8DAFF", HslColor.ScaleHsl(normal, 1, 0.6f, 1.72f).ToString())
                        // Border
                        .Replace("#7ABAFF", HslColor.ScaleHsl(normal, 1, 0.8f, 1.48f).ToString()) +
                    "\n";
            }
        }

        public string ColorToHexString(Color color) => color.ToHexString().Substring(0, 6);
    
    
        private void GenerateTextClasses()
        {
            _textClasses = "";
            foreach (var coloredClass in _coloredClasses.Where(x => x.enabled))
            {
                _textClasses += $".text-{coloredClass.className} {{ color: {ColorToHexString(coloredClass.color)} !important; }}\n";
            }
        }

        private void GenerateButtonClasses()
        {
            _buttonClasses = "";
            foreach (var coloredClass in _coloredClasses.Where(x => x.enabled))
            {
                var sc = new SimpleColor(coloredClass.color);
                var colorPack = ColorPack.Create(sc);
                // --btn-warning-normal: #FFC107;
                _buttonClasses +=
                    _buttonTemplate
                        .Replace("%variables%", colorPack.ToVariableString("--btn-" + coloredClass.className))
                        .Replace("btn-primary", "btn-" + coloredClass.className);
            }
        }
    }
}