using System.Collections.Generic;
using System.Linq;
using static AbstractPhysicalObject;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static class PlayerPearl_Helpers
{
    public static void MarkAsPlayerPearl(this AbstractPhysicalObject abstractObject)
    {
        var module = ModuleManager.PlayerPearlData.GetValue(abstractObject, _ => new PlayerPearlModule());

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            MeadowCompat.AddMeadowPlayerPearlData(abstractObject);
        }

        if (module.IsCurrentlyStored)
        {
            return;
        }

        var physicalObject = abstractObject.realizedObject;

        if (physicalObject is null)
        {
            return;
        }

        module.IsCurrentlyStored = true;

        module.CollideWithObjects = physicalObject.CollideWithObjects;
        module.CollideWithSlopes = physicalObject.CollideWithSlopes;
        module.CollideWithTerrain = physicalObject.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
        {
            module.PearlGlimmerWait = pearl.glimmerWait;
        }

        if (physicalObject is Weapon weapon)
        {
            module.WeaponRotationSpeed = weapon.rotationSpeed;
        }
    }

    public static void ClearAsPlayerPearl(this AbstractPhysicalObject abstractObject)
    {
        if (!abstractObject.TryGetPlayerPearlModule(out var module))
        {
            return;
        }

        if (!module.IsCurrentlyStored)
        {
            return;
        }

        var physicalObject = abstractObject.realizedObject;

        if (physicalObject is null)
        {
            return;
        }

        module.IsCurrentlyStored = false;

        physicalObject.gravity = 1.0f; // yem

        physicalObject.CollideWithObjects = module.CollideWithObjects;
        physicalObject.CollideWithSlopes = module.CollideWithSlopes;
        physicalObject.CollideWithTerrain = module.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
        {
            pearl.glimmerWait = module.PearlGlimmerWait;
        }

        if (physicalObject is Weapon weapon)
        {
            weapon.rotationSpeed = module.WeaponRotationSpeed;
        }
    }

    public static void GivePearls(this Player self, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
        {
            return;
        }

        // Only give pearls once per cycle
        if (playerModule.GivenPearlsThisCycle)
        {
            return;
        }

        var miscWorld = self.abstractCreature.world.game.GetMiscWorld();
        var id = self.playerState.playerNumber;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            if (miscWorld is not null)
            {
                if (!MeadowCompat.WasSaveDataSynced())
                {
                    return;
                }
            }

            var ownerId = ModCompat_Helpers.RainMeadow_GetOwnerIdOrNull(self.abstractPhysicalObject);

            if (ownerId is null)
            {
                return;
            }

            id = (int)ownerId;
        }

        var alreadyGivenPearls = miscWorld is not null && miscWorld.PlayersGivenPearls.Contains(id);

        if (alreadyGivenPearls && !ModOptions.InventoryOverride)
        {
            return;
        }


        List<DataPearlType> pearlsToAdd;
        var overrideLimit = false;

        var giveHalcyonPearl = self.IsFirstPearlcat() || self.abstractCreature.world.game.IsArenaSession || ModCompat_Helpers.RainMeadow_IsOnline;

        if (ModOptions.InventoryOverride || ModOptions.StartingInventoryOverride)
        {
            pearlsToAdd = ModOptions.GetOverridenInventory(giveHalcyonPearl);
        }
        else
        {
            // Defaults
            pearlsToAdd =
            [
                Enums.Pearls.AS_PearlBlue,
                Enums.Pearls.AS_PearlYellow,
                Enums.Pearls.AS_PearlGreen,
                Enums.Pearls.AS_PearlBlack,
                Enums.Pearls.AS_PearlRed,
            ];

            if (!playerModule.IsAdultPearlpup)
            {
                var specialPearl = giveHalcyonPearl ? Enums.Pearls.RM_Pearlcat : DataPearlType.Misc;

                pearlsToAdd.Add(specialPearl);
            }

            overrideLimit = true;
        }

        foreach (var pearlType in pearlsToAdd)
        {
            var pearl = new DataPearl.AbstractDataPearl(self.abstractCreature.world, AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.abstractCreature.world.game.GetNewID(), -1, -1, null, pearlType);

            self.StorePearl(pearl, overrideLimit: overrideLimit);
        }

        playerModule.GivenPearlsThisCycle = true;


        if (miscWorld is not null && !miscWorld.PlayersGivenPearls.Contains(id))
        {
            miscWorld.PlayersGivenPearls.Add(id);

            if (ModCompat_Helpers.RainMeadow_IsOnline)
            {
                MeadowCompat.RPC_SetGivenPearls_OnHost(self);
            }
        }
    }


    // Realization & Abstraction
    public static void TryRealizeInventory(this Player self, PlayerModule playerModule)
    {
        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject is null)
            {
                continue;
            }

            if (abstractObject.realizedObject is not null)
            {
                abstractObject.MarkAsPlayerPearl();
                continue;
            }

            var hasEffect = false;

            if (i < PlayerPearl_Helpers_Graphics.MaxPearlsWithEffects)
            {
                if (!ModOptions.HidePearls || abstractObject == playerModule.ActivePearl)
                {
                    hasEffect = true;
                }
            }

            RealizePlayerPearl(self, abstractObject);

            if (hasEffect)
            {
                abstractObject.realizedObject?.RealizedEffect();
            }
        }
    }

    public static void RealizePlayerPearl(Player self, AbstractPhysicalObject abstractObject)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(abstractObject))
        {
            return;
        }

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            // Meadow needs this, before realization
            abstractObject.InDen = false;
        }

        // For warp
        abstractObject.world = self.abstractCreature.world;

        // Standard realization
        abstractObject.pos = self.abstractCreature.pos;
        self.abstractCreature.Room.AddEntity(abstractObject);
        abstractObject.RealizeInRoom();

        // Pearlcat stuff
        abstractObject.MarkAsPlayerPearl();
    }

    public static void TryAbstractInventory(this Player self, bool isForceIncludingSentries = false)
    {
        self.slugOnBack?.slugcat?.TryAbstractInventory();

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        // Also abstract sentries when changing rooms
        var includingSentries = self.abstractCreature.Room != playerModule.LastRoom || self.inVoidSea || isForceIncludingSentries;

        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject is null)
            {
                continue;
            }

            if (abstractObject.TryGetSentry(out _) && !includingSentries)
            {
                continue;
            }

            var hasEffect = false;

            if (i < PlayerPearl_Helpers_Graphics.MaxPearlsWithEffects)
            {
                if (!ModOptions.HidePearls || abstractObject == playerModule.ActivePearl)
                {
                    hasEffect = true;
                }
            }

            if (hasEffect)
            {
                abstractObject.realizedObject.AbstractedEffect();
            }

            AbstractPlayerPearl(abstractObject);
        }
    }

    public static void AbstractPlayerPearl(AbstractPhysicalObject abstractObject)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(abstractObject))
        {
            return;
        }

        if (abstractObject.TryGetPlayerPearlModule(out var module))
        {
            // Pearlcat stuff
            module.ReturnSentry(abstractObject);
        }

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            // Meadow, set realized on the OnlinePhysicalObject to false
            MeadowCompat.SetRealized(abstractObject, false);
        }

        // Standard abstraction
        abstractObject.Abstractize(abstractObject.pos);
        abstractObject.Room?.RemoveEntity(abstractObject);

        // Meadow needs these
        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            abstractObject.InDen = true;
            abstractObject.pos.WashNode();
        }
    }


    // Storage & Removal
    public static void StorePearl(this Player self, AbstractPhysicalObject abstractObject, bool overrideLimit = false, bool fromGrasp = false, bool fromPearlSpear = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var save = self.abstractCreature.world.game.GetMiscWorld();
    
        if (playerModule.Inventory.Count >= ModOptions.MaxPearlCount && !overrideLimit)
        {
            if (save?.ShownFullInventoryTutorial == false && !ModOptions.DisableTutorials)
            {
                var t = Utils.Translator;

                save.ShownFullInventoryTutorial = true;
                self.abstractCreature.world.game.AddTextPrompt(t.Translate("Storage limit reached (") + ModOptions.MaxPearlCount + t.Translate("): swap out a pearl, or change the limit in the Remix options"), 40, 600);
            }

            self.room?.PlaySound(SoundID.MENU_Error_Ping, self.firstChunk, false, 2.0f, 1.0f);
            return;
        }

        if (fromGrasp)
        {
            if (!fromPearlSpear)
            {
                self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlStore, self.firstChunk);
            }

            self.ReleaseGrasp(0);
        }

        if (abstractObject is AbstractSpear spear && spear.TryGetSpearModule(out var spearModule))
        {
            abstractObject.realizedObject?.Destroy();
            abstractObject.Destroy();

            ExtEnumBase.TryParse(typeof(DataPearlType), spearModule.PearlType, false, out var type);

            if (type as DataPearlType == DataPearlType.PebblesPearl)
            {
                abstractObject = new PebblesPearl.AbstractPebblesPearl(self.abstractCreature.world, null,
                    new(self.abstractCreature.Room.index, -1, -1, 0), self.abstractCreature.world.game.GetNewID(),
                    -1, -1, null, spearModule.PebblesColor, 0);
            }
            else
            {
                abstractObject = new DataPearl.AbstractDataPearl(self.abstractCreature.world, AbstractObjectType.DataPearl, null,
                    new(self.abstractCreature.Room.index, -1, -1, 0), self.abstractCreature.world.game.GetNewID(),
                    -1, -1, null, type as DataPearlType ?? DataPearlType.Misc);
            }

            if (!fromPearlSpear)
            {
                self.room?.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.firstChunk, false, 1.0f, 3.5f);
            }
        }

        self.AddToInventory(abstractObject, fromPearlSpear);

        playerModule.PickPearlAnimation(self);
        playerModule.ShowHUD(30);

        if (save?.ShownSpearCreationTutorial == false && abstractObject.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.SpearCreation && abstractObject is DataPearl.AbstractDataPearl dataPearl && dataPearl.dataPearlType != DataPearlType.PebblesPearl && !ModOptions.DisableTutorials)
        {
            save.ShownSpearCreationTutorial = true;

            var t = Utils.Translator;

            if (ModOptions.CustomSpearKeybind)
            {
                self.abstractCreature.world.game.AddTextPrompt(
                    t.Translate("Hold (") + Input_Helpers.GetAbilityKeybindDisplayName(false) + t.Translate(") or (") + Input_Helpers.GetAbilityKeybindDisplayName(true) + t.Translate(") with an active common pearl to convert it into a pearl spear"), 0, 800);
            }
            else
            {
                self.abstractCreature.world.game.AddTextPrompt("Hold (GRAB) with an active common pearl to convert it into a pearl spear", 0, 800);
            }

            self.abstractCreature.world.game.AddTextPrompt("Pearl spears will attempt to return to you after being thrown, if they are not stuck", 0, 800);
        }
    }
    
    public static void RetrieveActivePearl(this Player self)
    {
        if (self.FreeHand() <= -1)
        {
            return;
        }

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var activePearl = playerModule.ActivePearl;
        if (activePearl is null)
        {
            return;
        }

        self.RemoveFromInventory(activePearl);
        self.SlugcatGrab(activePearl.realizedObject, self.FreeHand());

        playerModule.PickPearlAnimation(self);
        playerModule.ShowHUD(30);
    }


    public static void AddToInventory(this Player self, AbstractPhysicalObject abstractObject, bool fromLoadInventory = false, bool fromPearlSpear = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var targetIndex = playerModule.ActivePearlIndex ?? 0;

        if (fromLoadInventory)
        {
            playerModule.Inventory.Add(abstractObject);
        }
        else if (fromPearlSpear)
        {
            if (playerModule.ActivePearlIndex is not null)
            {
                targetIndex -= 1;

                if (targetIndex < 0)
                {
                    targetIndex = playerModule.Inventory.Count - 1;
                }
            }

            playerModule.Inventory.Insert(targetIndex, abstractObject);
        }
        else
        {
            playerModule.Inventory.Insert(targetIndex, abstractObject);
        }

        abstractObject.MarkAsPlayerPearl();


        // Don't update save data when loading
        if (fromLoadInventory)
        {
            return;
        }

        if (!fromPearlSpear || playerModule.ActivePearlIndex is null)
        {
            targetIndex = playerModule.ActivePearlIndex ?? 0;

            self.SetActivePearl(targetIndex, true);
        }

        self.UpdateInventorySaveData();
    }

    public static void RemoveFromInventory(this Player self, AbstractPhysicalObject abstractObject)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (!playerModule.Inventory.Contains(abstractObject))
        {
            return;
        }

        playerModule.Inventory.Remove(abstractObject);
        abstractObject.ClearAsPlayerPearl();

        if (abstractObject.TryGetPearlGraphicsModule(out var pearlGraphics))
        {
            pearlGraphics.Destroy();
        }

        if (abstractObject.TryGetPlayerPearlModule(out var module))
        {
            module.ReturnSentry(abstractObject);
        }

        if (playerModule.Inventory.Count == 0)
        {
            playerModule.ActivePearlIndex = null;
        }

        if (playerModule.ActivePearl is null && playerModule.Inventory.Count > 0)
        {
            var targetIndex = playerModule.ActivePearlIndex ?? 0;

            if (playerModule.ActivePearlIndex is not null)
            {
                targetIndex = playerModule.ActivePearlIndex.Value - 1;

                if (targetIndex < 0)
                {
                    targetIndex = playerModule.Inventory.Count - 1;
                }
            }

            self.SetActivePearl(targetIndex, false);
        }

        InventoryHUD.Symbols.Remove(abstractObject);

        self.UpdateInventorySaveData();
    }


    // Selection
    public static void SelectNextPearl(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        if (playerModule.ActivePearlIndex is null)
        {
            return;
        }

        if (playerModule.Inventory.Count <= 1)
        {
            return;
        }


        var targetIndex = (int)playerModule.ActivePearlIndex + 1;

        if (targetIndex >= playerModule.Inventory.Count)
        {
            targetIndex = 0;
        }

        self.SetActivePearl(targetIndex, true);
    }

    public static void SelectPreviousPearl(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        if (playerModule.ActivePearlIndex is null)
        {
            return;
        }

        if (playerModule.Inventory.Count <= 1)
        {
            return;
        }


        var targetIndex = (int)playerModule.ActivePearlIndex - 1;

        if (targetIndex < 0)
        {
            targetIndex = playerModule.Inventory.Count - 1;
        }

        self.SetActivePearl(targetIndex, true);
    }

    public static void SetActivePearl(this Player self, int pearlIndex, bool withEffect)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (pearlIndex < 0 ||  pearlIndex >= playerModule.Inventory.Count)
        {
            return;
        }

        if (pearlIndex == playerModule.ActivePearlIndex)
        {
            return;
        }

        var oldActive = playerModule.ActivePearl?.realizedObject;
        playerModule.ActivePearlIndex = pearlIndex;
        var newActive = playerModule.ActivePearl?.realizedObject;

        self.UpdateInventorySaveData();

        if (withEffect)
        {
            oldActive.SwapEffect(newActive);
            playerModule.ShowHUD(60);

            if (ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
            {
                self.PlayHUDSound(Enums.Sounds.Pearlcat_PearlScroll);
            }

            if (self.graphicsModule is PlayerGraphics pGraphics && newActive is not null)
            {
                pGraphics.LookAtPoint(newActive.firstChunk.pos, 1.0f);
            }
        }
    }


    // Save
    public static void UpdateInventorySaveData(this Player self)
    {
        // Save data is handled entirely by the host
        if (!ModCompat_Helpers.RainMeadow_IsHost)
        {
            return;
        }

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var inventory = playerModule.Inventory.Select(x => x.ToString()).ToList();
        var activePearlIndex = playerModule.ActivePearlIndex;

        var save = self.abstractCreature.world.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        var id = self.playerState.playerNumber;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            var ownerId = ModCompat_Helpers.RainMeadow_GetOwnerIdOrNull(self.abstractPhysicalObject);

            if (ownerId is null)
            {
                return;
            }

            id = (int)ownerId;
        }

        if (!ModOptions.InventoryOverride)
        {
            save.Inventory[id] = inventory;
        }

        save.ActiveObjectIndex[id] = activePearlIndex;
    }
}
