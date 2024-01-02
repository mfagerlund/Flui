namespace Flui.Creator
{
    public static class FluiCreatorStats
    {
        public static void Reset()
        {
            FluisCreated = 0;
            FluisRemoved = 0;
            FluisDestroyed = 0;
            UnparentedVisualElementsRemoved = 0;
            TotalRebuild = 0;
            ValueBindingStats.Reset();
        }

        public static int FluisCreated { get; set; }
        public static int FluisRemoved { get; set; }
        public static int FluisDestroyed { get; set; }
        public static int UnparentedVisualElementsRemoved { get; set; }
        public static int TotalRebuild { get; set; }

        public static string Describe() => 
            $"Fluid Creator: Created={FluisCreated} | Removed={FluisRemoved} | Destroyed={FluisDestroyed} | Rebuild={TotalRebuild}";
    }
}