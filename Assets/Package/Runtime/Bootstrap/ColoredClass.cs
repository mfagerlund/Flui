using System;
using UnityEngine;

namespace Flui.Bootstrap
{
    [Serializable]
    public class ColoredClass
    {
        [field: SerializeField] public bool enabled = true;
        [field: SerializeField] public string className;
        [field: SerializeField] public Color color;
    }
}