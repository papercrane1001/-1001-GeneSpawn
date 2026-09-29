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
    public class GeneSpawnerMod
    {
        private static readonly Type patchType = typeof(GeneSpawnerMod);
        static GeneSpawnerMod()
        {
            Harmony harmony = new Harmony("GeneSpawnerMod");

            //MethodInfo[] infos = typeof(PawnGenerator).GetMethods();

            //Log.Message("Got infos");
            //for (int i = 0; i < infos.Count(); ++i)
            //{
            //    Log.Message("infos: " + infos[i].Name);
            //}
            /*
             * GeneratePawn
             * GetXenotypeForGeneratedPawn
             * AdjustXenotypeForFactionlessPawn
             * XenotypesAvailableFor 
             */

            //PawnGenerator.GeneratePawn(PawnGenerationRequest request)
            //PawnGenerator.GeneratePawn(PawnKindDef kindDef,...)

            //harmony.Patch(
            //    //Pawn PawnGenerator.GeneratePawn(PawnKindDef kindDef, [Faction faction = null], [RimWorld.Planet.PlanetTile tile = null])
            //    //PawnGenerator.GetXenotypeForGeneratedPawn
            //    //XenotypeDef PawnGenerator.GetXenotypeForGeneratedPawn(PawnGenerationRequest request)
            //    AccessTools.Method(typeof(PawnGenerator),
            //    typeof(PawnGenerator).GetMethod("GeneratePawn", BindingFlags.Public | BindingFlags.Static).Name)
            //    ,
            //    postfix: new HarmonyMethod(patchType, nameof(PostfixGenerator))
            //    );

            MethodInfo method = AccessTools.Method(
                typeof(PawnGenerator),
                "GeneratePawn",
                new Type[] { typeof(PawnGenerationRequest) }
                );

            

            HarmonyMethod MyPrefix = new HarmonyMethod(patchType, 
                nameof(PrefixGenerator), 
                new Type[] { typeof(Pawn).MakeByRefType(), typeof(PawnGenerationRequest) });

            harmony.Patch(method, MyPrefix);

            //harmony.Patch(
            //    AccessTools.Method(typeof(PawnGenerator),
            //    "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) }),
            //    postfix: new HarmonyMethod(patchType, nameof(PostfixGenerator))
            //    );
        }


        static bool PrefixGenerator(ref Pawn __result, PawnGenerationRequest request)
        {
            int numGenes = 2;
            float t1 = 0.33f;
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
                    numGenes = 1;
                    //return;
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

            Log.Message("ln89");
            IEnumerable<GeneDef> tst = DefDatabase<GeneDef>.AllDefs;
            Log.Message("ln91");

            List<GeneDef> genes = RandomGeneFrom(tst, numGenes);
            Log.Message("ln94");
            //List<GeneDef> geneDefs = new List<GeneDef>();
            for (int i = 0; i < genes.Count; ++i)
            {
                //geneDefs.Add(genes[i]);
                //request.ForcedEndogenes.Add(genes[i]);
                //pawn.genes.AddGene(genes[i], false);
                __result.genes.AddGene(genes[i],false);
            }
            //Log.Message("ln120");
            //request.ForcedEndogenes = geneDefs;
            Log.Message("ln123");
            return true;
        }
        public static List<GeneDef> RandomGeneFrom(IEnumerable<GeneDef> ieArg, int numReturns = 1)
        {
            Log.Message("RandomGeneFrom start");
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
