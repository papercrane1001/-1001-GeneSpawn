using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GeneSpawner_02
{
    /// <summary>
    /// Mod entry point and the settings window (Options > Mod settings > Gene Spawn).
    /// Left column: how many genes, endo/xeno split, which pawns, gene rules.
    /// Right column: searchable gene blacklist.
    /// </summary>
    public sealed class GeneSpawner_Mod : Mod
    {
        public static GeneSpawnerSettings Settings { get; private set; }

        private const float RowHeight = 28f;
        private const float IconSize = 24f;
        private const float ColumnGap = 24f;

        private Vector2 optionsScroll = Vector2.zero;
        private float optionsHeight = 600f;
        private Vector2 blacklistScroll = Vector2.zero;
        private string searchText = "";
        private List<GeneDef> sortedGenes;

        public GeneSpawner_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<GeneSpawnerSettings>();
            Settings.Sanitize();
        }

        public override string SettingsCategory() => "Gene Spawn";

        public override void WriteSettings()
        {
            Settings.Sanitize();
            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float leftWidth = (inRect.width - ColumnGap) * 0.5f;
            Rect left = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            Rect right = new Rect(left.xMax + ColumnGap, inRect.y, inRect.width - leftWidth - ColumnGap, inRect.height);

            DrawOptions(left);
            DrawBlacklist(right);
        }

        // ------------------------------------------------------------------ left column

        private void DrawOptions(Rect outRect)
        {
            GeneSpawnerSettings s = Settings;

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, optionsHeight);
            Widgets.BeginScrollView(outRect, ref optionsScroll, viewRect);

            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);

            list.CheckboxLabeled("Enable Gene Spawn", ref s.enabled,
                "When off, newly generated pawns get no extra genes. Pawns that already exist are not changed.");
            list.GapLine();

            // --- How many genes
            Heading(list, "Number of genes");
            s.averageGenes = Mathf.RoundToInt(list.SliderLabeled(
                "Average genes per pawn: " + s.averageGenes,
                s.averageGenes, 0f, GeneSpawnerSettings.MaxAverageGenes, 0.6f,
                "The usual number of random genes added to each pawn."));

            s.chanceOffByOne = RoundPct(list.SliderLabeled(
                "Chance of ±1 gene: " + s.chanceOffByOne.ToStringPercent(),
                s.chanceOffByOne, 0f, 1f, 0.6f,
                "Chance a pawn gets one gene more or one fewer than the average."));

            float maxTwo = 1f - s.chanceOffByOne;
            if (s.chanceOffByTwo > maxTwo) s.chanceOffByTwo = maxTwo;
            s.chanceOffByTwo = RoundPct(list.SliderLabeled(
                "Chance of ±2 genes: " + s.chanceOffByTwo.ToStringPercent(),
                s.chanceOffByTwo, 0f, maxTwo, 0.6f,
                "Chance a pawn gets two genes more or two fewer than the average. Limited so the two chances never add up to more than 100%."));

            float exact = Mathf.Max(0f, 1f - s.chanceOffByOne - s.chanceOffByTwo);
            int min = Mathf.Max(0, s.averageGenes - (s.chanceOffByTwo > 0f ? 2 : (s.chanceOffByOne > 0f ? 1 : 0)));
            int max = s.averageGenes + (s.chanceOffByTwo > 0f ? 2 : (s.chanceOffByOne > 0f ? 1 : 0));
            SmallNote(list, "Exactly " + s.averageGenes + ": " + exact.ToStringPercent() + "   ·   Range: " + min + " to " + max + " genes");
            list.GapLine();

            // --- Endo vs xeno
            Heading(list, "Gene type");
            s.xenogeneChance = RoundPct(list.SliderLabeled(
                "Xenogene chance: " + s.xenogeneChance.ToStringPercent(),
                s.xenogeneChance, 0f, 1f, 0.6f,
                "Each added gene is a xenogene with this chance, otherwise a germline (endo) gene. Xenogenes override endogenes and are lost if the pawn is implanted with a new xenogerm."));
            list.GapLine();

            // --- Which pawns
            Heading(list, "Which pawns");
            list.CheckboxLabeled("Player pawns (starting colonists, etc.)", ref s.affectPlayerPawns,
                "Pawns that belong to your faction when they are generated.");
            list.CheckboxLabeled("All other pawns (raiders, traders, wanderers…)", ref s.affectOtherPawns,
                "Every other humanlike pawn, including factionless ones. Animals, mechanoids and entities never get genes.");
            list.GapLine();

            // --- Rules
            Heading(list, "Gene rules");
            list.CheckboxLabeled("Skip genes the pawn already has", ref s.skipDuplicates,
                "Don't pick a gene the pawn already carries.");
            list.CheckboxLabeled("Respect gene conflicts", ref s.respectConflicts,
                "Don't pick a gene that conflicts with one the pawn already has (e.g. two hair colours).");
            list.CheckboxLabeled("Require prerequisite genes", ref s.requirePrerequisites,
                "Only pick genes whose prerequisite gene the pawn already has.");
            list.CheckboxLabeled("Allow archite genes", ref s.allowArchite,
                "When off, genes that cost archite capsules are never picked.");
            list.GapLine();

            if (list.ButtonText("Reset to defaults"))
            {
                s.ResetToDefaults();
            }

            optionsHeight = list.CurHeight + 12f;
            list.End();
            Widgets.EndScrollView();
        }

        // ------------------------------------------------------------------ right column

        private void DrawBlacklist(Rect rect)
        {
            GeneSpawnerSettings s = Settings;
            List<GeneDef> genes = SortedGenes();

            float y = rect.y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 32f), "Gene blacklist");
            Text.Font = GameFont.Small;
            y += 34f;

            Rect toggleRect = new Rect(rect.x, y, rect.width, RowHeight);
            Widgets.CheckboxLabeled(toggleRect, "Use blacklist", ref s.useBlacklist);
            TooltipHandler.TipRegion(toggleRect, "Checked genes below are never picked while this is on.");
            y += RowHeight + 4f;

            // Search field + clear-all button
            float buttonWidth = 110f;
            Rect searchRect = new Rect(rect.x, y, rect.width - buttonWidth - 6f, RowHeight);
            searchText = Widgets.TextField(searchRect, searchText);
            if (string.IsNullOrEmpty(searchText))
            {
                GUI.color = new Color(1f, 1f, 1f, 0.4f);
                Widgets.Label(new Rect(searchRect.x + 6f, searchRect.y + 3f, searchRect.width, searchRect.height), "Search genes…");
                GUI.color = Color.white;
            }
            if (Widgets.ButtonText(new Rect(searchRect.xMax + 6f, y, buttonWidth, RowHeight), "Clear list"))
            {
                s.ClearBlacklist();
            }
            y += RowHeight + 4f;

            string filter = searchText?.Trim().ToLowerInvariant() ?? "";
            List<GeneDef> shown = filter.Length == 0
                ? genes
                : genes.Where(g => Matches(g, filter)).ToList();

            GUI.color = s.useBlacklist ? Color.white : Color.gray;
            Text.Font = GameFont.Tiny;
            string summary = s.blacklist.Count + " of " + genes.Count + " genes blacklisted";
            if (!s.useBlacklist) summary += " (blacklist is off)";
            Widgets.Label(new Rect(rect.x, y, rect.width, 20f), summary);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            y += 22f;

            Rect outRect = new Rect(rect.x, y, rect.width, rect.yMax - y);
            Widgets.DrawMenuSection(outRect);
            outRect = outRect.ContractedBy(4f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, shown.Count * RowHeight);

            Widgets.BeginScrollView(outRect, ref blacklistScroll, viewRect);

            // Only draw rows that are visible; mod lists can have hundreds of genes.
            int first = Mathf.Max(0, Mathf.FloorToInt(blacklistScroll.y / RowHeight));
            int last = Mathf.Min(shown.Count - 1, Mathf.CeilToInt((blacklistScroll.y + outRect.height) / RowHeight));
            for (int i = first; i <= last; i++)
            {
                DrawGeneRow(new Rect(0f, i * RowHeight, viewRect.width, RowHeight), shown[i], i, s);
            }

            Widgets.EndScrollView();
        }

        private void DrawGeneRow(Rect row, GeneDef gene, int index, GeneSpawnerSettings s)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(row);
            }
            Widgets.DrawHighlightIfMouseover(row);

            Rect iconRect = new Rect(row.x + 2f, row.y + (row.height - IconSize) / 2f, IconSize, IconSize);
            Texture2D icon = gene.Icon;
            if (icon != null)
            {
                GUI.color = gene.IconColor;
                GUI.DrawTexture(iconRect, icon);
                GUI.color = Color.white;
            }

            bool blacklisted = s.IsBlacklisted(gene);
            bool before = blacklisted;
            Rect checkRect = new Rect(iconRect.xMax + 6f, row.y, row.width - iconRect.width - 8f, row.height);
            Widgets.CheckboxLabeled(checkRect, LabelFor(gene), ref blacklisted);
            if (blacklisted != before)
            {
                s.SetBlacklisted(gene, blacklisted);
            }

            if (Mouse.IsOver(row))
            {
                TooltipHandler.TipRegion(row, () => TooltipFor(gene), gene.shortHash + 0x1001);
            }
        }

        // ------------------------------------------------------------------ helpers

        private List<GeneDef> SortedGenes()
        {
            if (sortedGenes == null || sortedGenes.Count != DefDatabase<GeneDef>.DefCount)
            {
                sortedGenes = DefDatabase<GeneDef>.AllDefsListForReading
                    .OrderBy(g => g.displayCategory?.displayPriorityInXenotype ?? 0f)
                    .ThenBy(g => LabelFor(g))
                    .ToList();
            }
            return sortedGenes;
        }

        private static string LabelFor(GeneDef gene)
        {
            string label = gene.LabelCap.ToString();
            return string.IsNullOrEmpty(label) ? gene.defName : label;
        }

        private static bool Matches(GeneDef gene, string filter)
        {
            return LabelFor(gene).ToLowerInvariant().Contains(filter)
                || gene.defName.ToLowerInvariant().Contains(filter)
                || (gene.displayCategory != null && gene.displayCategory.label != null && gene.displayCategory.label.ToLowerInvariant().Contains(filter))
                || (gene.modContentPack != null && gene.modContentPack.Name != null && gene.modContentPack.Name.ToLowerInvariant().Contains(filter));
        }

        private static string TooltipFor(GeneDef gene)
        {
            string tip = LabelFor(gene).Colorize(ColoredText.TipSectionTitleColor);
            if (gene.displayCategory != null)
            {
                tip += "\n" + gene.displayCategory.LabelCap;
            }
            if (gene.biostatArc > 0)
            {
                tip += "\nArchite gene";
            }
            tip += "\n\n" + gene.description;
            tip += "\n\n" + ("defName: " + gene.defName).Colorize(Color.gray);
            if (gene.modContentPack != null)
            {
                tip += "\n" + ("From: " + gene.modContentPack.Name).Colorize(Color.gray);
            }
            return tip;
        }

        private static void Heading(Listing_Standard list, string text)
        {
            Text.Font = GameFont.Medium;
            list.Label(text);
            Text.Font = GameFont.Small;
        }

        private static void SmallNote(Listing_Standard list, string text)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            list.Label(text);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        private static float RoundPct(float v) => Mathf.Round(v * 100f) / 100f;
    }
}
