using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using RimRound.UI;
using RimRound.Utilities;
using RimRound.Things;

namespace RimRound.Comps
{
    public class MakeBlobIntoBed_ThingComp : ThingComp
    {
        private bool? disabled = null;

        public bool Disabled
        {
            get
            {
                if (disabled == null)
                {
                    disabled = !this.parent.AsPawn().RaceProps.Humanlike || this.parent.AsPawn()?.needs?.food == null;
                }
                return disabled.GetValueOrDefault();
            }
        }


        public MakeBlobIntoBed_ThingComp() 
        {

        }

        bool CheckBlobBedElligibility() 
        {
            if (Find.TickManager.TicksGame % ticksBetweenChecks == 0)
            {
                if (Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Weight, (Pawn)parent) is Hediff h &&
                    h?.Severity is float s &&
                    s >= Utilities.HediffUtility.KilosToSeverityWithBaseWeight(GlobalSettings.weightToBeBed.threshold))
                {
                    canBeBed = true;
                }
                else 
                {
                    if (IsBed) { ResetBed(); }

                    canBeBed = false;
                    IsBed = false;
                }
            }
            return canBeBed;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Disabled) { return; }

            fndComp = parent.AsPawn().TryGetComp<FullnessAndDietStats_ThingComp>();
            if (fndComp is null)
            {
                Log.Error("Comp was null in MakeBlobIntoBed ctor");
                return;
            }

            gizmo = new MakeBlobIntoBedGizmo(this, fndComp);
            gizmoRec = new MakeRecreationSpotGizmo(this);
            generatorGizmo = new MakeGeneratorGizmo(this, fndComp);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Disabled || gizmo is null || gizmoRec is null || generatorGizmo is null || fndComp is null)
                yield break;

            if (!parent.AsPawn().InBed())
            {
                gizmo.Disable($"They must be in a bed to be one!");
                gizmoRec.Disable("They must be in a bed to be a rec spot!");
                generatorGizmo.Disable("They must be in a bed to be a generator!");
            }
            else
            {
                gizmo.Disabled = false;
                gizmoRec.Disabled = false;
                generatorGizmo.Disabled = false;
            }

            if (GlobalSettings.showBlobIntobedGizmo && CheckBlobBedElligibility())
            {
                yield return gizmo;

                if ((fndComp?.perkLevels?.PerkToLevels?["RR_WeLikeToParty_Title"] ?? 0) > 0)
                    yield return gizmoRec;

                if ((fndComp?.perkLevels?.PerkToLevels?["RR_PaunchPower_Title"] ?? 0) > 0)
                    yield return generatorGizmo;
            }
                
        }


        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Disabled) { return; }

            Scribe_Values.Look<bool>(ref _isBed, "blobIsBed", false);
            Scribe_Values.Look<bool>(ref _isRecSpot, "isRecSpot", false);
            Scribe_Values.Look<bool>(ref _isPowerSpot, "isPowerSpot", false);

            Scribe_References.Look<Thing>(ref recSpot, "recSpot");
            Scribe_References.Look<Thing>(ref generatorSpot, "generatorSpot");
            Scribe_References.Look<Thing>(ref blobBed, "blobBed");
        }

        bool _isBed = false;
        bool canBeBed = false;
        public bool IsBed 
        {
            get 
            {
                return _isBed;
            }
            set 
            {
                _isBed = value;
            }
        }

        public void SpawnBed() 
        {
            IntVec3 parentPos = new IntVec3(parent.Position.x, parent.Position.y, parent.Position.z);
            parentPos.z -= 2;

            ThingDef bedToSpawn = null;

            switch (fndComp?.perkLevels?.PerkToLevels?["RR_FoldsOfHeaven_Title"] ?? 4)
            {
                case 0:
                    bedToSpawn = Defs.ThingDefOf.BlobBed_FoldsOfHeaven_z;
                    break;
                case 1:
                    bedToSpawn = Defs.ThingDefOf.BlobBed_FoldsOfHeaven_I;
                    break;
                case 2:
                    bedToSpawn = Defs.ThingDefOf.BlobBed_FoldsOfHeaven_II;
                    break;
                case 3:
                    bedToSpawn = Defs.ThingDefOf.BlobBed_FoldsOfHeaven_III;
                    break;
                default:
                    Log.Error("Folds of Heaven level in Blob Bed gizmo");
                    break;
            }

            Thing t = ThingMaker.MakeThing(bedToSpawn);
            blobBed = GenSpawn.Spawn(t, parentPos, parent.Map, Rot4.South);
            ((Building_BlobBed)blobBed).originatingPawn = parent.AsPawn();
            blobBed.SetFaction(Faction.OfPlayer, null);
            Utilities.HediffUtility.AddHediffSeverity(Defs.HediffDefOf.RimRound_BlobBed, parent.AsPawn(), 1.0f, true);
        }

        public void ResetBed()
        {
            Utilities.HediffUtility.RemoveHediffOfDefFrom(Defs.HediffDefOf.RimRound_BlobBed, this.parent.AsPawn());
            if (GeneralUtility.IsNotNull(blobBed))
            {
                blobBed.DeSpawn();
                blobBed = null;
            }

        }


        bool _isRecSpot = false;
        public bool IsRecSpot 
        {
            get { return _isRecSpot; }
            set { _isRecSpot = value; }
        }

        bool _isPowerSpot = false;
        public bool IsPowerSpot
        {
            get { return _isPowerSpot; }
            set { _isPowerSpot = value; }
        }


        const int ticksBetweenChecks = 15;

        public MakeBlobIntoBedGizmo gizmo;
        public MakeRecreationSpotGizmo gizmoRec;
        public MakeGeneratorGizmo generatorGizmo;


        private FullnessAndDietStats_ThingComp fndComp;
        public Thing recSpot;
        public Thing generatorSpot;
        public Thing blobBed;
    }
}
