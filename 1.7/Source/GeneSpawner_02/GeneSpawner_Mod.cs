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
            Rect rect01 = GenUI.TopPart(rect, 0.25f);
            Rect rect02 = GenUI.BottomPart(rect, 0.75f);

            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);
            ls.Label("pc1001_GS_SettingsTitle".Translate());


            Listing_Standard simpleOptions = ls.BeginSection(rect01.height);

            simpleOptions.CheckboxLabeled("pc1001_GS_UseBlacklist".Translate(), ref GeneSpawnerSettings.useBlacklist_GeneSpawner);

            ls.EndSection(simpleOptions);


            Listing_Standard blacklistSection = ls.BeginSection(rect02.height);
            
            if (GeneSpawnerSettings.useBlacklist_GeneSpawner)
            {
                //If using the blacklist
                if (GeneSpawnerSettings.blacklist_GeneSpawner != null)
                {
                    //render the blacklist
                    if(GeneSpawnerSettings.blacklist_GeneSpawner.Count > 0)
                    {
                        bool flagRemove = Widgets.ButtonText(rect02, "pc1001_GS_RemoveGene".Translate());
                        if (flagRemove)
                        {
                            List<FloatMenuOption> blacklist = new List<FloatMenuOption>();
                            using (IEnumerator<GeneDef> blacklistEnumerator = GeneSpawnerSettings.blacklist_GeneSpawner.GetEnumerator())
                            {
                                while (blacklistEnumerator.MoveNext())
                                {
                                    GeneDef gene = blacklistEnumerator.Current;
                                    blacklist.Add(new FloatMenuOption(gene.LabelCap, delegate ()
                                    {
                                        GeneSpawnerSettings.blacklist_GeneSpawner.Remove(gene);
                                    }));
                                }
                            }
                        }
                    }


                }
                bool flag = Widgets.ButtonText(rect02, "pc1001_GS_AddGene".Translate());
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
            ls.EndSection(blacklistSection);

            ls.End();
            base.DoSettingsWindowContents(rect);
        }
        public override string SettingsCategory()
        {
            //return "pc1001_GS".Translate();
            return "GeneSpawnerTest";
        }

    }
}
