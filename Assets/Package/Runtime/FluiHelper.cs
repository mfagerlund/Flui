using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui
{
    public static class FluiHelper
    {
        public static void ExecuteAfterClassTransition(
            VisualElement visualElement,
            string @class,
            string expectedPropertyName,
            Action action)
        {
            visualElement.AddToClassList(@class);
            visualElement.RegisterCallback<TransitionEndEvent>(Callback);

            void Callback(TransitionEndEvent e)
            {
                if (e.stylePropertyNames.Contains(expectedPropertyName))
                {
                    action();
                    visualElement.UnregisterCallback<TransitionEndEvent>(Callback);
                }
            }
        }
        
        public static void RunDelayed(this MonoBehaviour monoBehaviour, int frameCount, Action action)
        {
            monoBehaviour.StartCoroutine(Coroutine());
            IEnumerator Coroutine()
            {
                for (int i = 0; i <= frameCount; i++)
                {
                    yield return 0;    
                }
                
                action();
            }
        }    
    }
}