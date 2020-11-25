﻿using PKHeX.Core;
using System;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class StopConditionSettings
    {
        private const string StopConditions = nameof(StopConditions);
        public override string ToString() => "Stop Condition Settings";

        [Category(StopConditions), Description("Stops only on Pokémon of this species. No restrictions if set to \"None\".")]
        public Species StopOnSpecies { get; set; }

        [Category(StopConditions), Description("Stop only on Pokémon of the specified nature.")]
        public Nature TargetNature { get; set; } = Nature.Random;

        [Category(StopConditions), Description("Targets the specified IVs HP/Atk/Def/SpA/SpD/Spe. Matches 0's and 31's, checks min value otherwise. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public string TargetIVs { get; set; } = "";

        [Category(StopConditions), Description("Selects the shiny type to stop on.")]
        public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;

        [Category(StopConditions), Description("If set to \"Any\", the bot will target a Pokémon that has any of the Marks listed. Select a certain Mark if you're hunting for it specifically. No restrictions if set to \"None\".")]
        public MarkIndex MarkTarget { get; set; } = MarkIndex.None;

        [Category(StopConditions), Description("Holds Capture button to record a 30 second clip when a matching Pokémon is found by EncounterBot or Fossilbot.")]
        public bool CaptureVideoClip { get; set; }

        [Category(StopConditions), Description("Extra time in milliseconds to wait after an encounter is matched before pressing Capture for EncounterBot or Fossilbot.")]
        public int ExtraTimeWaitCaptureVideo { get; set; } = 10000;

        [Category(StopConditions), Description("If set to TRUE, matches both ShinyTarget and TargetIVs settings. Otherwise, looks for either ShinyTarget or TargetIVs match.")]
        public bool MatchShinyAndIV { get; set; } = true;

        public static bool EncounterFound(PK8 pk, int[] targetIVs, StopConditionSettings settings)
        {
            // Match Nature and Species if they were specified.
            if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
                return false;

            if (settings.TargetNature != Nature.Random && settings.TargetNature != (Nature)pk.Nature)
                return false;

            //If target is Any Mark then do the standard routine otherwise check for a specific Marker
            if ((settings.MarkTarget == MarkIndex.Any && !HasMark(pk, settings.MarkTarget, false)) || 
                (settings.MarkTarget != MarkIndex.None && settings.MarkTarget != MarkIndex.Any && !HasMark(pk, settings.MarkTarget, true)))
                 return false;

            if (settings.ShinyTarget != TargetShinyType.DisableOption)
            {
                bool shinymatch = settings.ShinyTarget switch
                {
                    TargetShinyType.AnyShiny => pk.IsShiny,
                    TargetShinyType.NonShiny => !pk.IsShiny,
                    TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                    TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                    TargetShinyType.DisableOption => true,
                    _ => throw new ArgumentException(nameof(TargetShinyType)),
                };

                // If we only needed to match one of the criteria and it shinymatch'd, return true.
                // If we needed to match both criteria and it didn't shinymatch, return false.
                if (!settings.MatchShinyAndIV && shinymatch)
                    return true;
                if (settings.MatchShinyAndIV && !shinymatch)
                    return false;
            }

            int[] pkIVList = PKX.ReorderSpeedLast(pk.IVs);

            for (int i = 0; i < 6; i++)
            {
                // Match all 0's.
                if (targetIVs[i] == 0 && pkIVList[i] != 0)
                    return false;
                // Wild cards should be -1, so they will always be less than the Pokemon's IVs.
                if (targetIVs[i] > pkIVList[i])
                    return false;
            }
            return true;
        }

        public static int[] InitializeTargetIVs(PokeTradeHub<PK8> hub)
        {
            int[] targetIVs = { -1, -1, -1, -1, -1, -1 };

            /* Populate targetIVs array.  Bot matches 0 and 31 IVs.
             * Any other nonzero IV is treated as a minimum accepted value.
             * If they put "x", this is a wild card so we can leave -1. */
            string[] splitIVs = hub.Config.StopConditions.TargetIVs.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Only accept up to 6 values in case people can't count.
            for (int i = 0; i < 6 && i < splitIVs.Length; i++)
            {
                var str = splitIVs[i];
                if (int.TryParse(str, out var val))
                    targetIVs[i] = val;
            }
            return targetIVs;
        }

        private static bool HasMark(IRibbonIndex pk, MarkIndex target, bool specific)
        {
            if (!specific) {
                for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
                    if ((!specific && pk.GetRibbon((int)mark)) || (specific && pk.GetRibbon((int)mark) && mark.Equals(target)))
                        return true;
            }
            else if (specific && pk.GetRibbon((int)target))
                return true;

            return false;
        }
    }

    public enum TargetShinyType
    {
        DisableOption,  // Doesn't care
        NonShiny,       // Match nonshiny only
        AnyShiny,       // Match any shiny regardless of type
        StarOnly,       // Match star shiny only
        SquareOnly,     // Match square shiny only
    }

    public enum MarkIndex
    {
        None = 0,
        Any = 1,
        MarkLunchtime = 53,
        MarkSleepyTime = 54,
        MarkDusk = 55,
        MarkDawn = 56,
        MarkCloudy = 57,
        MarkRainy = 58,
        MarkStormy = 59,
        MarkSnowy = 60,
        MarkBlizzard = 61,
        MarkDry = 62,
        MarkSandstorm = 63,
        MarkMisty = 64,
        MarkDestiny = 65,
        MarkFishing = 66,
        MarkCurry = 67,
        MarkUncommon = 68,
        MarkRare = 69,
        MarkRowdy = 70,
        MarkAbsentMinded = 71,
        MarkJittery = 72,
        MarkExcited = 73,
        MarkCharismatic = 74,
        MarkCalmness = 75,
        MarkIntense = 76,
        MarkZonedOut = 77,
        MarkJoyful = 78,
        MarkAngry = 79,
        MarkSmiley = 80,
        MarkTeary = 81,
        MarkUpbeat = 82,
        MarkPeeved = 83,
        MarkIntellectual = 84,
        MarkFerocious = 85,
        MarkCrafty = 86,
        MarkScowling = 87,
        MarkKindly = 88,
        MarkFlustered = 89,
        MarkPumpedUp = 90,
        MarkZeroEnergy = 91,
        MarkPrideful = 92,
        MarkUnsure = 93,
        MarkHumble = 94,
        MarkThorny = 95,
        MarkVigor = 96,
        MarkSlump = 97
    }
}
