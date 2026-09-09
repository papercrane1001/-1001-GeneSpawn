using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;


namespace GeneSpawner
{
    [StaticConstructorOnStartup]
    public class GeneSpawnerMod
    {
        private static readonly Type patchType = typeof(GeneSpawnerMod);

        static GeneSpawnerMod()
        {
            Harmony harmony = new Harmony("GeneSpawnerMod");
            harmony.Patch(
                AccessTools.Method(typeof(PawnGenerator),
                    typeof(PawnGenerator).GetMethod("GenerateGenes", BindingFlags.NonPublic | BindingFlags.Static)
                        .Name),
                postfix: new HarmonyMethod(patchType, nameof(PostfixGenerator)));
        }


        static void PostfixGenerator(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request)
        {
            int numGenes = 2;
            float t1 = 0.33f;
            float t2 = 0.1f;

            //if(request.xen)

            //TODO: Blacklist?

            //Decide how many genes to add
            Random r = new Random();
            double rr = r.NextDouble();
            if (rr < t2)
            {
                if (r.NextDouble() > 0.5)
                {
                    numGenes = 4;
                }
                else
                {
                    numGenes = 0;
                    return;
                }
            }
            else if (rr < t1)
            {
                if (r.NextDouble() > 0.5)
                {
                    numGenes = 3;
                }
                else
                {
                    numGenes = 1;
                }
            }


        }
    }

    public static class GeneSpawnerModMath
    {
        //Eventually have the probability and range configurable.  Right now...
    }
}
