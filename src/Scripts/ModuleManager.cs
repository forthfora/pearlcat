using UnityEngine;
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
    public static List<PlayerModule> GetAllPlayerData(this RainWorldGame game)
    {
        List<PlayerModule> allPlayerData = new();
        var players = game.Players;

        if (players == null)
            return allPlayerData;

        foreach (AbstractCreature creature in players)
        {
            if (creature.realizedCreature is not Player player) continue;

            if (!PearlcatData.TryGetValue(player, out PlayerModule playerModule)) continue;

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
        if (crit.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) return;

        if (crit.IsPearlpup()) return;

        var save = crit.world.game.GetMiscWorld();

        if (save == null) return;

        if (save.PearlpupID != null) return;

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
    public static readonly ConditionalWeakTable<MenuScene, MenuSceneModule> MenuSceneData = new();
    public static readonly ConditionalWeakTable<MenuDepthIllustration, MenuIllustrationModule> MenuIllustrationData = new();


    // Five Pebbles
    public static ConditionalWeakTable<SSOracleBehavior, SSOracleModule> SSOracleData { get; } = new();
    public static SSOracleModule GetModule(this SSOracleBehavior oracle) => SSOracleData.GetValue(oracle, x => new SSOracleModule());


    // Music Player
    public static readonly ConditionalWeakTable<MusicPlayer, MusicPlayerModule> MusicPlayerData = new();
    public static MusicPlayerModule GetModule(this MusicPlayer self) => MusicPlayerData.GetValue(self, x => new MusicPlayerModule());


    // Sentry
    public static ConditionalWeakTable<AbstractPhysicalObject, POSentry> SentryData { get; } = new();
    public static bool TryGetSentry(this AbstractPhysicalObject self, out POSentry sentry) => SentryData.TryGetValue(self, out sentry);


    // Player Object
    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerObjectModule> PlayerObjectData { get; } = new();
    public static bool TryGetPOModule(this AbstractPhysicalObject abstractObject, out PlayerObjectModule module) => PlayerObjectData.TryGetValue(abstractObject, out module);


    // Object Addon
    public static ConditionalWeakTable<AbstractPhysicalObject, POGraphics> PlayerObjectGraphicsData { get; } = new();
    public static bool TryGetPOGraphics(this AbstractPhysicalObject abstractObject, out POGraphics module) => PlayerObjectGraphicsData.TryGetValue(abstractObject, out module);


    // Slugcat Select Menu
    public static ConditionalWeakTable<SlugcatSelectMenu, SlugcatSelectMenuModule> SlugcatSelectMenuData { get; } = new();
    public static SlugcatSelectMenuModule GetModule(this SlugcatSelectMenu self) => SlugcatSelectMenuData.GetValue(self, x => new SlugcatSelectMenuModule(self));


    // Pearl Spear
    public static ConditionalWeakTable<AbstractSpear, SpearModule> TempPearlSpearData { get; } = new();
    public static bool TryGetSpearModule(this AbstractSpear spear, out SpearModule module)
    {
        var save = spear.Room.world.game.GetMiscWorld();

        if (save == null)
        {
            return TempPearlSpearData.TryGetValue(spear, out module);
        }

        return save.PearlSpears.TryGetValue(spear.ID.number, out module);
    }


    // Save Data
    public static SaveMiscWorld? GetMiscWorld(this RainWorldGame game) => game.IsStorySession ? GetMiscWorld(game.GetStorySession.saveState.miscWorldSaveData) : null;
    public static SaveMiscWorld GetMiscWorld(this MiscWorldSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscWorld save))
        {
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());
        }

        return save;
    }

    public static SaveMiscProgression GetMiscProgression(this RainWorld rainWorld) => GetMiscProgression(rainWorld.progression.miscProgressionData);
    public static SaveMiscProgression GetMiscProgression(this PlayerProgression.MiscProgressionData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscProgression save))
        {
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());
        }

        return save;
    }
}
