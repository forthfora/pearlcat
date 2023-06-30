using RWCustom;
using UnityEngine;
using static AbstractPhysicalObject;
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
    }

    public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;
        
        playerModule.BaseStats = self.Malnourished ? playerModule.MalnourishedStats : playerModule.NormalStats;


        if (self.onBack != null)
            self.AbstractizeInventory();
        
        // Warp Fix
        if (self.room != null && JustWarpedData.TryGetValue(self.room.game, out var justWarped) && justWarped.Value)
        {
            self.AbstractizeInventory();
            justWarped.Value = false;
        }

        self.TryRealizeInventory();


        CheckInput(self, playerModule);

        UpdatePlayerOA(self, playerModule);
        UpdatePlayerDaze(self, playerModule);
        UpdatePostDeathInventory(self, playerModule);

        UpdateCombinedPOEffect(self, playerModule);
        ApplyCombinedPOEffect(self, playerModule);

        UpdateHUD(self, playerModule);
        //UpdateSFX(self, playerModule);

        UpdateStoreRetrieveObject(self, playerModule);
    }

    private static void UpdateStoreRetrieveObject(Player self, PlayerModule playerModule)
    {

        if (!StoreObjectDelay.TryGet(self, out var storeObjectDelay)) return;

        var storeInput = self.IsStoreKeybindPressed();
        var isStoring = self.GraspsHasType(AbstractObjectType.DataPearl) == 0;
        var toStore = self.grasps[0]?.grabbed;

        if (isStoring && toStore == null) return;

        if (!isStoring && self.FreeHand() == -1) return;


        if (playerModule.StoreObjectStacker > storeObjectDelay)
        {
            if (isStoring)
            {
                self.ReleaseGrasp(0);
                self.StoreObject(toStore!.abstractPhysicalObject);
            }
            else
            {
                self.RetrieveActiveObject();
            }

            playerModule.StoreObjectStacker = -int.MaxValue;
        }
        

        if (storeInput)
            playerModule.StoreObjectStacker++;
        
        else
            playerModule.StoreObjectStacker = 0;
    }

    private static void UpdateSFX(Player self, PlayerModule playerModule)
    {
        playerModule.MenuCrackleLoop.Update();
        playerModule.MenuCrackleLoop.Volume = playerModule.HudFade;
    }

    private static void UpdateHUD(Player self, PlayerModule playerModule)
    {
        if (playerModule.HudFadeStacker > 0)
        {
            playerModule.HudFadeStacker--;
            playerModule.HudFade = Mathf.Lerp(playerModule.HudFade, 1.0f, 0.1f);
        }
        else
        {
            playerModule.HudFadeStacker = 0;
            playerModule.HudFade = Mathf.Lerp(playerModule.HudFade, 0.0f, 0.05f);
        }
    }

    public static void CheckInput(Player self, PlayerModule playerModule)
    {
        var input = self.input[0];
        var unblockedInput = playerModule.UnblockedInput;

        bool swapLeftInput = (Input.GetKey(PearlcatOptions.swapLeftKeybind.Value) || Input.GetAxis("DschockHorizontalRight") < -0.5f) && self.IsFirstPearlcat();
        bool swapRightInput = (Input.GetKey(PearlcatOptions.swapRightKeybind.Value) || Input.GetAxis("DschockHorizontalRight") > 0.5f) && self.IsFirstPearlcat();

        bool swapInput = self.IsSwapKeybindPressed();
        bool storeInput = self.IsStoreKeybindPressed();
        bool abilityInput = self.IsAbilityKeybindPressed();
        
        int numPressed = self.IsFirstPearlcat() ? self.GetNumberPressed() : -1;


        playerModule.BlockInput = false;


        if (numPressed >= 0)
            self.ActivateObjectInStorage(numPressed - 1);

        // Should probably clean this up sometime
        if (SwapRepeatInterval.TryGet(self, out var swapInterval))
        {
            // || playerModule.swapIntervalStacker > swapInterval
            if (Mathf.Abs(unblockedInput.x) <= 0.5f)
            {
                playerModule.WasSwapped = false;
                playerModule.SwapIntervalStacker = 0;
            }

            if (swapInput)
            {
                playerModule.BlockInput = true;

                if (playerModule.SwapIntervalStacker <= swapInterval)
                    playerModule.SwapIntervalStacker++;
            }
            else
            {
                playerModule.SwapIntervalStacker = 0;
            }
        }

        if (swapLeftInput && !playerModule.WasSwapLeftInput)
        {
            self.SelectPreviousObject();
        }
        else if (swapRightInput && !playerModule.WasSwapRightInput)
        {
            self.SelectNextObject();
        }
        else if (swapInput && !playerModule.WasSwapped)
        {
            if (unblockedInput.x < -0.5f)
            {
                self.SelectPreviousObject();
                playerModule.WasSwapped = true;
            }
            else if (unblockedInput.x > 0.5f)
            {
                self.SelectNextObject();
                playerModule.WasSwapped = true;
            }
        }


        playerModule.WasSwapLeftInput = swapLeftInput;
        playerModule.WasSwapRightInput = swapRightInput;
        playerModule.WasStoreInput = storeInput;
        playerModule.WasAbilityInput = abilityInput;
    }

    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        
        if (!self.TryGetPearlcatModule(out var playerModule)) return;
        
        var input = self.input[0];
        playerModule.UnblockedInput = input;

        if (playerModule.BlockInput)
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
        if (!self.dead && playerModule.PostDeathInventory.Count > 0)
        {
            for (int i = playerModule.PostDeathInventory.Count - 1; i >= 0; i--)
            {
                AbstractPhysicalObject? item = playerModule.PostDeathInventory[i];
                playerModule.PostDeathInventory.RemoveAt(i);

                if (item.realizedObject == null) continue;

                if (item.realizedObject.room != self.room) continue;

                if (item.realizedObject.grabbedBy.Count > 0) continue;


                if (ObjectAddon.ObjectsWithAddon.TryGetValue(item, out var _))
                    ObjectAddon.ObjectsWithAddon.Remove(item);

                self.StoreObject(item);
            }
        }
    }

    public static void UpdatePlayerDaze(Player self, PlayerModule playerModule)
    {
        if (!DazeDuration.TryGet(self, out var dazeDuration)) return;

        if (self.dead || self.bodyMode == Player.BodyModeIndex.Stunned || self.Sleeping)
            playerModule.DazeStacker = dazeDuration;

        if (playerModule.DazeStacker > 0)
            playerModule.DazeStacker--;
    }


    public static void UpdatePlayerOA(Player self, PlayerModule playerModule)
    {
        if (playerModule.CurrentObjectAnimation is FreeFallOA)
        {
            if (self.bodyMode != Player.BodyModeIndex.Stunned && self.bodyMode != Player.BodyModeIndex.Dead && !self.Sleeping)
            {
                foreach (var abstractObject in playerModule.AbstractInventory)
                    abstractObject.realizedObject.ConnectEffect(((PlayerGraphics)self.graphicsModule).head.pos);

                playerModule.PickObjectAnimation(self);
            }
        }
        else if (self.bodyMode == Player.BodyModeIndex.Stunned || self.bodyMode == Player.BodyModeIndex.Dead || self.Sleeping)
        {
            playerModule.CurrentObjectAnimation = new FreeFallOA(self);
        }

        if (playerModule.ObjectAnimationStacker > playerModule.ObjectAnimationDuration)
            playerModule.PickObjectAnimation(self);

        playerModule.CurrentObjectAnimation?.Update(self);
        playerModule.ObjectAnimationStacker++;


        
        // HACK
        if (!self.dead && !hasSpawned)
        {
            hasSpawned = true;

            for (int i = 0; i < 6; i++)
            {
                DataPearlType type = i switch
                {
                    0 => MoreSlugcats.MoreSlugcatsEnums.DataPearlType.RM,
                    1 => Enums.Pearls.AS_PearlBlue,
                    2 => Enums.Pearls.AS_PearlYellow,
                    3 => Enums.Pearls.AS_PearlRed,
                    4 => Enums.Pearls.AS_PearlGreen,
                    5 => Enums.Pearls.AS_PearlBlack,
                    6 => DataPearlType.LF_bottom,
                    7 => DataPearlType.SL_chimney,
                    8 => DataPearlType.SL_bridge,
                    9 => DataPearlType.HI,
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


        for (int i = playerModule.AbstractInventory.Count - 1; i >= 0; i--)
        {
            AbstractPhysicalObject abstractObject = playerModule.AbstractInventory[i];

            DeathEffect(abstractObject.realizedObject);
            RemoveFromInventory(self, abstractObject);

            playerModule.PostDeathInventory.Add(abstractObject);
        }
    }


    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);

        //StoreObjectUpdate(self);

        //TransferObjectUpdate(self);
    }

    public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj.abstractPhysicalObject.IsPlayerObject())
            return Player.ObjectGrabability.CantGrab;

        return orig(self, obj);
    }

    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player player && player.TryGetPearlcatModule(out _))
            player.AbstractizeInventory();

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
