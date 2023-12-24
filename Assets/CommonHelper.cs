using System;
using System.Collections.Generic;
using Flui;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap
{
    public static class CommonHelper
    {
        public static void FadeIn(MonoBehaviour monoBehaviour, VisualElement visualElement)
        {
            var wasPickable = DisableInteratability(visualElement);
            visualElement.RemoveFromClassList("fade-in");
            visualElement.AddToClassList("fade-out-instant");
            monoBehaviour.RunDelayed(1, () =>
            {
                FluiHelper.ExecuteAfterClassTransition(
                    visualElement,
                    "fade-in",
                    "opacity",
                    () =>
                    {
                        ResetInteratability(visualElement, wasPickable);
                    });
            });
        }

        public static void FadeOut(
            MonoBehaviour monoBehaviour,
            VisualElement visualElement,
            Action actionAfterFadeOut)
        {
            var wasPickable = DisableInteratability(visualElement);
            visualElement.RemoveFromClassList("fade-out-instant");
            visualElement.RemoveFromClassList("fade-in");
            monoBehaviour.RunDelayed(1, () =>
            {
                FluiHelper.ExecuteAfterClassTransition(
                    visualElement,
                    "fade-out",
                    "opacity",
                    () =>
                    {
                        ResetInteratability(visualElement, wasPickable);
                        actionAfterFadeOut();
                    });
            });
        }

        public static void ForEachInHierarchy(VisualElement parent, Action<VisualElement> action)
        {
            action(parent);
            foreach (var child in parent.Children())
            {
                ForEachInHierarchy(child, action);
            }
        }

        public static void ResetInteratability(VisualElement parent, HashSet<VisualElement> werePickable)
        {
            ForEachInHierarchy(parent, x =>
            {
                if (werePickable.Contains(x))
                {
                    x.pickingMode = PickingMode.Position;
                }
            });
        }

        public static HashSet<VisualElement> DisableInteratability(VisualElement parent)
        {
            HashSet<VisualElement> werePickable = new();
            ForEachInHierarchy(parent, x =>
            {
                if (x.pickingMode == PickingMode.Position)
                {
                    werePickable.Add(x);
                    x.pickingMode = PickingMode.Ignore;
                }
            });
            return werePickable;
        }
    }
}