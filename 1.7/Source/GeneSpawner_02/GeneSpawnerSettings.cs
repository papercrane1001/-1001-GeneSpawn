using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace GeneSpawner_02
{
    public class GeneSpawnerSettings : ModSettings
    {
        internal static bool useBlacklist_GeneSpawner = false;
        internal static List<GeneDef> blacklist_GeneSpawner = new List<GeneDef>();

        //internal static int medianGenes_GeneSpawner = 2;





        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref useBlacklist_GeneSpawner, "QRY_useBlacklist_GeneSpawner", false);
            Scribe_Values.Look(ref blacklist_GeneSpawner, "QRY_blacklist_GeneSpawner");
        }

    }
}
