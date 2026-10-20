using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Text;
using System.Threading.Tasks;

namespace GeneSpawner_02
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawn", new Type[] {typeof(PawnGenerationRequest) })]
    public class GeneSpawner
    {
        private static readonly Type patchType = typeof(GeneSpawner);
        static GeneSpawner()
        {
            Harmony harmony = new Harmony("GeneSpawnerMod");

            MethodInfo method = AccessTools.Method(
                typeof(PawnGenerator),
                "GeneratePawn",
                new Type[] { typeof(PawnGenerationRequest) }
                );

            HarmonyMethod MyPrefix = new HarmonyMethod(patchType, 
                nameof(PrefixGenerator), 
                new Type[] { typeof(Pawn).MakeByRefType(), typeof(PawnGenerationRequest) });

            harmony.Patch(method, null, MyPrefix);
        }


        static void PrefixGenerator(ref Pawn __result, PawnGenerationRequest request)
        {
            try
            {
                if (__result == null)
                {
                    return;
                }

                int numGenes = 2;
                float t1 = 0.32f; //all within 1 standard dev have 1-3 random genes
                float t2 = 0.1f;

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
                        //NOTE: I made the floor 1 here.  Should be zero, but wanted to test.  
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

                //SpawnThoseGenes has a def database of allowed xenotypes.

                IEnumerable<GeneDef> tst = DefDatabase<GeneDef>.AllDefs;

                List<GeneDef> genes = RandomGeneFrom(tst, numGenes);
                for (int i = 0; i < genes.Count; ++i)
                {
                    __result.genes.AddGene(genes[i], false);
                }
            }
            catch
            {
                return;//Probably unnecessary.  
            }

        }
        public static List<GeneDef> RandomGeneFrom(IEnumerable<GeneDef> ieArg, int numReturns = 1)
        {
            Random r = new Random();
            int length = ieArg.Count();

            List<GeneDef> toList = ieArg.ToList();
            List<GeneDef> returns = new List<GeneDef>();

            int result;

            for (int i = 0; i < numReturns; ++i)
            {
                result = r.Next(length - 1);
                returns.Add(toList[result]);
            }
            return returns;
        }
    }
}
