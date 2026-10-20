using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GeneSpawner_02
{
    /// <summary>
    /// Harmony postfix on PawnGenerator.GeneratePawn(PawnGenerationRequest) that adds random genes
    /// to every freshly generated pawn that can carry genes.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class GeneSpawner
    {
        public const string HarmonyId = "pc1001.genespawner";

        static GeneSpawner()
        {
            Harmony harmony = new Harmony(HarmonyId);

            MethodInfo original = AccessTools.Method(
                typeof(PawnGenerator),
                nameof(PawnGenerator.GeneratePawn),
                new Type[] { typeof(PawnGenerationRequest) });

            if (original == null)
            {
                Log.Error("[Gene Spawn] Could not find PawnGenerator.GeneratePawn(PawnGenerationRequest); mod disabled.");
                return;
            }

            // Harmony 3 type-checks __result, so the postfix must take exactly "Pawn".
            harmony.Patch(original, postfix: new HarmonyMethod(AccessTools.Method(typeof(GeneSpawner), nameof(GeneratePawn_Postfix))));
        }

        public static void GeneratePawn_Postfix(Pawn __result)
        {
            try
            {
                GeneSpawnerSettings settings = GeneSpawner_Mod.Settings;
                if (settings == null || !settings.enabled)
                {
                    return;
                }

                // Mechanoids, animals, entities etc. have no gene tracker (this was the NullReferenceException in Log_09).
                if (__result == null || __result.genes == null || !__result.RaceProps.Humanlike)
                {
                    return;
                }

                bool isPlayerPawn = __result.Faction != null && __result.Faction.IsPlayer;
                if (isPlayerPawn ? !settings.affectPlayerPawns : !settings.affectOtherPawns)
                {
                    return;
                }

                int numGenes = RollGeneCount(settings);
                if (numGenes <= 0)
                {
                    return;
                }

                AddRandomGenes(__result, numGenes, settings);
            }
            catch (Exception e)
            {
                // Never let a gene problem break pawn generation; just report it once.
                Log.ErrorOnce("[Gene Spawn] Failed to add genes to " + (__result?.LabelShort ?? "a pawn") + ": " + e, 0x1001_6E5E);
            }
        }

        /// <summary>
        /// Average ± 1 with chance <see cref="GeneSpawnerSettings.chanceOffByOne"/>,
        /// average ± 2 with chance <see cref="GeneSpawnerSettings.chanceOffByTwo"/>, otherwise the average.
        /// Never below zero.
        /// </summary>
        public static int RollGeneCount(GeneSpawnerSettings settings)
        {
            int count = settings.averageGenes;
            float roll = Rand.Value;

            if (roll < settings.chanceOffByTwo)
            {
                count += Rand.Bool ? 2 : -2;
            }
            else if (roll < settings.chanceOffByTwo + settings.chanceOffByOne)
            {
                count += Rand.Bool ? 1 : -1;
            }

            return count < 0 ? 0 : count;
        }

        public static void AddRandomGenes(Pawn pawn, int count, GeneSpawnerSettings settings)
        {
            List<GeneDef> candidates = new List<GeneDef>();
            List<GeneDef> allGenes = DefDatabase<GeneDef>.AllDefsListForReading;

            for (int added = 0; added < count; added++)
            {
                candidates.Clear();
                for (int i = 0; i < allGenes.Count; i++)
                {
                    GeneDef gene = allGenes[i];
                    if (IsAllowed(pawn, gene, settings))
                    {
                        candidates.Add(gene);
                    }
                }

                if (candidates.Count == 0)
                {
                    return;
                }

                GeneDef chosen = candidates.RandomElement();
                bool asXenogene = Rand.Chance(settings.xenogeneChance);
                pawn.genes.AddGene(chosen, asXenogene);
            }
        }

        public static bool IsAllowed(Pawn pawn, GeneDef gene, GeneSpawnerSettings settings)
        {
            if (gene == null)
            {
                return false;
            }

            if (settings.useBlacklist && settings.IsBlacklisted(gene))
            {
                return false;
            }

            if (!settings.allowArchite && gene.biostatArc > 0)
            {
                return false;
            }

            Pawn_GeneTracker tracker = pawn.genes;

            if (settings.skipDuplicates && Carries(tracker, gene))
            {
                return false;
            }

            if (settings.requirePrerequisites && gene.prerequisite != null && !tracker.HasActiveGene(gene.prerequisite))
            {
                return false;
            }

            if (settings.respectConflicts)
            {
                List<Gene> current = tracker.GenesListForReading;
                for (int i = 0; i < current.Count; i++)
                {
                    if (current[i].def != gene && current[i].def.ConflictsWith(gene))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>True if the pawn carries the gene at all, active or overridden (HasGene is obsolete in 1.7).</summary>
        private static bool Carries(Pawn_GeneTracker tracker, GeneDef gene)
        {
            List<Gene> current = tracker.GenesListForReading;
            for (int i = 0; i < current.Count; i++)
            {
                if (current[i].def == gene)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
