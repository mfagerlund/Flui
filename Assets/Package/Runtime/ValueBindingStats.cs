namespace Flui
{
    public static class ValueBindingStats
    {
        public static int BindingSetViewValueCount { get; set; }
        public static int BindingSetModelValueCount { get; set; }

        public static string Describe() => $"Value Binding: View Update={BindingSetViewValueCount} | Model Update={BindingSetModelValueCount}";
        
        public static void Reset()
        {
            BindingSetViewValueCount = 0;
            BindingSetModelValueCount = 0;
        }
    }
}