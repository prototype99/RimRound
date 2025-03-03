﻿using RimRound.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Comps
{
    public class ThingComp_GeneticDispositions : ThingComp
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



        public float diabetesPredisposition = Values.RandomFloat(0,1);
        public float fLDPredisposition = Values.RandomFloat(0,1);
        public float aFLDPredisposition = Values.RandomFloat(0,1);
        public float hypertensionPredisposition = Values.RandomFloat(0,1);

        public void UpdateHediffs() 
        {
            if (diabetesPredisposition >= GlobalSettings.diabetes.threshold) 
            {
            
            }
            if (fLDPredisposition >= GlobalSettings.FLD.threshold)
            {
                //Functions.AddHediffOfDefTo(Defs.HediffDefOf.);
            }
            if (aFLDPredisposition >= GlobalSettings.aFLD.threshold)
            {

            }
            if (hypertensionPredisposition >= GlobalSettings.hypertension.threshold)
            {

            }
        }

        public override void PostExposeData()
        {
            if (Disabled) { return; }


            Scribe_Values.Look<float>(ref diabetesPredisposition, "diabetesPredisposition");
            Scribe_Values.Look<float>(ref fLDPredisposition, "fLDPredisposition");
            Scribe_Values.Look<float>(ref aFLDPredisposition, "aFLDPredisposition");
            Scribe_Values.Look<float>(ref hypertensionPredisposition, "hypertensionPredisposition");

            base.PostExposeData();
        }

    }
}
