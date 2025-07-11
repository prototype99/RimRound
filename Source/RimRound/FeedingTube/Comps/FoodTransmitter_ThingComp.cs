﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.FeedingTube.Comps
{
    public class FoodTransmitter_ThingComp : ThingComp
    {
        public FoodNet FoodNet { get; set; }

        public virtual bool TransmitsFoodNow 
        {
            get 
            {
                return true;
            }
        }

        public FoodTransmitter_CompProperties Props
        {
            get
            {
                return this.props as FoodTransmitter_CompProperties;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                FoodTransmitter_NetManager.For(this.parent.Map).Notify_ConnectorAdded(this);
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            base.PostDeSpawn(map, mode);
            FoodTransmitter_NetManager.For(map).Notify_ConnectorRemoved();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.CompInspectStringExtra());
            sb.AppendLine(FoodNet.NetInfo);

            return sb.ToString().Trim();
        }

        public bool AcceptConnectionFrom(IntVec3 originPosition)
        {
            if (!this.Props.isOneWay)
                return true;
            
            Building building = ((Building)this.parent);

            IntVec3 inputDirection = building.Position + IntVec3.South.RotatedBy(building.Rotation);
            
            if (originPosition == inputDirection)
                return true;
            else
                return false;
        }
    }
}
