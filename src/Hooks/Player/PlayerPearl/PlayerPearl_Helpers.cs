using System.Collections.Generic;
using static AbstractPhysicalObject;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static class PlayerPearl_Helpers
{
    public static void GivePearls(this Player self, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
        {
            return;
        }

        if (ModOptions.InventoryOverride.Value && playerModule.JustWarped)
        {
            playerModule.GivenPearls = false;
        }

        if (playerModule.GivenPearls)
        {
            return;
        }


        var miscWorld = self.room.game.GetMiscWorld();

        var alreadyGivenPearls = miscWorld is not null && miscWorld.PlayersGivenPearls.Contains(self.playerState.playerNumber);

        if (alreadyGivenPearls && !ModOptions.InventoryOverride.Value)
        {
            return;
        }


        List<DataPearlType> pearlsToAdd;
        var overrideLimit = false;

        if (ModOptions.InventoryOverride.Value || ModOptions.StartingInventoryOverride.Value)
        {
            // Halcyon pearl condition
            var giveHalcyonPearl = self.IsFirstPearlcat() || self.abstractCreature.world.game.IsArenaSession;

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
                var specialPearl = (self.IsFirstPearlcat() || self.abstractCreature.world.game.IsArenaSession) ? Enums.Pearls.RM_Pearlcat : DataPearlType.Misc;

                pearlsToAdd.Add(specialPearl);
            }

            overrideLimit = true;
        }

        foreach (var pearlType in pearlsToAdd)
        {
            var pearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), -1, -1, null, pearlType);

            self.StoreObject(pearl, overrideLimit: overrideLimit);
        }

        playerModule.GivenPearls = true;

        if (miscWorld is not null && !miscWorld.PlayersGivenPearls.Contains(self.playerState.playerNumber))
        {
            miscWorld.PlayersGivenPearls.Add(self.playerState.playerNumber);
        }
    }


    // Realization & Abstraction
    public static void TryRealizeInventory(this Player self, PlayerModule playerModule)
    {
        if (self.room is null)
        {
            return;
        }

        if (playerModule.JustWarped)
        {
            return;
        }

        if (self.inVoidSea)
        {
            return;
        }

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
                if (!ModOptions.HidePearls.Value || abstractObject == playerModule.ActiveObject)
                {
                    hasEffect = true;
                }
            }

            RealizePlayerPearl(self, abstractObject, hasEffect);
        }
    }

    private static void RealizePlayerPearl(Player self, AbstractPhysicalObject abstractObject, bool hasEffect)
    {
        RealizePlayerPearl_Local(self, abstractObject, hasEffect);

        if (ModCompat_Helpers.IsModEnabled_RainMeadow)
        {
            MeadowCompat.RPC_RealizePlayerPearl(self, abstractObject, hasEffect);
        }
    }

    public static void RealizePlayerPearl_Local(Player self, AbstractPhysicalObject abstractObject, bool hasEffect)
    {
        if (abstractObject.Room is not null)
        {
            abstractObject.Move(self.abstractPhysicalObject.pos);
        }
        else
        {
            abstractObject.pos = self.abstractCreature.pos;
            self.room.abstractRoom.AddEntity(abstractObject);
        }

        abstractObject.RealizeInRoom();

        abstractObject.MarkAsPlayerPearl();

        if (hasEffect)
        {
            abstractObject.realizedObject?.RealizedEffect();
        }
    }

    public static void AbstractizeInventory(this Player self, bool excludeSentries = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject is null)
            {
                continue;
            }

            if (abstractObject.TryGetSentry(out _) && excludeSentries)
            {
                continue;
            }

            var hasEffect = false;

            if (i < PlayerPearl_Helpers_Graphics.MaxPearlsWithEffects)
            {
                if (!ModOptions.HidePearls.Value || abstractObject == playerModule.ActiveObject)
                {
                    hasEffect = true;
                }
            }

            AbstractPlayerPearl(abstractObject, hasEffect);
        }
    }

    public static void AbstractPlayerPearl(AbstractPhysicalObject abstractObject, bool hasEffect)
    {
        AbstractPlayerPearl_Local(abstractObject, hasEffect);

        if (ModCompat_Helpers.IsModEnabled_RainMeadow)
        {
            MeadowCompat.RPC_AbstractPlayerPearl(abstractObject, hasEffect);
        }
    }

    public static void AbstractPlayerPearl_Local(AbstractPhysicalObject abstractObject, bool hasEffect)
    {
        if (hasEffect)
        {
            abstractObject.realizedObject.AbstractedEffect();
        }

        if (abstractObject.TryGetPlayerPearlModule(out var module))
        {
            module.RemoveSentry(abstractObject);
        }

        abstractObject.Abstractize(abstractObject.pos);
    }


    // Storage & Removal
    public static void StoreObject(this Player self, AbstractPhysicalObject abstractObject, bool fromGrasp = false, bool overrideLimit = false, bool noSound = false, bool storeBeforeActive = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var save = self.abstractCreature.world.game.GetMiscWorld();
    
        if (playerModule.Inventory.Count >= ModOptions.MaxPearlCount.Value && !overrideLimit)
        {
            if (save?.ShownFullInventoryTutorial == false && !ModOptions.DisableTutorials.Value)
            {
                var t = Utils.Translator;

                save.ShownFullInventoryTutorial = true;
                self.abstractCreature.world.game.AddTextPrompt(t.Translate("Storage limit reached (") + ModOptions.MaxPearlCount.Value + t.Translate("): swap out a pearl, or change the limit in the Remix options"), 40, 600);
            }

            self.room?.PlaySound(SoundID.MENU_Error_Ping, self.firstChunk, false, 2.0f, 1.0f);
            return;
        }

        if (fromGrasp)
        {
            if (!noSound)
            {
                self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlStore, self.firstChunk);
            }

            self.ReleaseGrasp(0);
        }

        if (abstractObject is AbstractSpear spear && spear.TryGetModule(out var spearModule))
        {
            abstractObject.destroyOnAbstraction = true;
            abstractObject.Abstractize(abstractObject.pos);

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

            if (!noSound)
            {
                self.room?.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.firstChunk, false, 1.0f, 3.5f);
            }
        }

        self.AddToInventory(abstractObject, storeBeforeActive);

        if (!storeBeforeActive || playerModule.ActiveObjectIndex is null)
        {
            var targetIndex = playerModule.ActiveObjectIndex ?? 0;
            self.ActivateObjectInStorage(targetIndex);
        }
        
        playerModule.PickPearlAnimation(self);
        playerModule.ShowHUD(30);

        self.UpdateInventorySaveData(playerModule);

        if (save?.ShownSpearCreationTutorial == false && abstractObject.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.SpearCreation && abstractObject is DataPearl.AbstractDataPearl dataPearl && dataPearl.dataPearlType != DataPearlType.PebblesPearl && !ModOptions.DisableTutorials.Value)
        {
            save.ShownSpearCreationTutorial = true;

            var t = Utils.Translator;

            if (ModOptions.CustomSpearKeybind.Value)
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
    
    public static void RetrieveActiveObject(this Player self)
    {
        if (self.FreeHand() <= -1)
        {
            return;
        }

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var activeObject = playerModule.ActiveObject;
        if (activeObject is null)
        {
            return;
        }

        self.RemoveFromInventory(activeObject);
        self.SlugcatGrab(activeObject.realizedObject, self.FreeHand());

        playerModule.PickPearlAnimation(self);
        playerModule.ShowHUD(30);

        if (playerModule.ActiveObject is null && playerModule.Inventory.Count > 0)
        {
            var targetIndex = playerModule.ActiveObjectIndex ?? 0;

            if (playerModule.ActiveObjectIndex is not null)
            {
                targetIndex = playerModule.ActiveObjectIndex.Value - 1;

                if (targetIndex < 0)
                {
                    targetIndex = playerModule.Inventory.Count - 1;
                }
            }

            self.ActivateObjectInStorage(targetIndex);
        }

        self.UpdateInventorySaveData(playerModule);
    }


    public static void AddToInventory(this Player self, AbstractPhysicalObject abstractObject, bool addToEnd = false, bool storeBeforeActive = false)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var targetIndex = playerModule.ActiveObjectIndex ?? 0;

        if (addToEnd)
        {
            playerModule.Inventory.Add(abstractObject);
        }
        else if (storeBeforeActive)
        {
            if (playerModule.ActiveObjectIndex is not null)
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
            module.RemoveSentry(abstractObject);
        }

        if (playerModule.Inventory.Count == 0)
        {
            playerModule.ActiveObjectIndex = null;
        }

        InventoryHUD.Symbols.Remove(abstractObject);
    }

    // Selection
    public static void SelectNextObject(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        if (playerModule.ActiveObjectIndex is null)
        {
            return;
        }

        if (playerModule.Inventory.Count <= 1)
        {
            return;
        }


        var targetIndex = (int)playerModule.ActiveObjectIndex + 1;

        if (targetIndex >= playerModule.Inventory.Count)
        {
            targetIndex = 0;
        }

        self.ActivateObjectInStorage(targetIndex);
    }

    public static void SelectPreviousObject(this Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        if (playerModule.ActiveObjectIndex is null)
        {
            return;
        }

        if (playerModule.Inventory.Count <= 1)
        {
            return;
        }


        var targetIndex = (int)playerModule.ActiveObjectIndex - 1;

        if (targetIndex < 0)
        {
            targetIndex = playerModule.Inventory.Count - 1;
        }

        self.ActivateObjectInStorage(targetIndex);
    }

    public static void ActivateObjectInStorage(this Player self, int objectIndex)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (objectIndex < 0 ||  objectIndex >= playerModule.Inventory.Count)
        {
            return;
        }

        if (objectIndex == playerModule.ActiveObjectIndex)
        {
            return;
        }

        var oldObject = playerModule.ActiveObject?.realizedObject;
        playerModule.ActiveObjectIndex = objectIndex;
        var newObject = playerModule.ActiveObject?.realizedObject;

        oldObject.SwapEffect(newObject);

        playerModule.ShowHUD(60);
        self.PlayHUDSound(Enums.Sounds.Pearlcat_PearlScroll);
        
        var save = self.room.game.GetMiscWorld();

        if (save is not null)
        {
            save.ActiveObjectIndex[self.playerState.playerNumber] = objectIndex;
        }

        if (self.graphicsModule is not PlayerGraphics pGraphics || newObject is null)
        {
            return;
        }

        //player.room.PlaySound(Enums.Sounds.Pearlcat_PearlEquip, newObject.firstChunk.pos);
        pGraphics.LookAtPoint(newObject.firstChunk.pos, 1.0f);

        self.UpdateInventorySaveData(playerModule);
    }


    // Save
    public static void UpdateInventorySaveData(this Player self, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
        {
            return;
        }

        if (ModOptions.InventoryOverride.Value)
        {
            return;
        }

        var save = self.room.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        List<string> inventory = [];

        foreach (var item in playerModule.Inventory)
        {
            inventory.Add(item.ToString());
        }


        save.Inventory[self.playerState.playerNumber] = inventory;

        if (playerModule.Inventory.Count == 0)
        {
            playerModule.ActiveObjectIndex = null;
            save.ActiveObjectIndex[self.playerState.playerNumber] = null;
        }
    }
}
