using System.Collections.Generic;
using Verse;

namespace GeneSpawner_02
{
    /// <summary>
    /// All player-facing options for Gene Spawn. Saved to
    /// %USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config\Mod_*_GeneSpawner_Mod.xml
    /// </summary>
    public class GeneSpawnerSettings : ModSettings
    {
        // ---- Defaults (kept in one place so "Reset to defaults" and ExposeData agree) ----
        public const bool DefaultEnabled = true;
        public const int DefaultAverageGenes = 2;
        public const float DefaultChanceOffByOne = 0.33f;   // from Notes.txt: ±1 33% of the time
        public const float DefaultChanceOffByTwo = 0.10f;   // from Notes.txt: ±2 10% of the time
        public const float DefaultXenogeneChance = 0.5f;    // from Notes.txt: "for the moment let's just split them"
        public const bool DefaultAffectPlayerPawns = true;
        public const bool DefaultAffectOtherPawns = true;
        public const bool DefaultSkipDuplicates = true;
        public const bool DefaultRespectConflicts = false;
        public const bool DefaultRequirePrerequisites = false;
        public const bool DefaultAllowArchite = true;
        public const bool DefaultUseBlacklist = false;

        public const int MaxAverageGenes = 10;

        // ---- Master switch ----
        public bool enabled = DefaultEnabled;

        // ---- How many genes ----
        /// <summary>Typical number of random genes added to each pawn.</summary>
        public int averageGenes = DefaultAverageGenes;
        /// <summary>Chance the count is one higher or lower than the average.</summary>
        public float chanceOffByOne = DefaultChanceOffByOne;
        /// <summary>Chance the count is two higher or lower than the average.</summary>
        public float chanceOffByTwo = DefaultChanceOffByTwo;

        // ---- Endo vs xeno ----
        /// <summary>Chance each added gene is a xenogene (otherwise it is an endogene/germline gene).</summary>
        public float xenogeneChance = DefaultXenogeneChance;

        // ---- Which pawns ----
        public bool affectPlayerPawns = DefaultAffectPlayerPawns;
        public bool affectOtherPawns = DefaultAffectOtherPawns;

        // ---- Gene rules ----
        public bool skipDuplicates = DefaultSkipDuplicates;
        public bool respectConflicts = DefaultRespectConflicts;
        public bool requirePrerequisites = DefaultRequirePrerequisites;
        public bool allowArchite = DefaultAllowArchite;

        // ---- Blacklist ----
        public bool useBlacklist = DefaultUseBlacklist;
        /// <summary>
        /// Blacklisted genes, stored by defName so the list survives gene mods being removed or reordered.
        /// </summary>
        public List<string> blacklist = new List<string>();

        private HashSet<string> blacklistSet;

        public bool IsBlacklisted(GeneDef def)
        {
            if (blacklistSet == null)
            {
                blacklistSet = new HashSet<string>(blacklist);
            }
            return blacklistSet.Contains(def.defName);
        }

        public void SetBlacklisted(GeneDef def, bool value)
        {
            if (IsBlacklisted(def) == value)
            {
                return;
            }
            if (value)
            {
                blacklist.Add(def.defName);
                blacklistSet.Add(def.defName);
            }
            else
            {
                blacklist.Remove(def.defName);
                blacklistSet.Remove(def.defName);
            }
        }

        public void ClearBlacklist()
        {
            blacklist.Clear();
            blacklistSet = null;
        }

        public void ResetToDefaults()
        {
            enabled = DefaultEnabled;
            averageGenes = DefaultAverageGenes;
            chanceOffByOne = DefaultChanceOffByOne;
            chanceOffByTwo = DefaultChanceOffByTwo;
            xenogeneChance = DefaultXenogeneChance;
            affectPlayerPawns = DefaultAffectPlayerPawns;
            affectOtherPawns = DefaultAffectOtherPawns;
            skipDuplicates = DefaultSkipDuplicates;
            respectConflicts = DefaultRespectConflicts;
            requirePrerequisites = DefaultRequirePrerequisites;
            allowArchite = DefaultAllowArchite;
            useBlacklist = DefaultUseBlacklist;
            // The blacklist itself is kept; there is a separate "Clear" button for it.
        }

        /// <summary>Keeps values in range, e.g. after loading a hand-edited config file.</summary>
        public void Sanitize()
        {
            if (averageGenes < 0) averageGenes = 0;
            if (averageGenes > MaxAverageGenes) averageGenes = MaxAverageGenes;
            chanceOffByOne = Clamp01(chanceOffByOne);
            chanceOffByTwo = Clamp01(chanceOffByTwo);
            if (chanceOffByOne + chanceOffByTwo > 1f)
            {
                chanceOffByTwo = 1f - chanceOffByOne;
            }
            xenogeneChance = Clamp01(xenogeneChance);
            if (blacklist == null)
            {
                blacklist = new List<string>();
            }
            blacklist.RemoveAll(string.IsNullOrEmpty);
            blacklistSet = null;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref enabled, "enabled", DefaultEnabled);

            Scribe_Values.Look(ref averageGenes, "averageGenes", DefaultAverageGenes);
            Scribe_Values.Look(ref chanceOffByOne, "chanceOffByOne", DefaultChanceOffByOne);
            Scribe_Values.Look(ref chanceOffByTwo, "chanceOffByTwo", DefaultChanceOffByTwo);

            Scribe_Values.Look(ref xenogeneChance, "xenogeneChance", DefaultXenogeneChance);

            Scribe_Values.Look(ref affectPlayerPawns, "affectPlayerPawns", DefaultAffectPlayerPawns);
            Scribe_Values.Look(ref affectOtherPawns, "affectOtherPawns", DefaultAffectOtherPawns);

            Scribe_Values.Look(ref skipDuplicates, "skipDuplicates", DefaultSkipDuplicates);
            Scribe_Values.Look(ref respectConflicts, "respectConflicts", DefaultRespectConflicts);
            Scribe_Values.Look(ref requirePrerequisites, "requirePrerequisites", DefaultRequirePrerequisites);
            Scribe_Values.Look(ref allowArchite, "allowArchite", DefaultAllowArchite);

            Scribe_Values.Look(ref useBlacklist, "useBlacklist", DefaultUseBlacklist);
            Scribe_Collections.Look(ref blacklist, "blacklist", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Sanitize();
            }
        }
    }
}
