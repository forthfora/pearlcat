using RWCustom;
using SlugBase.Features;
using UnityEngine;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_SpearCreation
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (ModOptions.DisableSpear || self.inVoidSea || playerModule.PossessedCreature is not null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.SpearCreation);
            return;
        }

        var spearCreationTime = 30;
        playerModule.SpearLerp = Custom.LerpMap(playerModule.SpearTimer, 5, spearCreationTime, 0.0f, 1.0f);

        playerModule.ForceLockSpearOnBack = false;

        if (effect.MajorEffect != PearlEffect.MajorEffectType.SpearCreation)
        {
            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
            return;
        }

        if (playerModule.SpearCount <= 0)
        {
            return;
        }

        playerModule.ForceLockSpearOnBack = self.spearOnBack is not null && (self.spearOnBack.HasASpear != playerModule.WasSpearOnBack);

        var abilityInput = self.IsSpearCreationKeybindPressed(playerModule) &&
                           !self.IsStoreKeybindPressed(playerModule) && !IsHoldingFoodOrPlayer(self, self);

        var holdingSpear = self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0;

        if (abilityInput && ((self.spearOnBack is null && !holdingSpear) ||
                             (self.spearOnBack is not null &&
                              (self.spearOnBack.interactionLocked || (!holdingSpear && !self.spearOnBack.HasASpear)) &&
                              !(holdingSpear && self.spearOnBack.HasASpear) &&
                              !(self.spearOnBack.HasASpear && self.onBack is not null))))
        {
            playerModule.ForceLockSpearOnBack = true;

            if (playerModule.SpearDelay > 10)
            {
                playerModule.BlockInput = true;
                playerModule.SpearTimer++;
                self.Blink(5);

                if (playerModule.SpearTimer > spearCreationTime)
                {
                    playerModule.SpearTimer = 0;

                    var abstractSpear = new AbstractSpear(self.abstractCreature.world, null,
                        self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.abstractCreature.world.game.GetNewID(), false);
                    self.abstractCreature.Room.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();

                    var dataPearlType = (playerModule.ActivePearl as DataPearl.AbstractDataPearl)?.dataPearlType.value;


                    var spearModule = new SpearModule(playerModule.ActiveColor, dataPearlType ?? "");

                    if (playerModule.ActivePearl is PebblesPearl.AbstractPebblesPearl pebblesPearl)
                    {
                        spearModule.PebblesColor = pebblesPearl.color;
                    }


                    var save = self.abstractCreature.world.game.GetMiscWorld();

                    // Story
                    if (save is not null)
                    {
                        save.PearlSpears.Add(abstractSpear.ID.number, spearModule);
                    }
                    // Non-Story (e.g. Arena / Sandbox)
                    else
                    {
                        ModuleManager.TempPearlSpearData.Add(abstractSpear, spearModule);
                    }

                    if (ModCompat_Helpers.RainMeadow_IsOnline)
                    {
                        MeadowCompat.AddMeadowPearlSpearData(abstractSpear);
                    }


                    if (self.spearOnBack is not null && (holdingSpear || self.onBack is not null))
                    {
                        self.spearOnBack.SpearToBack((Spear)abstractSpear.realizedObject);
                    }
                    else
                    {
                        self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                    }

                    (playerModule.ActivePearl?.realizedObject).ConnectEffect(abstractSpear.realizedObject.firstChunk.pos, syncOnline: true);

                    self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlStore, self.firstChunk, false, 0.5f,
                        Random.Range(2.2f, 2.5f));

                    if (playerModule.ActivePearl is not null)
                    {
                        var activeObj = playerModule.ActivePearl;
                        self.RemoveFromInventory(playerModule.ActivePearl);

                        activeObj.realizedObject.Destroy();
                        activeObj.Destroy();

                        self.UpdateInventorySaveData();
                    }
                }
            }
            else
            {
                playerModule.SpearDelay++;
            }
        }
        else
        {
            // SpearTimer sync makes it look weird
            if (playerModule.SpearTimer > spearCreationTime / 2.0f && !ModCompat_Helpers.RainMeadow_IsOnline)
            {
                self.room?.AddObject(new ShockWave(playerModule.ActivePearl!.realizedObject.firstChunk.pos, 30.0f,
                    0.5f, 6));
            }

            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
        }
    }

    private static bool IsHoldingFoodOrPlayer(Player player, Player self)
    {
        var grasps = player.grasps;

        foreach (var grasp in grasps)
        {
            if (grasp is null)
            {
                continue;
            }

            if (grasp.grabbed is Player)
            {
                return true;
            }


            // not hungry
            if (self.CurrentFood == self.slugcatStats.maxFood)
            {
                continue;
            }

            if (grasp.grabbed is Creature creature && creature.dead &&
                PlayerFeatures.Diet.TryGet(self, out var diet) && diet.GetFoodMultiplier(creature) > 0)
            {
                return true;
            }


            // not a consumable object
            if (grasp.grabbed?.abstractPhysicalObject is not AbstractConsumable)
            {
                continue;
            }

            if (grasp.grabbed?.abstractPhysicalObject is AbstractConsumable consumable
                && consumable.realizedObject is not null
                && PlayerFeatures.Diet.TryGet(self, out diet)
                && diet.GetFoodMultiplier(consumable.realizedObject) > 0)
            {
                return true;
            }
        }

        return false;
    }
}
