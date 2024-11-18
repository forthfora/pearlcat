using Menu;
using MoreSlugcats;
using Music;
using SlugBase.SaveData;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pearlcat;

public static class ModuleManager
{
    // Pearlcat
    public static ConditionalWeakTable<Player, PlayerModule> PearlcatData { get; } = new();
    public static bool TryGetPearlcatModule(this Player player, out PlayerModule playerModule)
    {
        if (!player.IsPearlcat())
        {
            playerModule = null!;
            return false;
        }

        if (!PearlcatData.TryGetValue(player, out playerModule))
        {
            playerModule = new PlayerModule(player);
            PearlcatData.Add(player, playerModule);

            playerModule.LoadSaveData(player);
        }

        return true;
    }
    public static List<PlayerModule> GetAllPearlcatModules(this RainWorldGame game)
    {
        List<PlayerModule> allPlayerData = [];
        var players = game.Players;

        if (players == null)
        {
            return allPlayerData;
        }

        foreach (var creature in players)
        {
            if (creature.realizedCreature is not Player player)
            {
                continue;
            }

            if (!PearlcatData.TryGetValue(player, out var playerModule))
            {
                continue;
            }

            allPlayerData.Add(playerModule);
        }

        return allPlayerData;
    }
    

    // Pearlpup
    public static ConditionalWeakTable<Player, PearlpupModule> PearlpupData { get; } = new();
    public static bool TryGetPearlpupModule(this Player pup, out PearlpupModule module)
    {
        if (!pup.IsPearlpup())
        {
            module = null!;
            return false;
        }

        if (!PearlpupData.TryGetValue(pup, out module))
        {
            module = new PearlpupModule(pup);
            PearlpupData.Add(pup, module);
        }

        return true;
    }

    public static void MakePearlpup(this AbstractCreature crit)
    {
        if (crit.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            return;
        }

        if (crit.IsPearlpup())
        {
            return;
        }

        var save = crit.world.game.GetMiscWorld();

        if (save == null)
        {
            return;
        }

        if (save.PearlpupID != null)
        {
            return;
        }

        save.PearlpupID = crit.ID.number;
    }

    public static ConditionalWeakTable<DataPearl.AbstractDataPearl, PearlpupPearlModule> PearlpupPearlData { get; } = new();
    public static bool TryGetPearlpupPearlModule(this DataPearl.AbstractDataPearl dataPearl, out PearlpupPearlModule module)
    {
        if (!dataPearl.IsHeartPearl())
        {
            module = null!;
            return false;
        }

        if (!PearlpupPearlData.TryGetValue(dataPearl, out module))
        {
            module = new(dataPearl);
            PearlpupPearlData.Add(dataPearl, module);
        }

        return true;
    }


    // Menu Scene
    public static ConditionalWeakTable<MenuScene, MenuSceneModule> MenuSceneData { get; } = new();
    public static ConditionalWeakTable<MenuIllustration, MenuIllustrationModule> MenuIllustrationData { get; } = new();


    // Five Pebbles
    public static ConditionalWeakTable<SSOracleBehavior, SSOracleModule> SSOracleData { get; } = new();
    public static SSOracleModule GetModule(this SSOracleBehavior oracle)
    {
        return SSOracleData.GetValue(oracle, _ => new SSOracleModule());
    }


    // Music Player
    public static ConditionalWeakTable<MusicPlayer, MusicPlayerModule> MusicPlayerData { get; } = new();
    public static MusicPlayerModule GetModule(this MusicPlayer self)
    {
        return MusicPlayerData.GetValue(self, _ => new MusicPlayerModule());
    }


    // Sentry
    public static ConditionalWeakTable<AbstractPhysicalObject, PearlSentry> SentryData { get; } = new();
    public static bool TryGetSentry(this AbstractPhysicalObject self, out PearlSentry sentry)
    {
        return SentryData.TryGetValue(self, out sentry);
    }


    // Player Pearl
    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerPearlModule> PlayerPearlData { get; } = new();
    public static bool TryGetPlayerPearlModule(this AbstractPhysicalObject abstractObject, out PlayerPearlModule module)
    {
        return PlayerPearlData.TryGetValue(abstractObject, out module);
    }


    // Player Pearl Graphics
    public static ConditionalWeakTable<AbstractPhysicalObject, PearlGraphics> PlayerPearlGraphicsData { get; } = new();
    public static bool TryGetPearlGraphicsModule(this AbstractPhysicalObject abstractObject, out PearlGraphics module)
    {
        return PlayerPearlGraphicsData.TryGetValue(abstractObject, out module);
    }


    // Slugcat Select Menu
    public static ConditionalWeakTable<SlugcatSelectMenu, SlugcatSelectMenuModule> SlugcatSelectMenuData { get; } = new();
    public static SlugcatSelectMenuModule GetModule(this SlugcatSelectMenu self)
    {
        return SlugcatSelectMenuData.GetValue(self, _ => new SlugcatSelectMenuModule(self));
    }


    // Pearl Spear
    public static ConditionalWeakTable<AbstractSpear, SpearModule> TempPearlSpearData { get; } = new();
    public static bool TryGetSpearModule(this AbstractSpear spear, out SpearModule module)
    {
        var save = spear.Room?.world?.game?.GetMiscWorld();

        if (save is null)
        {
            return TempPearlSpearData.TryGetValue(spear, out module);
        }

        return save.PearlSpears.TryGetValue(spear.ID.number, out module);
    }


    // Save Data
    public static SaveMiscWorld? GetMiscWorld(this RainWorldGame game)
    {
        return game.IsStorySession ? GetMiscWorld(game.GetStorySession.saveState.miscWorldSaveData) : null;
    }

    public static SaveMiscWorld GetMiscWorld(this MiscWorldSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscWorld save))
        {
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());
        }

        return save;
    }

    public static SaveMiscProgression GetMiscProgression(this RainWorld rainWorld)
    {
        return GetMiscProgression(rainWorld.progression.miscProgressionData);
    }

    public static SaveMiscProgression GetMiscProgression(this PlayerProgression.MiscProgressionData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscProgression save))
        {
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());
        }

        return save;
    }
}
