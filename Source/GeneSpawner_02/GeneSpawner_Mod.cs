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
        public GeneSpawner_Mod(ModContentPack mcp) : base(mcp)
        {
            LongEventHandler.ExecuteWhenFinished(GetSettings);
            LongEventHandler.ExecuteWhenFinished(PushDatabase);
            LongEventHandler.ExecuteWhenFinished(BuildDictionary);
            LongEventHandler.ExecuteWhenFinished(SetFertility);

            LongEventHandler.ExecuteWhenFinished()
        }

        public override void DoSettingsWindowContents(Rect rect)
        {

        }


    }
}
