namespace Flui.Builder
{
    public static class FluiBuilderStats
    {
        public static void Reset()
        {
            FluisCreated = 0;
            FluisRemoved = 0;
            FluisDestroyed = 0;
            UnparentedVisualElementsRemoved = 0;
            ValueBindingStats.Reset();
        }

        public static int FluisCreated { get; set; }
        public static int FluisRemoved { get; set; }
        public static int FluisDestroyed { get; set; }
        public static int UnparentedVisualElementsRemoved { get; set; }
    }
}