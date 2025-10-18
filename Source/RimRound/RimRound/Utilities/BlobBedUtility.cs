using RimWorld;

namespace RimRound.Utilities
{
    public static class BlobBedUtility
    {
        public static bool IsBlobBed(this Building_Bed bed) 
        {
            // Safely handle null bed or def to avoid NREs when pawns sleep on the ground or bedless
            string defName = bed?.def?.defName;
            if (defName == null)
                return false;

            return defName == Defs.ThingDefOf.BlobBed_FoldsOfHeaven_z.defName ||
                   defName == Defs.ThingDefOf.BlobBed_FoldsOfHeaven_I.defName ||
                   defName == Defs.ThingDefOf.BlobBed_FoldsOfHeaven_II.defName ||
                   defName == Defs.ThingDefOf.BlobBed_FoldsOfHeaven_III.defName ||
                   defName == ThingDefOf.SleepingSpot.defName ||
                   defName == Defs.ThingDefOf.DoubleSleepingSpot.defName;
        }
    }
}
