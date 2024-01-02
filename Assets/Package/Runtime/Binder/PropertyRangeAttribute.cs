using System;

namespace Flui.Binder
{
    public sealed class PropertyRangeAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get;  }
        
        public PropertyRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}