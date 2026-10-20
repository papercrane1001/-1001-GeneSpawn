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
        //Using Quarry for a lot of help on the settings window
        public GeneSpawner_Mod(ModContentPack mcp) : base(mcp)
        {
            LongEventHandler.ExecuteWhenFinished()
        }

        public override void DoSettingsWindowContents(Rect rect)
        {

        }


    }
}
