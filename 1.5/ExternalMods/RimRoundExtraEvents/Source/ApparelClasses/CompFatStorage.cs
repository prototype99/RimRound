using RimRound.Comps;
using RimRound.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ApparelClasses
{
    [StaticConstructorOnStartup]
    public class CompFatStorage : ThingComp
    {
		public CompProperties_FatStorage Props
		{
			get
			{
				return (CompProperties_FatStorage)this.props;
			}
		}

        /*
		* This method handles moving fat between the wearer and this comp,
		* without delays like weight gain requests would.
		* Positive values add to the comp, negative values remove.
		* Returns the actual amount moved
		*/
        public float MoveFat(float kgsToMove, bool funnel = false)
		{
			if (kgsToMove == 0) return 0;

			Pawn pawn = this.PawnOwner;

			if (!(pawn.RaceProps.Humanlike))
				return 0;

			if (!(pawn.WeightHediff() is Hediff))
				return 0;

			var comp = pawn.TryGetComp<FullnessAndDietStats_ThingComp>();

			if (comp is null)
				return 0;

			if (kgsToMove > 0)
			{
				return this.MoveFatTo(kgsToMove, funnel);
			}
			else
			{
				return this.MoveFatFrom(-1 * kgsToMove, funnel);
			}
		}

		private float MoveFatTo(float kgsToMove, bool funnel)
		{
			float newKgsToMove = kgsToMove;

			Hediff weightHediff = this.PawnOwner.WeightHediff();

			float pawnCurrentWeight = RimRound.Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(weightHediff.Severity);

			//Immediately return when the pawn has the minimum possible severity on the weight hediff
			if (pawnCurrentWeight == 1) return 0;

			if (kgsToMove > pawnCurrentWeight)
			{
				// -1 due to the weight hediff having a minimum weight of 1kg
				newKgsToMove = pawnCurrentWeight - 1;
			}

			if (!funnel)
			{
				if (this.FatStored < this.FatStoredSoftLimit)
				{
					newKgsToMove = Math.Min(newKgsToMove, this.FatStoredSoftLimit - this.FatStored);
				}
			}

			if (this.Props.hardLimitFailureMode == FatStorageEnums.HardLimitFailureMode.Block)
			{
				newKgsToMove = Math.Min(newKgsToMove, this.FatStoredHardLimit - this.FatStored);
			}

			//Don't even try to change weight if no weight has to be changed
			if (newKgsToMove == 0) return 0;

			float actualGainedSeverity = this.ChangeWeightAndUpdateSprite(-1 * newKgsToMove);

			float actualFatStoredChanged = RimRound.Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(-1 * actualGainedSeverity);

			this.FatStored += actualFatStoredChanged;

			return actualFatStoredChanged;
		}

		private float MoveFatFrom(float kgsToMove, bool funnel)
		{
			float newKgsToMove = Math.Min(kgsToMove, this.FatStored);

			newKgsToMove = Math.Max(newKgsToMove, 0);

			float pawnCurrentWeight = RimRound.Utilities.HediffUtility.SeverityToKilosWithBaseWeight(this.PawnOwner.WeightHediff().Severity);
			newKgsToMove = Math.Min(newKgsToMove, GlobalSettings.maxWeight.threshold - pawnCurrentWeight);

			var comp = this.PawnOwner.TryGetComp<FullnessAndDietStats_ThingComp>();
			if (comp.perkLevels.PerkToLevels["RR_Even_Further_Beyond_Title"] < 1)
			{
				float gelatinous11Severity = RacialBodyTypeInfoUtility.GetBodyTypeWeightRequirementMultiplier(this.PawnOwner) * 21.85f;
				float gelatinous11Weight = RimRound.Utilities.HediffUtility.SeverityToKilosWithBaseWeight(gelatinous11Severity);
				newKgsToMove = Math.Min(newKgsToMove, gelatinous11Weight - 0.1f - pawnCurrentWeight);
			}

			//Don't even try to change weight if no weight has to be changed
			if (newKgsToMove == 0) return 0;

			float actualGainedSeverity = this.ChangeWeightAndUpdateSprite(newKgsToMove);

			float actualFatStoredChanged = RimRound.Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(-1 * actualGainedSeverity);

			this.FatStored += actualFatStoredChanged;

			return actualFatStoredChanged;
		}

		protected float ChangeWeightAndUpdateSprite(float kgsToAdd)
		{
			float actualGainedSeverity = RimRound.Utilities.HediffUtility.AddHediffSeverity(
				 RimRound.Defs.HediffDefOf.RimRound_Weight,
				 this.PawnOwner,
				 RimRound.Utilities.HediffUtility.KilosToSeverityWithoutBaseWeight(kgsToAdd),
				 false,
				 false,
				 false);

			var pbtThingComp = this.PawnOwner.TryGetComp<PawnBodyType_ThingComp>();
			if (pbtThingComp != null)
				BodyTypeUtility.UpdatePawnSprite(this.PawnOwner, pbtThingComp.PersonallyExempt, pbtThingComp.CategoricallyExempt);

			return actualGainedSeverity;
		}

		/*
		* This method handles moving fat between the wearer and this comp,
		* without delays like weight gain requests would.
		* Positive values add to the comp, negative values remove.
		*/
		public void DumpFatIntoPawn(Pawn pawn, float kgsToDump, int ticksTaken, int applications)
		{
			if (kgsToDump == 0) return;
			
			if (kgsToDump < 0)
			{
				Log.Warning("class RimRoundExtraEvents.ApparelClasses.CompFatStorage method dumpFatIntoPawn() was fed a negative number,"
					+ "it can only accept positive numbers.");
				return;
			}

			if (pawn == null) return;

			if (!(pawn.RaceProps.Humanlike))
				return;

			if (pawn.Dead) return;

			var comp = pawn.TryGetComp<FullnessAndDietStats_ThingComp>();

			if (comp is null)
				return;

			float kilosToAdd = kgsToDump;
			//float kilosPerApplication = 100; //TODO maybe make this ramp up over time, or implement some other solution
			float kilosPerApplication = kilosToAdd / applications;
			int ticksPerApplication = ticksTaken / applications; //Originally 60
			int numberOfApplications = applications;

			int currentTick = Find.TickManager.TicksGame;

			for (int i = 0; i < numberOfApplications; ++i)
			{
				comp.activeWeightGainRequests.Enqueue(new WeightGainRequest(kilosPerApplication, currentTick + i * ticksPerApplication, 0, true, false));
			}

			float leftoverKilos = kilosToAdd % kilosPerApplication;

			if (leftoverKilos > 0)
			{
				comp.activeWeightGainRequests.Enqueue(new WeightGainRequest(leftoverKilos, currentTick + numberOfApplications * ticksPerApplication, 0, true, false));
			}

			this.FatStored -= kgsToDump;
		}

		public virtual void UpdateFunnelToCurrentWeight(Pawn pawn)
		{
			Hediff weightHediff = pawn.WeightHediff();

			if (weightHediff != null
				&& this.Props.hasFunnelMode)
			{
				float kgsToSetTo = RimRound.Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(weightHediff.Severity);
				this.UpdateFunnelToWeight(kgsToSetTo);
			}
		}

		protected void TurnOffOtherWornFunnels(Pawn pawn)
		{
			foreach (Apparel apparel in pawn.apparel.GetDirectlyHeldThings())
			{
				foreach (CompFatStorage comp in apparel.GetComps<CompFatStorage>())
				{
					if (!comp.Equals(this))
						comp.FunnelModeActive = false;
				}
			}
		}

		public virtual void UpdateFunnelToWeight(float kgsToSetTo)
		{
			this.funnelFat = kgsToSetTo;
		}

		public virtual void AddButtonPressed(float kgDiff)
		{
			float kgsRemovedFromPawn = this.MoveFat(kgDiff, false);
			if (this.Props.hasFunnelMode)
				this.UpdateFunnelToWeight(this.funnelFat - kgsRemovedFromPawn);
		}

		public virtual void RemoveButtonPressed(float kgDiff)
		{
			float kgsAddedToPawn = -1 * this.MoveFat(kgDiff, false);
			if (this.Props.hasFunnelMode)
				this.UpdateFunnelToWeight(this.funnelFat + kgsAddedToPawn);
		}

		public virtual void FunnelModeButtonPressed()
		{
			if (this.PawnOwner is null) return;
			this.funnelModeActive = !this.funnelModeActive;
			if (this.funnelModeActive)
			{
				this.UpdateFunnelToCurrentWeight(this.PawnOwner);

				this.TurnOffOtherWornFunnels(this.PawnOwner);

				this.funnelModeActive = true;
			}
		}

		public virtual bool AddButtonDisabled()
		{
			if (this.ButtonsDisabled()) return true;
			return false;
		}

		public virtual bool RemoveButtonDisabled()
		{
			if (this.ButtonsDisabled()) return true;
			return false;
		}

		public virtual bool FunnelModeButtonDisabled()
		{
			if (this.ButtonsDisabled()) return true;
			return false;
		}

		public virtual bool ButtonsDisabled()
        {
			if (this.PawnOwner is null) return true;
			if (this.disabledTicks > 0) return true;
			return false;
        }

		public virtual bool FunnelModeDisabled()
		{
			if (this.PawnOwner is null) return true;
			if (this.disabledTicks > 0) return true;
			return !this.funnelModeActive;
		}

		protected virtual void UpdateMovementPenalties(Pawn pawn, float diff)
		{
			if (diff == 0) return;
			
			RimRoundStatBonuses bonus = new RimRoundStatBonuses();

			bonus.movementFlatBonus = diff;

			StatChangeUtility.ChangeRimRoundStats(pawn, bonus);

			this.currentMovementBonus += diff;
		}

		protected virtual void ApplyOverSoftLimitDamage()
		{
			float num = GenMath.RoundRandom(this.Props.overSoftLimitDamagePerApplication);

			Pawn pawn = this.PawnOwner;

			this.parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, (float)num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));

			if (this.parent.Destroyed
				&& pawn != null
				&& PawnUtility.ShouldSendNotificationAbout(pawn)
				&& !pawn.Dead)
			{
				switch (this.Props.dumpsFatOnRemoval)
				{
					case FatStorageEnums.DumpsFatOn.OnUnequip:
					case FatStorageEnums.DumpsFatOn.OnFailure:
						Messages.Message("RREE_MessageWornFatStorageApparelSoftPoppedFatDump".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
						break;
					default:
						Messages.Message("RREE_MessageWornFatStorageApparelSoftPopped".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
						break;
				}
			}
		}

		public virtual void ApplyEMPPenalties(DamageInfo dinfo)
		{
			this.disabledTicks = Math.Max(this.disabledTicks, this.Props.ticksDisabledByEMP);
			
			if (this.Props.dumpsFatOnEMP)
            {
				this.DumpFatIntoPawn(this.PawnOwner, this.fatStored, Math.Min(300, this.Props.ticksDisabledByEMP), 10);

				if (this.Props.sendEMPFatDumpMessage
					&& PawnUtility.ShouldSendNotificationAbout(this.PawnOwner)
				&& !this.PawnOwner.Dead)
				{
					Messages.Message("RREE_MessageWornFatStorageApparelEMPFatDump".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), this.PawnOwner).CapitalizeFirst(), this.PawnOwner, MessageTypeDefOf.NegativeEvent, true);
				}
			}
		}

		public override void Notify_Equipped(Pawn pawn)
		{
			base.Notify_Equipped(pawn);

			this.UpdateFunnelToCurrentWeight(this.PawnOwner);

			if (this.Props.lockEquipmentWhen == FatStorageEnums.LockingMechanism.OnEquip)
			{
				this.PawnOwner.apparel.Lock(this.parent as Apparel);
			}

			if (this.FatStored > this.FatStoredSoftLimit
				&& this.currentMovementBonus != this.Props.softLimitMovementPenalty)
			{
				float diff = this.Props.softLimitMovementPenalty - this.currentMovementBonus;
				this.UpdateMovementPenalties(this.PawnOwner, diff);
			}
		}

        public override void Notify_Unequipped(Pawn pawn)
		{
			base.Notify_Unequipped(pawn);

			switch (this.Props.dumpsFatOnRemoval)
            {
				case FatStorageEnums.DumpsFatOn.OnUnequip:
					this.DumpFatIntoPawn(pawn, this.FatStored, 300, 10);
					break;
				case FatStorageEnums.DumpsFatOn.OnFailure:
					if (this.parent.Destroyed)
						this.DumpFatIntoPawn(pawn, this.FatStored, 300, 10);
					break;
				default:
					break;
            }

			this.UpdateMovementPenalties(pawn, -1 * this.currentMovementBonus);
		}

		public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;

			if (this.PawnOwner is null) return;

			if (dinfo.Def == DamageDefOf.EMP)
			{
				this.ApplyEMPPenalties(dinfo);
				return;
			}
		}

		public override void CompTick()
		{
			base.CompTick();

			if (this.disabledTicks > 0)
			{
				this.disabledTicks--;
			}

			if (this.Props.overSoftLimitTicksPerDamageApplication > 0
				&& this.FatStored > this.FatStoredSoftLimit
				&& this.parent.IsHashIntervalTick(this.Props.overSoftLimitTicksPerDamageApplication))
			{
				this.ApplyOverSoftLimitDamage();
			}

			if (!this.FunnelModeDisabled()
				&& this.parent.IsHashIntervalTick(this.Props.funnelModeIntervalTicks))
			{
				Hediff weightHediff = this.PawnOwner.WeightHediff();
				float pawnWeight = RimRound.Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(weightHediff.Severity);

				float kgDiff = pawnWeight - this.funnelFat;

				this.MoveFat(kgDiff, true);
			}
		}

		public float FatStoredSoftLimit
		{
			get
			{
				return this.parent.GetStatValue(StatDefOf.FatStorageAmount, true, -1);
			}
		}

		public float FatStoredHardLimit
		{
			get
			{
				return this.FatStoredSoftLimit * (1 + this.FatStoredHardLimitPercent);
			}
		}

		public float FatStoredHardLimitPercent
		{
			get
			{
				return this.parent.GetStatValue(StatDefOf.FatStorageHardLimit, true, -1);
			}
		}

		public virtual float FatStored
		{
			get
			{
				return this.fatStored;
			}
            set
			{
				this.fatStored = value;

				if (value == 0) this.NewFatStoredZero(value);

				if (value <= this.FatStoredSoftLimit) this.NewFatStoredUnderAtSoftLim(value);

				if (value > this.FatStoredSoftLimit) this.NewFatStoredOverSoftLim(value);

				if (value >= this.FatStoredHardLimit) this.NewFatStoredAtOverHardLim(value);

				//TODO: This will break on guest pawns
				if (this.Props.lockEquipmentWhen == FatStorageEnums.LockingMechanism.OverSoftCap)
					this.ChangeLockingBySoftLimit(value);

				if (value > this.FatStoredHardLimit) this.NewFatStoredOverHardLim(value);
			}
		}

		protected virtual void ChangeLockingBySoftLimit(float value)
		{
			if (this.PawnOwner is null) return;

			if (!this.CurrentlyLocked && value > this.FatStoredSoftLimit)
			{
				this.PawnOwner.apparel.Lock(this.parent as Apparel);
				this.CurrentlyLocked = true;
			}
			else if (this.CurrentlyLocked && value <= this.FatStoredSoftLimit)
			{
				this.PawnOwner.apparel.Unlock(this.parent as Apparel);
				this.CurrentlyLocked = false;
			}
		}

		protected virtual void NewFatStoredZero(float value)
		{
			if (this.Props.stopHidingContentsOnceEmptied
						&& this.contentsCurrentlyHidden)
			{
				this.contentsCurrentlyHidden = false;
			}
			if (this.Props.stopHidingCapacityOnceEmptied
				&& this.capacityCurrentlyHidden)
			{
				this.capacityCurrentlyHidden = false;
			}
		}

		protected virtual void NewFatStoredUnderAtSoftLim(float value)
		{
			if (this.PawnOwner != null
				&& this.currentMovementBonus != 0f)
			{
				this.UpdateMovementPenalties(this.PawnOwner, -1 * this.currentMovementBonus);
			}
		}

		protected virtual void NewFatStoredOverSoftLim(float value)
		{
			if (this.Props.stopHidingContentsAtLimit == FatStorageEnums.HideStorageUpTo.UntilOverSoftLimit
						&& this.contentsCurrentlyHidden)
			{
				this.contentsCurrentlyHidden = false;
			}
			if (this.Props.stopHidingCapacityAtLimit == FatStorageEnums.HideStorageUpTo.UntilOverSoftLimit
				&& this.capacityCurrentlyHidden)
			{
				this.capacityCurrentlyHidden = false;
			}

			if (this.PawnOwner != null
				&& this.currentMovementBonus != this.Props.softLimitMovementPenalty)
			{
				float diff = this.Props.softLimitMovementPenalty - this.currentMovementBonus;
				this.UpdateMovementPenalties(this.PawnOwner, diff);
			}
		}

		protected virtual void NewFatStoredAtOverHardLim(float value)
		{
			if (this.Props.stopHidingContentsAtLimit == FatStorageEnums.HideStorageUpTo.UntilHardLimit
						&& this.contentsCurrentlyHidden)
			{
				this.contentsCurrentlyHidden = false;
			}
			if (this.Props.stopHidingCapacityAtLimit == FatStorageEnums.HideStorageUpTo.UntilHardLimit
				&& this.capacityCurrentlyHidden)
			{
				this.capacityCurrentlyHidden = false;
			}
		}

		protected virtual void NewFatStoredOverHardLim(float value)
		{
			switch (this.Props.hardLimitFailureMode)
			{
				case FatStorageEnums.HardLimitFailureMode.Drop:
					if (!(this.PawnOwner is null))
					{
						if (this.Props.dumpsFatOnRemoval is FatStorageEnums.DumpsFatOn.OnFailure)
							this.DumpFatIntoPawn(this.PawnOwner, this.fatStored, 300, 10);

						Pawn pawn = this.PawnOwner;

						bool dropped = pawn.apparel.TryDrop((Apparel)this.parent);
						if (!dropped)
							Log.Error("Couldn't drop Apparel with CompFatStorage");

						if (PawnUtility.ShouldSendNotificationAbout(pawn) && !pawn.Dead)
						{
							switch (this.Props.dumpsFatOnRemoval)
							{
								case FatStorageEnums.DumpsFatOn.OnUnequip:
								case FatStorageEnums.DumpsFatOn.OnFailure:
									Messages.Message("RREE_MessageWornFatStorageApparelDroppedFatDump".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
									break;
								default:
									Messages.Message("RREE_MessageWornFatStorageApparelDropped".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
									break;
							}
						}

						//TODO: maybe also throw particles like the shield belt
					}
					break;
				case FatStorageEnums.HardLimitFailureMode.Break:
					if (!(this.PawnOwner is null))
					{
						Pawn pawn = this.PawnOwner;

						this.parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 9999f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));

						if (this.parent.Destroyed && PawnUtility.ShouldSendNotificationAbout(pawn) && !pawn.Dead)
						{
							switch (this.Props.dumpsFatOnRemoval)
                            {
								case FatStorageEnums.DumpsFatOn.OnUnequip:
								case FatStorageEnums.DumpsFatOn.OnFailure:
									Messages.Message("RREE_MessageWornFatStorageApparelPoppedFatDump".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
									break;
								default:
									Messages.Message("RREE_MessageWornFatStorageApparelPopped".Translate(GenLabel.ThingLabel(this.parent.def, this.parent.Stuff, 1), pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.NegativeEvent, true);
									break;
                            }
						}

						//TODO: maybe also throw particles like the shield belt
					}
					break;
				default:
					break;
			}
		}

		public bool FunnelModeActive
		{
			get
			{
				return this.funnelModeActive;
			}
			set
            {
				this.funnelModeActive = value;

				if (value)
				{
					if (this.PawnOwner is null) return;

					this.UpdateFunnelToCurrentWeight(this.PawnOwner);
				}
			}
		}

		public bool CurrentlyLocked
		{
			get
			{
				return this.currentlyLocked;
			}
			set
			{
				this.currentlyLocked = value;
			}
		}

		public bool ContentsCurrentlyHidden
		{
			get
			{
				return this.contentsCurrentlyHidden;
			}
			set
			{
				this.contentsCurrentlyHidden = value;
			}
		}

		public bool CapacityCurrentlyHidden
		{
			get
			{
				return this.capacityCurrentlyHidden;
			}
			set
            {
				this.capacityCurrentlyHidden = value;
            }
		}

		protected Pawn PawnOwner
		{
			get
			{
				Apparel apparel = this.parent as Apparel;
				if (apparel != null)
				{
					return apparel.Wearer;
				}
				return null;
			}
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
			{
				yield return gizmo;
			}

			foreach (Gizmo gizmo2 in this.GetGizmos())
			{
				yield return gizmo2;
			}

			yield break;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			foreach (Gizmo gizmo2 in this.GetGizmos())
			{
				yield return gizmo2;
			}
			yield break;
		}

		private IEnumerable<Gizmo> GetGizmos()
		{
			if (Find.Selector.SingleSelectedThing == this.parent || (this.PawnOwner.Faction == Faction.OfPlayer && Find.Selector.SingleSelectedThing == this.PawnOwner))
			{
				yield return new Gizmo_FatStorageStatus
				{
					comp = this
				};
			}
			yield break;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.fatStored, "fatStored", 0f, false);
			Scribe_Values.Look<bool>(ref this.funnelModeActive, "funnelModeActive", false, false);
			Scribe_Values.Look<float>(ref this.funnelFat, "funnelFat", 20f, false);
			Scribe_Values.Look<float>(ref this.currentMovementBonus, "currentMovementBonus", 0f, false);
			Scribe_Values.Look<bool>(ref this.contentsCurrentlyHidden, "contentsCurrentlyHidden", true, false);
			Scribe_Values.Look<bool>(ref this.capacityCurrentlyHidden, "capacityCurrentlyHidden", true, false);
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			this.contentsCurrentlyHidden = this.Props.hideStorageContentsWhenMade;
			this.capacityCurrentlyHidden = this.Props.hideStorageCapacityWhenMade;
		}

		protected float fatStored = 0f;

		protected bool funnelModeActive = false;
		protected float funnelFat = 20f;

		protected float disabledTicks = 0f;

		//Used for the soft limit movement penalty
		protected float currentMovementBonus = 0f;

		//Only used by the OverSoftCap lockEquipmentWhen option.
		protected bool currentlyLocked = false;

		protected bool contentsCurrentlyHidden = false;
		protected bool capacityCurrentlyHidden = false;

		public float gizmoVal = 10f;
		public String gizmoBuffer = "10";
	}
}
