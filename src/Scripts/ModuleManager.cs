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
    public static ConditionalWeakTable<AbstractCreature, PlayerModule> PearlcatData { get; } = new();
    public static bool TryGetPearlcatModule(this Player self, out PlayerModule playerModule)
    {
        if (!self.IsPearlcat())
        {
            playerModule = null!;
            return false;
        }

        if (!PearlcatData.TryGetValue(self.abstractCreature, out playerModule))
        {
            playerModule = new PlayerModule(self);
            PearlcatData.Add(self.abstractCreature, playerModule);

            playerModule.LoadInventorySaveData(self);
        }

        return true;
    }
    public static List<PlayerModule> GetAllPearlcatModules(this RainWorldGame game)
    {
        var allPearlcats = game.GetAllPearlcats();
        var playerModules = new List<PlayerModule>();

        foreach (var abstractCreature in allPearlcats)
        {
            if (abstractCreature.realizedObject is not Player player)
            {
                continue;
            }

            if (!player.TryGetPearlcatModule(out var playerModule))
            {
                continue;
            }

            playerModules.Add(playerModule);
        }

        return playerModules;
    }
    

    // Pearlpup
    public static ConditionalWeakTable<AbstractCreature, PearlpupModule> PearlpupData { get; } = new();
    public static bool TryGetPearlpupModule(this Player self, out PearlpupModule module)
    {
        if (!self.IsPearlpup())
        {
            module = null!;
            return false;
        }

        if (!PearlpupData.TryGetValue(self.abstractCreature, out module))
        {
            module = new PearlpupModule();
            PearlpupData.Add(self.abstractCreature, module);
        }

        return true;
    }
    public static void TryMakePearlpup(this AbstractCreature abstractCreature)
    {
        if (abstractCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            return;
        }

        if (abstractCreature.IsPearlpup())
        {
            return;
        }

        var save = abstractCreature.world.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        if (save.PearlpupID is not null)
        {
            return;
        }

        save.PearlpupID = abstractCreature.ID.number;
    }

    public static ConditionalWeakTable<DataPearl.AbstractDataPearl, HeartPearlModule> HeartPearlData { get; } = new();
    public static bool TryGetHeartPearlModule(this DataPearl.AbstractDataPearl dataPearl, out HeartPearlModule module)
    {
        if (!dataPearl.IsHeartPearl())
        {
            module = null!;
            return false;
        }

        if (!HeartPearlData.TryGetValue(dataPearl, out module))
        {
            module = new(dataPearl);
            HeartPearlData.Add(dataPearl, module);
        }

        return true;
    }


    // Menu Scene
    public static ConditionalWeakTable<MenuScene, MenuSceneModule> MenuSceneData { get; } = new();
    public static bool TryGetModule(this MenuScene self, out MenuSceneModule module)
    {
        return MenuSceneData.TryGetValue(self, out module);
    }

    public static ConditionalWeakTable<MenuIllustration, MenuIllustrationModule> MenuIllustrationData { get; } = new();
    public static MenuIllustrationModule GetModule(this MenuIllustration self)
    {
        return MenuIllustrationData.GetValue(self, _ => new MenuIllustrationModule());
    }

    // Five Pebbles
    public static ConditionalWeakTable<SSOracleBehavior, SSOracleModule> SSOracleData { get; } = new();
    public static SSOracleModule GetModule(this SSOracleBehavior self)
    {
        return SSOracleData.GetValue(self, _ => new SSOracleModule());
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
    public static bool TryGetPlayerPearlModule(this AbstractPhysicalObject self, out PlayerPearlModule module)
    {
        return PlayerPearlData.TryGetValue(self, out module);
    }


    // Player Pearl Graphics
    public static ConditionalWeakTable<AbstractPhysicalObject, PearlGraphics> PlayerPearlGraphicsData { get; } = new();
    public static bool TryGetPearlGraphicsModule(this AbstractPhysicalObject self, out PearlGraphics module)
    {
        return PlayerPearlGraphicsData.TryGetValue(self, out module);
    }


    // Slugcat Select Menu
    public static ConditionalWeakTable<SlugcatSelectMenu, SlugcatSelectMenuModule> SlugcatSelectMenuData { get; } = new();
    public static SlugcatSelectMenuModule GetModule(this SlugcatSelectMenu self)
    {
        return SlugcatSelectMenuData.GetValue(self, _ => new SlugcatSelectMenuModule(self));
    }


    // Pearl Spear
    public static ConditionalWeakTable<AbstractPhysicalObject, SpearModule> TempPearlSpearData { get; } = new();
    public static bool TryGetSpearModule(this AbstractPhysicalObject self, out SpearModule module)
    {
        var save = self.Room?.world?.game?.GetMiscWorld();

        if (save is null)
        {
            return TempPearlSpearData.TryGetValue(self, out module);
        }

        return save.PearlSpears.TryGetValue(self.ID.number, out module);
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
