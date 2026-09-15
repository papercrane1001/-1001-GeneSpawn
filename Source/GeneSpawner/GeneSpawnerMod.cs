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

            MethodInfo[] infos = typeof(PawnGenerator).GetMethods();

            Log.Message("Got infos");
            for(int i = 0; i < infos.Count(); ++i)
            {
                Log.Message(infos[i].Name);
            }

            //harmony.Patch(
            //    PawnGenerator.GeneratePawn
            //    AccessTools.Method(typeof(PawnGenerator),
            //    typeof(PawnGenerator).GetMethod("GenerateGenes", BindingFlags.NonPublic | BindingFlags.Static).Name)
            //    ,
            //    postfix: new HarmonyMethod(patchType, nameof(PostfixGenerator))
            //    );
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
            //GeneDef

            //GeneDefOf.
            //GeneDef test = GeneDefGenerator.
            IEnumerable<GeneDef> tst = DefDatabase<GeneDef>.AllDefs;

            List<GeneDef> genes = RandomGeneFrom(tst, numGenes);

            for(int i = 0; i < genes.Count; ++i)
            {
                pawn.genes.AddGene(genes[i],false);
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
                result = (int)r.NextInt64((long)length - 1);
                returns.Add(toList[result]);
            }
            return returns;
        }
    }
}
