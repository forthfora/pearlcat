using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

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



    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.baseStats = self.Malnourished ? playerModule.malnourishedStats : playerModule.normalStats;

        if (self.room != null && JustWarpedData.TryGetValue(self.room.game, out var justWarped) && justWarped.Value)
        {
            self.AbstractizeInventory();
            justWarped.Value = false;
        }

        self.TryRealizeInventory();

        UpdatePlayerOA(self, playerModule);
        UpdatePlayerDaze(self, playerModule);
        UpdatePostDeathInventory(self, playerModule);

        UpdateCombinedPOEffect(self, playerModule);
        ApplyCombinedPOEffect(self, playerModule);

        CheckInput(self, playerModule);
    }


    public static void CheckInput(Player self, PearlcatModule playerModule)
    {
        var numPressed = self.GetNumberPressed();

        if (numPressed >= 0)
            self.ActivateObjectInStorage(numPressed - 1);
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

            for (int i = 0; i < 6; i++)
            {
                DataPearlType type = i switch
                {
                    0 => Enums.Pearls.AS_Pearl,
                    1 => DataPearlType.CC,
                    2 => DataPearlType.HI,
                    3 => DataPearlType.DS,
                    4 => DataPearlType.SH,
                    _ => DataPearlType.Misc,
                };

                var pearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), -1, -1, null, type);
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

    // HACK
    public static bool hasSpawned = false;
    public static bool wasPressedLeft = false;
    public static bool wasPressedRight = false;



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


    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        //StoreObjectUpdate(self);

        //TransferObjectUpdate(self);
    }
    
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
    public static void Revive(this Player self)
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

    public static int GraspsHasType(this Player self, AbstractPhysicalObject.AbstractObjectType type)
    {
        for (int i = 0; i < self.grasps.Length; i++)
        {
            Creature.Grasp? grasp = self.grasps[i];
            
            if (grasp == null) continue;

            if (grasp.grabbed.abstractPhysicalObject.type == type)
                return i;
        }

        return -1;
    }
}
