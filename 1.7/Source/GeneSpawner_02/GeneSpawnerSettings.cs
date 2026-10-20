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
        public static bool useBlacklist_GeneSpawner = false;
        public static List<GeneDef> blacklist_GeneSpawner = new List<GeneDef>();

        //internal static int medianGenes_GeneSpawner = 2;





        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref useBlacklist_GeneSpawner, "QRY_useBlacklist_GeneSpawner", false);
            try
            {
                Scribe_Values.Look(ref blacklist_GeneSpawner, "QRY_blacklist_GeneSpawner");
            }
            catch(Exception e)
            {
                //Known causes: useBlackList is true, but on load list contains no items
                Log.Message("Exception encountered in GeneSpawner.  Known causes: useBlackList is true, but on load list contains no items");
                Log.Message(e);
            }

        }

    }
}
