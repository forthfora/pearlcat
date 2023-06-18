using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerHooks()
    {
        On.Player.Update += Player_Update;

        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.Grabability += Player_Grabability;

        On.Player.Die += Player_Die;

        On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;

        try
        {
            IL.Player.GrabUpdate += Player_GrabUpdateIL;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }



    public static void Player_Die(On.Player.orig_Die orig, Player self) 
    {
        orig(self);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;
        

        for (int i = playerModule.abstractInventory.Count - 1; i >= 0; i--)
        {
            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

            DeathEffect(abstractObject.realizedObject);
            RemoveFromInventory(self, abstractObject);

            playerModule.postDeathInventory.Add(abstractObject);
        }
    }



    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;


        self.TryRealizeInventory();

        UpdatePlayerOA(self, playerModule);
        UpdatePlayerDaze(self, playerModule);

        UpdatePostDeathInventory(self, playerModule);
        UpdatePearlEffects(self, playerModule);
    }

    public static void UpdatePearlEffects(Player self, PearlcatModule playerModule)
    {
        foreach (var pearl in playerModule.abstractInventory)
        {

        }
    }

    public static void UpdatePostDeathInventory(Player self, PearlcatModule playerModule)
    {
        if (!self.dead && playerModule.postDeathInventory.Count > 0)
        {
            for (int i = playerModule.postDeathInventory.Count - 1; i >= 0; i--)
            {
                AbstractPhysicalObject? item = playerModule.postDeathInventory[i];
                playerModule.postDeathInventory.RemoveAt(i);

                if (item.realizedObject == null) continue;

                if (item.realizedObject.room != self.room) continue;

                if (item.realizedObject.grabbedBy.Count > 0) continue;


                if (ObjectAddon.ObjectsWithAddon.TryGetValue(item.realizedObject, out var objectAddon))
                    ObjectAddon.ObjectsWithAddon.Remove(item.realizedObject);

                self.StoreObject(item);
            }
        }
    }

    public static void UpdatePlayerDaze(Player self, PearlcatModule playerModule)
    {
        if (!DazeDuration.TryGet(self, out var dazeDuration)) return;

        if (self.dead || self.bodyMode == Player.BodyModeIndex.Stunned || self.Sleeping)
            playerModule.dazeStacker = dazeDuration;

        if (playerModule.dazeStacker > 0)
            playerModule.dazeStacker--;
    }

    public static void UpdatePlayerOA(Player self, PearlcatModule playerModule)
    {
        if (playerModule.currentObjectAnimation is FreeFallOA)
        {
            if (self.bodyMode != Player.BodyModeIndex.Stunned && self.bodyMode != Player.BodyModeIndex.Dead && !self.Sleeping)
            {
                foreach (var abstractObject in playerModule.abstractInventory)
                    abstractObject.realizedObject.ConnectEffect(((PlayerGraphics)self.graphicsModule).head.pos);

                playerModule.PickObjectAnimation(self);
            }
        }
        else if (self.bodyMode == Player.BodyModeIndex.Stunned || self.bodyMode == Player.BodyModeIndex.Dead || self.Sleeping)
        {
            playerModule.currentObjectAnimation = new FreeFallOA(self);
        }

        playerModule.currentObjectAnimation?.Update(self);


        
        // HACK
        if (!self.dead && !hasSpawned)
        {
            hasSpawned = true;

            for (int i = 0; i < 10; i++)
            {
                DataPearl.AbstractDataPearl.DataPearlType type = i switch
                {
                    0 => DataPearl.AbstractDataPearl.DataPearlType.CC,
                    1 => DataPearl.AbstractDataPearl.DataPearlType.SL_chimney,
                    2 => DataPearl.AbstractDataPearl.DataPearlType.SL_bridge,
                    3 => DataPearl.AbstractDataPearl.DataPearlType.SI_top,
                    4 => DataPearl.AbstractDataPearl.DataPearlType.SB_ravine,
                    5 => DataPearl.AbstractDataPearl.DataPearlType.UW,
                    6 => DataPearl.AbstractDataPearl.DataPearlType.HI,
                    7 => MoreSlugcats.MoreSlugcatsEnums.DataPearlType.OE,
                    8 => DataPearl.AbstractDataPearl.DataPearlType.Red_stomach,
                    _ => DataPearl.AbstractDataPearl.DataPearlType.LF_west,
                };

                AbstractPhysicalObject pearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), -1, -1, null, type);
                self.StoreObject(pearl);
            }
        }

        var inputLeft = Input.GetKey("[");
        var inputRight = Input.GetKey("]");

        if (inputLeft && !wasPressedLeft)
            self.SelectPreviousObject();

        else if (inputRight && !wasPressedRight)
            self.SelectNextObject();

        wasPressedLeft = inputLeft;
        wasPressedRight = inputRight;
    }

    public static bool hasSpawned = false;
    public static bool wasPressedLeft = false;
    public static bool wasPressedRight = false;


    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        //StoreObjectUpdate(self);

        //TransferObjectUpdate(self);
    }

    /*
    public static void StoreObjectUpdate(Player self)
    {
        var playerModule = self.GetModule();

        // Gather held storables
        AbstractPhysicalObject? heldStorable = null;

        for (int i = 0; i < self.grasps.Length; i++)
        {
            if (self.grasps[i] == null) continue;

            AbstractPhysicalObject heldObject = self.grasps[i].grabbed.abstractPhysicalObject;

            if (!IsObjectStorable(heldObject)) continue;

            heldStorable = heldObject;
            break;
        }


        if (!IsStoreKeybindPressed(self))
        {
            playerModule.transferObject = null;
            playerModule.canTransferObject = true;

            // Reenable normal actions
            playerModule.canSwallowOrRegurgitate = true;
            self.spearOnBack.interactionLocked = false;
            self.slugOnBack.interactionLocked = false;
            return;
        }

        // Prevent transfer immediately after one has taken place 
        if (!playerModule.canTransferObject) return;


        // Held items take priority
        playerModule.transferObject = heldStorable ?? GetRealizedActiveObject(self);

        // Disable normal actions
        playerModule.canSwallowOrRegurgitate = false;
        self.slugOnBack.interactionLocked = true;
        self.spearOnBack.interactionLocked = true;

        self.input[0].x = 0;
        self.input[0].y = 0;
    }

    public static void TransferObjectUpdate(Player self)
    {
        if (!PlayerData.TryGetValue(self, out PlayerModule? playerModule)) return;

        playerModule.storingObjectSound.Update();
        playerModule.retrievingObjectSound.Update();

        if (playerModule.transferObject == null)
        {
            ResetTransferObject(playerModule);
            return;
        }

        PlayerGraphics playerGraphics = (PlayerGraphics)self.graphicsModule;

        playerModule.transferObjectInitialPos ??= playerModule.transferObject.realizedObject.firstChunk.pos;

        playerModule.transferStacker++;
        bool puttingInStorage = playerModule.transferObject != GetStoredActiveObject(self);

        if (puttingInStorage)
        {
            int? targetHand = null;

            if (self.grasps.Length == 0) return;

            for (int i = 0; i < self.grasps.Length; i++)
            {
                PhysicalObject graspedObject = self.grasps[i].grabbed;

                if (graspedObject == playerModule.transferObject.realizedObject)
                {
                    targetHand = i;
                    break;
                }
            }

            if (targetHand == null) return;

            //playerModule.storingObjectSound.Volume = 1.0f;

            // Pearl to head
            playerModule.transferObject.realizedObject.firstChunk.pos = Vector2.Lerp(playerModule.transferObject.realizedObject.firstChunk.pos, GetActiveObjectPos(self), (float)playerModule.transferStacker / FramesToStoreObject);
            playerGraphics.hands[(int)targetHand].absoluteHuntPos = playerModule.transferObject.realizedObject.firstChunk.pos;
            playerGraphics.hands[(int)targetHand].reachingForObject = true;

            if (playerModule.transferStacker < FramesToStoreObject) return;

            //playerModule.storingObjectSound.Volume = 0.0f;
            self.room.PlaySound(Enums.Sounds.ObjectStored, self.firstChunk);

            StoreObject(self, playerModule.transferObject);
            AbstractizeInventory(self);
            DestroyTransferObject(playerModule);

            ActivateObjectInStorage(self, playerModule.abstractInventory.Count - 1);
            return;
        }

        // Hand to head

        //playerModule.retrievingObjectSound.Volume = 1.0f;

        playerGraphics.hands[self.FreeHand()].absoluteHuntPos = GetActiveObjectPos(self);
        playerGraphics.hands[self.FreeHand()].reachingForObject = true;

        if (playerModule.transferStacker < FramesToRetrieveObject) return;

        //playerModule.retrievingObjectSound.Volume = 0.0f;
        self.room.PlaySound(Enums.Sounds.ObjectRetrieved, self.firstChunk);

        RetrieveObject(self);
        DestroyTransferObject(playerModule);
    }
    */


    public static void Player_GrabUpdateIL(ILContext il)
    {
        ILCursor c = new(il);
        ILLabel dest = null!;

        // Allow disabling of ordinary swallowing mechanic
        c.GotoNext(MoveType.After,
            x => x.MatchLdcR4(0.5f),
            x => x.MatchBltUn(out _),
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(1),
            x => x.MatchLdloc(1),
            x => x.MatchBrfalse(out dest)
            );

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>((self) =>
        {
            if (!self.TryGetPearlcatModule(out var playerModule))
                return true;

            return playerModule.canSwallowOrRegurgitate;
        });

        c.Emit(OpCodes.Brfalse, dest);
    }
    
    public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (IsPlayerObject(obj))
            return Player.ObjectGrabability.CantGrab;

        return orig(self, obj);
    }



    public static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player player)
            AbstractizeInventory(player);

        orig(self, entrancePos, carriedByOther);
    }

    // Revivify moment
    // https://github.com/Dual-Iron/revivify/blob/master/src/Plugin.cs
    private static void Revive(this Player self)
    {
        self.stun = 20;
        self.airInLungs = 0.1f;
        self.exhausted = true;
        self.aerobicLevel = 1;
         
        self.playerState.alive = true;
        self.playerState.permaDead = false;
        self.dead = false;
        self.killTag = null;
        self.killTagCounter = 0;
        self.abstractCreature.abstractAI?.SetDestination(self.abstractCreature.pos);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.PickObjectAnimation(self);
    }
}
