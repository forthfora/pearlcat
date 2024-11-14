using RWCustom;
using SlugBase.Features;
using UnityEngine;

namespace Pearlcat;

public static partial class PlayerAbilities_Helpers
{
    public static void UpdateSpearCreation(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (ModOptions.DisableSpear.Value || self.inVoidSea || playerModule.PossessedCreature != null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.SPEAR_CREATION);
            return;
        }

        var spearCreationTime = 30;
        playerModule.SpearLerp = Custom.LerpMap(playerModule.SpearTimer, 5, spearCreationTime, 0.0f, 1.0f);

        playerModule.ForceLockSpearOnBack = false;

        if (effect.MajorEffect != PearlEffect.MajorEffectType.SPEAR_CREATION)
        {
            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
            return;
        }

        if (playerModule.SpearCount <= 0) return;

        playerModule.ForceLockSpearOnBack = self.spearOnBack != null &&
                                            (self.spearOnBack.HasASpear != playerModule.WasSpearOnBack ||
                                             spearCreationTime < 20);

        bool IsHoldingFoodOrPlayer(Player player)
        {
            var grasps = player.grasps;

            foreach (var grasp in grasps)
            {
                if (grasp == null) continue;

                if (grasp.grabbed is Player)
                {
                    return true;
                }


                // not hungry
                if (self.CurrentFood == self.slugcatStats.maxFood) continue;

                if (grasp.grabbed is Creature creature && creature.dead &&
                    PlayerFeatures.Diet.TryGet(self, out var diet) && diet.GetFoodMultiplier(creature) > 0)
                {
                    return true;
                }


                // not a consumable object
                if (grasp.grabbed?.abstractPhysicalObject is not AbstractConsumable) continue;

                if (grasp.grabbed?.abstractPhysicalObject is AbstractConsumable consumable
                    && consumable.realizedObject != null
                    && PlayerFeatures.Diet.TryGet(self, out diet)
                    && diet.GetFoodMultiplier(consumable.realizedObject) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        var abilityInput = self.IsSpearCreationKeybindPressed(playerModule) &&
                           !self.IsStoreKeybindPressed(playerModule) && !IsHoldingFoodOrPlayer(self);

        var holdingSpear = self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0;

        //Plugin.Logger.LogWarning(self.eatCounter);

        if (abilityInput && ((self.spearOnBack == null && !holdingSpear) ||
                             (self.spearOnBack != null &&
                              (self.spearOnBack.interactionLocked || (!holdingSpear && !self.spearOnBack.HasASpear)) &&
                              !(holdingSpear && self.spearOnBack.HasASpear) &&
                              !(self.spearOnBack.HasASpear && self.onBack != null))))
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

                    var abstractSpear = new AbstractSpear(self.room.world, null,
                        self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();

                    var dataPearlType = (playerModule.ActiveObject as DataPearl.AbstractDataPearl)?.dataPearlType.value;


                    var spearModule = new SpearModule(playerModule.ActiveColor, dataPearlType ?? "");

                    if (playerModule.ActiveObject is PebblesPearl.AbstractPebblesPearl pebblesPearl)
                    {
                        spearModule.PebblesColor = pebblesPearl.color;
                    }


                    var save = self.abstractCreature.Room.world.game.GetMiscWorld();

                    // Story
                    if (save != null)
                    {
                        save.PearlSpears.Add(abstractSpear.ID.number, spearModule);
                    }
                    // Non-Story (e.g. Arena / Sandbox)
                    else
                    {
                        ModuleManager.TempPearlSpearData.Add(abstractSpear, spearModule);
                    }


                    if (self.spearOnBack != null && (holdingSpear || self.onBack != null))
                    {
                        self.spearOnBack.SpearToBack((Spear)abstractSpear.realizedObject);
                    }
                    else
                    {
                        self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                    }

                    (playerModule.ActiveObject?.realizedObject).ConnectEffect(abstractSpear.realizedObject.firstChunk.pos);

                    self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlStore, self.firstChunk, false, 0.5f,
                        Random.Range(2.2f, 2.5f));

                    if (playerModule.ActiveObject != null)
                    {
                        var activeObj = playerModule.ActiveObject;
                        self.RemoveFromInventory(playerModule.ActiveObject);

                        activeObj.destroyOnAbstraction = true;
                        activeObj.Abstractize(activeObj.pos);

                        self.UpdateInventorySaveData(playerModule);
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
            if (playerModule.SpearTimer > spearCreationTime / 2.0f)
                self.room?.AddObject(new ShockWave(playerModule.ActiveObject!.realizedObject.firstChunk.pos, 30.0f,
                    0.5f, 6));

            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
        }
    }
}
