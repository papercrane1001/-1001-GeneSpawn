using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Verse;

namespace GeneSpawner_02
{
    public sealed class GeneSpawner_Mod : Mod
    {

        GeneSpawnerSettings settings;



        //Using Quarry for a lot of help on the settings window
        public GeneSpawner_Mod(ModContentPack mcp) : base(mcp)
        {
            //LongEventHandler.ExecuteWhenFinished()
            this.settings = GetSettings<GeneSpawnerSettings>();
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            //GenUI
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);
            ls.Label("pc1001_GS_SettingsTitle".Translate());
            ls.CheckboxLabeled("pc1001_GS_UseBlacklist".Translate(), ref GeneSpawnerSettings.useBlacklist_GeneSpawner);
            if (GeneSpawnerSettings.useBlacklist_GeneSpawner)
            {
                bool flag = Widgets.ButtonText(rect, "pc1001_GS_AddGene".Translate());
                if (flag) 
                {
                    List<FloatMenuOption> geneList = new List<FloatMenuOption>();
                    using(IEnumerator<GeneDef> geneEnumerator = DefDatabase<GeneDef>.AllDefs.GetEnumerator())
                    {
                        while (geneEnumerator.MoveNext())
                        {
                            GeneDef gene = geneEnumerator.Current;
                            if (!GeneSpawnerSettings.blacklist_GeneSpawner.Contains(gene))
                            {
                                geneList.Add(new FloatMenuOption(gene.LabelCap, delegate ()
                                {
                                    GeneSpawnerSettings.blacklist_GeneSpawner.Add(gene);
                                }));
                            }
                        }
                    }
                    FloatMenu floatMenu = new FloatMenu(geneList);
                    Find.WindowStack.Add(floatMenu);
                }
            }
            ls.End();
            base.DoSettingsWindowContents(rect);
        }
        public override string SettingsCategory()
        {
            return "pc1001_GS".Translate();
        }

    }
}
