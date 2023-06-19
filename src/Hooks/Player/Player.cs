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
    }


    public static void UpdateCombinedPOEffect(Player self, PearlcatModule playerModule)
    {
        POEffect combinedEffect = new();

        foreach (var playerObject in playerModule.abstractInventory)
        {
            var effect = playerObject.GetPOEffect();
            var mult = playerObject == playerModule.ActiveObject ? effect.activeMultiplier : 1.0f;

            if (self.Malnourished)
                mult *= 0.75f;

            combinedEffect.runSpeedFac += effect.runSpeedFac * mult;
            combinedEffect.corridorClimbSpeedFac += effect.corridorClimbSpeedFac * mult;
            combinedEffect.poleClimbSpeedFac += effect.poleClimbSpeedFac * mult;
            
            combinedEffect.throwingSkill += effect.throwingSkill * mult;
            combinedEffect.lungsFac += effect.lungsFac * mult;
            combinedEffect.bodyWeightFac += effect.bodyWeightFac * mult;

            combinedEffect.loudnessFac += effect.loudnessFac * mult;
            combinedEffect.generalVisibilityBonus += effect.generalVisibilityBonus * mult;
            combinedEffect.visualStealthInSneakMode += effect.visualStealthInSneakMode * mult;
        }

        playerModule.currentPOEffect = combinedEffect;
    }

    public static void ApplyCombinedPOEffect(Player self, PearlcatModule playerModule)
    {
        var effect = playerModule.currentPOEffect;
        var stats = self.slugcatStats;
        var baseStats = playerModule.baseStats;

        stats.lungsFac = baseStats.lungsFac + effect.lungsFac;
        stats.throwingSkill = (int)Mathf.Clamp(baseStats.throwingSkill + effect.throwingSkill, 0, 2);
        stats.runspeedFac = baseStats.runspeedFac + effect.runSpeedFac;

        stats.corridorClimbSpeedFac = baseStats.corridorClimbSpeedFac + effect.corridorClimbSpeedFac;
        stats.poleClimbSpeedFac = baseStats.poleClimbSpeedFac + effect.poleClimbSpeedFac;
        stats.bodyWeightFac = baseStats.bodyWeightFac + effect.bodyWeightFac;

        stats.loudnessFac = baseStats.loudnessFac + effect.loudnessFac;
        stats.generalVisibilityBonus = baseStats.generalVisibilityBonus + effect.generalVisibilityBonus;
        stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode + effect.visualStealthInSneakMode;
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
                    0 => Enums.Pearls.AS_Pearl_ThreatMusic,
                    1 => DataPearlType.CC,
                    2 => DataPearlType.LF_bottom,
                    3 => DataPearlType.Red_stomach,
                    4 => DataPearlType.DS,
                    _ => DataPearlType.Misc,
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

    // HACK
    public static bool hasSpawned = false;
    public static bool wasPressedLeft = false;
    public static bool wasPressedRight = false;


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


    // rewrite this someday 
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
