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
        On.Player.checkInput += Player_checkInput;

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
            Plugin.Logger.LogError("Player Hooks Error:\n" + e);
        }
    }

    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        CheckInput(self, playerModule);

        playerModule.baseStats = self.Malnourished ? playerModule.malnourishedStats : playerModule.normalStats;

        // Warp Fix
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

        UpdateHUD(self, playerModule);

        if (Input.GetKeyDown(KeyCode.G))
            self.RetrieveActiveObject();
    }

    private static void UpdateHUD(Player self, PlayerModule playerModule)
    {
        if (playerModule.hudFadeStacker > 0)
        {
            playerModule.hudFadeStacker--;
            playerModule.hudFade = Mathf.Lerp(playerModule.hudFade, 1.0f, 0.1f);
        }
        else
        {
            playerModule.hudFadeStacker = 0;
            playerModule.hudFade = Mathf.Lerp(playerModule.hudFade, 0.0f, 0.05f);
        }
    }

    public static void CheckInput(Player self, PlayerModule playerModule)
    {
        var input = self.input[0];
        var unblockedInput = playerModule.unblockedInput;

        bool swapLeftInput = (Input.GetKey(PearlcatOptions.swapLeftKeybind.Value) || Input.GetAxis("DschockHorizontalRight") < -0.5f) && self.IsFirstPearlcat();
        bool swapRightInput = (Input.GetKey(PearlcatOptions.swapRightKeybind.Value) || Input.GetAxis("DschockHorizontalRight") > 0.5f) && self.IsFirstPearlcat();

        bool swapInput = self.IsSwapKeybindPressed();
        bool storeInput = self.IsStoreKeybindPressed();
        bool abilityInput = self.IsAbilityKeybindPressed();
        
        int numPressed = self.IsFirstPearlcat() ? self.GetNumberPressed() : -1;


        playerModule.blockInput = false;


        if (numPressed >= 0)
            self.ActivateObjectInStorage(numPressed - 1);

        // Should probably clean this up sometime
        if (SwapRepeatInterval.TryGet(self, out var swapInterval))
        {
            // || playerModule.swapIntervalStacker > swapInterval
            if (Mathf.Abs(unblockedInput.x) <= 0.5f)
            {
                playerModule.wasSwapped = false;
                playerModule.swapIntervalStacker = 0;
            }

            if (swapInput)
            {
                playerModule.blockInput = true;

                if (playerModule.swapIntervalStacker <= swapInterval)
                    playerModule.swapIntervalStacker++;
            }
            else
            {
                playerModule.swapIntervalStacker = 0;
            }
        }

        if (swapLeftInput && !playerModule.wasSwapLeftInput)
        {
            self.SelectPreviousObject();
        }
        else if (swapRightInput && !playerModule.wasSwapRightInput)
        {
            self.SelectNextObject();
        }
        else if (swapInput && !playerModule.wasSwapped)
        {
            if (unblockedInput.x < -0.5f)
            {
                self.SelectPreviousObject();
                playerModule.wasSwapped = true;
            }
            else if (unblockedInput.x > 0.5f)
            {
                self.SelectNextObject();
                playerModule.wasSwapped = true;
            }
        }


        playerModule.wasSwapLeftInput = swapLeftInput;
        playerModule.wasSwapRightInput = swapRightInput;
        playerModule.wasStoreInput = storeInput;
        playerModule.wasAbilityInput = abilityInput;
    }

    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        
        if (!self.TryGetPearlcatModule(out var playerModule)) return;
        
        var input = self.input[0];
        playerModule.unblockedInput = input;

        if (playerModule.blockInput)
        {
            input.x = 0;
            input.y = 0;
            input.analogueDir *= 0f;

            input.jmp = false;
            input.thrw = false;
            input.pckp = false;
        }

        self.input[0] = input;
    }


    public static void UpdatePostDeathInventory(Player self, PlayerModule playerModule)
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


                if (ObjectAddon.ObjectsWithAddon.TryGetValue(item.realizedObject, out var _))
                    ObjectAddon.ObjectsWithAddon.Remove(item.realizedObject);

                self.StoreObject(item);
            }
        }
    }

    public static void UpdatePlayerDaze(Player self, PlayerModule playerModule)
    {
        if (!DazeDuration.TryGet(self, out var dazeDuration)) return;

        if (self.dead || self.bodyMode == Player.BodyModeIndex.Stunned || self.Sleeping)
            playerModule.dazeStacker = dazeDuration;

        if (playerModule.dazeStacker > 0)
            playerModule.dazeStacker--;
    }


    public static void UpdatePlayerOA(Player self, PlayerModule playerModule)
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

        if (playerModule.objectAnimationStacker > playerModule.objectAnimationDuration)
            playerModule.PickObjectAnimation(self);

        playerModule.currentObjectAnimation?.Update(self);
        playerModule.objectAnimationStacker++;


        
        // HACK
        if (!self.dead && !hasSpawned)
        {
            hasSpawned = true;

            for (int i = 0; i < 11; i++)
            {
                DataPearlType type = i switch
                {
                    0 => DataPearlType.GW,
                    1 => DataPearlType.CC,
                    2 => DataPearlType.HI,
                    3 => DataPearlType.DS,
                    4 => DataPearlType.SH,
                    5 => DataPearlType.UW,
                    6 => DataPearlType.LF_bottom,
                    7 => DataPearlType.SL_bridge,
                    8 => DataPearlType.SL_moon,
                    9 => DataPearlType.Red_stomach,
                    10 => DataPearlType.SB_filtration,
                    _ => DataPearlType.Misc,
                };

                var pearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), -1, -1, null, type);
                self.StoreObject(pearl);
            }
        }
    }

    // HACK
    public static bool hasSpawned = false;


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
        if (obj.abstractPhysicalObject.IsPlayerObject())
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
