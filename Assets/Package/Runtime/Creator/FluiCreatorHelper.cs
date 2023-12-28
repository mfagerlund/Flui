using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public static class FluiCreatorHelper
    {
        public static string GenerateCreatorCode(VisualElement root)
        {
            if (root == null)
            {
                return "NO ROOT - make sure the game object is enabled during generation";
            }

            var sb = new StringBuilder();
            var fakeNameCounter = 0;

            if (root is TemplateContainer)
            {
                AppendChildren(root, 0);
            }
            else
            {
                Append(root, 0, new HashSet<string>());
            }

            void Append(
                VisualElement visualElement,
                int indent,
                HashSet<string> usedNames)
            {
                var type = visualElement.GetType();
                var name = GetName(visualElement, usedNames, ref fakeNameCounter);

                usedNames.Add(name);
                var classes = visualElement.GetClasses().ToList();
                classes.RemoveAll(x => x.StartsWith("unity-"));
                sb.Append($"{new string(' ', indent * 2)}.{visualElement.GetType().Name}(\"{name}\"");

                // LABEL
                if (visualElement is Label label)
                {
                    sb.Append($", _=>\"{label.text}\"");
                }
                else if (visualElement is TextElement textElement)
                {
                    sb.Append($", \"{textElement.text}\"");
                }
                else if (FluiHelper.IsBaseFieldType(visualElement))
                {
                    sb.Append($", \"{FluiHelper.GetBaseFieldLabel(visualElement)}\"");
                }
                // else if (visualElement is TextElement te)
                // {
                //     sb.Append($", \"{te.text}\"");
                // }
                //
                // else if (visualElement is Toggle toggle)
                // {
                //     sb.Append($", \"{toggle.label}\"");
                // }
                // else if (visualElement is Slider slider)
                // {
                //     sb.Append($", \"{slider.label}\"");
                // }
                // else if (visualElement is TextField textField)
                // {
                //     sb.Append($", \"{textField.label}\"");
                // }
                // else if (visualElement is IntegerField integerField)
                // {
                //     sb.Append($", \"{integerField.label}\"");
                // }

                // CLASSES
                sb.Append($", \"{classes.SJoin()}\"");

                if (type == typeof(Button))
                {
                    sb.Append(", null");
                }

                if (type == typeof(Slider))
                {
                    sb.Append(", 0, 1");
                }

                if (type == typeof(DropdownField))
                {
                    sb.Append(", new List<string>{\"a\", \"b\"}");
                }

                if (FluiHelper.IsBaseFieldType(visualElement)
                    // type == typeof(Toggle)
                    // || type == typeof(Slider)
                    // || type == typeof(TextField)
                    // || type == typeof(IntegerField)
                    // || type == typeof(FloatField)
                    // || type == typeof(DropdownField)
                   )
                {
                    sb.Append($", ctx=> ctx.{name}");
                }

                // string name,
                // string label,
                // string classes,
                // float lowValue,
                // float highValue,
                //     Func<TContext, float> getValue,
                // Action<TContext, float> setValue,

                if (visualElement.childCount == 0
                    || FluiHelper.IsBaseFieldType(visualElement))
                {
                    sb.AppendLine(")");
                }
                else
                {
                    var nameInitialLowerCase = ToValidVariableName(MakeFirstCharacterLowercase(name));
                    sb.AppendLine($", {nameInitialLowerCase}=>{nameInitialLowerCase}");
                    AppendChildren(visualElement, indent);
                    sb.AppendLine(")");
                }
            }

            void AppendChildren(VisualElement visualElement, int indent)
            {
                var newUsedNames = new HashSet<string>();
                foreach (var child in visualElement.Children())
                {
                    Append(child, indent + 1, newUsedNames);
                }
            }

            return sb.ToString();
        }
        
        private static string ToValidVariableName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "_"; // Default to underscore if input is null or empty

            StringBuilder sb = new StringBuilder();
            bool isFirstCharacter = true;

            foreach (char c in input)
            {
                if (isFirstCharacter)
                {
                    // First character must be a letter or an underscore
                    if (char.IsLetter(c))
                        sb.Append(c);
                    else
                        sb.Append('_');

                    isFirstCharacter = false;
                }
                else
                {
                    // Subsequent characters: letters, digits, or underscores
                    if (char.IsLetterOrDigit(c) || c == '_')
                        sb.Append(c);
                }
            }

            return sb.ToString();
        }


        private static string GetName(VisualElement visualElement, HashSet<string> usedNames, ref int fakeNameCounter)
        {
            var name = visualElement.name;
            if (string.IsNullOrWhiteSpace(name) && visualElement is TextElement textElement)
            {
                name = textElement.text.Replace(" ", "");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "unnamed" + fakeNameCounter++;
            }

            if (usedNames.Contains(name))
            {
                for (int i = 2; i < 1000; i++)
                {
                    var testName = name + i;
                    if (!usedNames.Contains(testName))
                    {
                        name = testName;
                        break;
                    }
                }
            }

            return name;
        }

        private static string MakeFirstCharacterLowercase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length == 1)
                return input.ToLower();

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}