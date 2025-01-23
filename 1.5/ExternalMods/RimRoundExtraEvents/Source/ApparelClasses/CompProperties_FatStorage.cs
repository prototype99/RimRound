using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ApparelClasses
{
    public class CompProperties_FatStorage : CompProperties
    {
        public CompProperties_FatStorage()
        {
            this.compClass = typeof(CompFatStorage);
            //TODO: talk to niwatori about a notify/absorbed based system for fat handling
            //maybe including a priority list

            //TODO: turn off messages on funnel&whatevs
            //TODO: fix issue with text field not being exited for some reason

            //TODO: talk to niwatori about resolving the temporary fat exploit issue

            //TODO: CompGetSpecialApparelScoreOffset, change apparel score based on cap
        }

        //cool shit:
        //weapon that steals fat on kills and adds to storage
        //apparel that passively "eats" stuff around the pawn
        //apparel with hidden contents/storage
        //apparel with gluttonium generator
        //ancient armor/equipment with unknown capacity&contents, no buttons, "its emits a soft hum, resonating through the wearer's body"
        //has a gluttonium generator, slowly increases contents when worn, "gluttonium buildup"
        //maybe an exoskeleton
        //gluttonium buildup hediff, very slowly increases pawn weight as it decreases in severity
        //from gluttonium reactor exposure, mentions how much weight has built up
        //higher values deteriote quicker

        public bool hasFunnelMode = true;
        public bool hasFunnelModeButton = true;
        public int funnelModeIntervalTicks = 30;

        public bool hasAddButton = true;

        public bool hasRemoveButton = true;

        //TODO: maybe downing the pawn or applying movement penalties as EMP options
        public int ticksDisabledByEMP = 600;
        public bool dumpsFatOnEMP = true;
        public bool sendEMPFatDumpMessage = false;

        //Use negative values, applies a penalty to the wearer's movement capacity when over the soft limit
        public float softLimitMovementPenalty = -0.2f;

        //When over the soft limit, deals overSoftLimitDamagePerApplication every overSoftLimitTicksPerDamageApplication ticks
        //Default is randomly rounded 2.5 damage every half hour, breaking 120 hitpoints apparel in about a day
        public float overSoftLimitDamagePerApplication = 2.5f;
        public int overSoftLimitTicksPerDamageApplication = 1250;

        public FatStorageEnums.HardLimitFailureMode hardLimitFailureMode = FatStorageEnums.HardLimitFailureMode.Break;

        public FatStorageEnums.DumpsFatOn dumpsFatOnRemoval = FatStorageEnums.DumpsFatOn.OnFailure;

        public FatStorageEnums.LockingMechanism lockEquipmentWhen = FatStorageEnums.LockingMechanism.OverSoftCap;

        public bool hideStorageContentsWhenMade = false;
        public bool stopHidingContentsOnceEmptied = false;
        public FatStorageEnums.HideStorageUpTo stopHidingContentsAtLimit = FatStorageEnums.HideStorageUpTo.Never;

        public bool hideStorageCapacityWhenMade = false;
        public bool stopHidingCapacityOnceEmptied = false;
        public FatStorageEnums.HideStorageUpTo stopHidingCapacityAtLimit = FatStorageEnums.HideStorageUpTo.Never;
    }
}
