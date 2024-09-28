using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimRoundExtraEvents.ApparelClasses
{
    public class FatStorageEnums
    {
		public enum LockingMechanism
		{
			Never, //Doesn't do anything
			OnEquip, //Locks the equipment on equip
			OverSoftCap, //Locks the equipment over softcap
		}

		public enum HideStorageUpTo
		{
			Never, //Doesn't do anything
			UntilOverSoftLimit, //Stops hiding once the softcap is reached once
			UntilHardLimit //Stops hiding once the hardcap is reached once
		}

		public enum DumpsFatOn
		{
			Never, //Doesn't do anything
			OnUnequip, //Dumps fat on unequip
			OnFailure //Dumps fat on failure, like breaking or dropping after going over the hard limit
		}

		public enum HardLimitFailureMode
		{
			None, //Doesn't do anything
			Block, //Blocks more fat from being stored
			Drop, //Harmlessly drops the equipment, best used with dumping set to OnUnequip or OnFailure 
			Break //Breaks the equipment, best used with dumping set to OnUnequip or OnFailure 
		}
	}
}
