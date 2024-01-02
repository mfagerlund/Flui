namespace Flui.Binder
{
    public static class FluiBinderStats
    {
        public static int FluiBinderCreated { get; set; }
        public static int FluidBinderDestroyed { get; set; }
        public static int FluiBinderRemoved { get; set; }
        public static int TotalRebuild { get; set; }

        public static string Describe() => $"Fluid Binder: Created={FluiBinderCreated} | Removed={FluiBinderRemoved} | Destroyed={FluidBinderDestroyed} | Rebuild={TotalRebuild}";

        public static void Reset()
        {
            FluiBinderCreated = 0;
            FluidBinderDestroyed = 0;
            FluiBinderRemoved = 0;
            TotalRebuild = 0;
            ValueBindingStats.Reset();
        }
    }
}