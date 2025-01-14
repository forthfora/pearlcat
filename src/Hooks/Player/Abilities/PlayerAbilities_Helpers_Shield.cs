using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_Shield
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (playerModule.ShieldTimer > 0)
        {
            self.AllGraspsLetGoOfThisObject(false);
            playerModule.ShieldTimer--;

            self.airInLungs = 1.0f;

            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 1.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 6.0f, 0.4f);

            if (playerModule.ShieldTimer % 3 == 0)
            {
                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var item = playerModule.Inventory[i];

                    if (i >= PlayerPearl_Helpers_Graphics.MaxPearlsWithEffects)
                    {
                        break;
                    }

                    if (ModOptions.HidePearls.Value)
                    {
                        if (item != playerModule.ActiveObject)
                        {
                            continue;
                        }
                    }

                    var itemEffect = item.GetPearlEffect();

                    if (!item.TryGetPlayerPearlModule(out var module))
                    {
                        continue;
                    }

                    if (module.CooldownTimer != 0)
                    {
                        continue;
                    }

                    if (itemEffect.MajorEffect == PearlEffect.MajorEffectType.Shield && !item.TryGetSentry(out _))
                    {
                        item.realizedObject.ConnectEffect(self.firstChunk.pos);
                    }
                }
            }

            if (playerModule.ShieldTimer == 0)
            {
                self.room?.PlaySound(Enums.Sounds.Pearlcat_ShieldOff, self.firstChunk);
            }
        }
        else
        {
            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 0.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 0.0f, 0.4f);
        }

        if (self.airInLungs < 0.1f && playerModule.ShieldActive)
        {
            playerModule.ActivateVisualShield();
        }

        if (self.room is null)
        {
            return;
        }

        var roomObjects = self.room.updateList;
        var shouldActivate = false;

        if (ModOptions.DisableShield.Value || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Shield);
            return;
        }

        if (playerModule.ShieldActive)
        {
            for (var i = roomObjects.Count - 1; i >= 0; i--)
            {
                var obj = roomObjects[i];

                if (obj is Weapon weapon)
                {
                    if (weapon.thrownBy == self)
                    {
                        continue;
                    }

                    // Thrown by another player
                    if (weapon.thrownBy is Player playerThrownBy)
                    {
                        // Thrown by a player we are on the back of
                        if (playerThrownBy.onBack == self)
                        {
                            continue;
                        }

                        // Jolly FF is off, doesn't apply to arena sessions, also check meadow FF if it's enabled
                        if (!self.abstractCreature.world.game.IsArenaSession && !Utils.RainWorld.options.friendlyFire && !ModCompat_Helpers.RainMeadow_FriendlyFire)
                        {
                            continue;
                        }

                        // Arena FF is off, only applies to arena sessions
                        if (self.abstractCreature.world.game.IsArenaSession && !self.abstractCreature.world.game
                                .GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
                        {
                            continue;
                        }
                    }


                    // When possessing a creature, don't let the spears activate our shield (only relevant for possessing scavs really)
                    if (playerModule.PossessedCreature?.TryGetTarget(out var possessed) == true &&
                        possessed.realizedCreature == weapon.thrownBy)
                    {
                        continue;
                    }


                    if (weapon.mode == Weapon.Mode.Thrown &&
                        Custom.DistLess(weapon.firstChunk.pos, self.firstChunk.pos, 75.0f))
                    {
                        weapon.ChangeMode(Weapon.Mode.Free);
                        weapon.SetRandomSpin();
                        weapon.firstChunk.vel *= -0.2f;

                        weapon.room.DeflectEffect(weapon.firstChunk.pos);
                        shouldActivate = true;
                    }
                }
                else if (obj is LizardSpit spit)
                {
                    if (playerModule.ShieldTimer > 0 && Custom.DistLess(spit.pos, self.firstChunk.pos, 75.0f))
                    {
                        spit.vel = Vector2.zero;

                        if (playerModule.ShieldTimer <= 0)
                        {
                            spit.room.DeflectEffect(spit.pos);
                        }
                    }
                }
                else if (obj is DartMaggot dart)
                {
                    if (dart.mode != DartMaggot.Mode.Free)
                    {
                        if (Custom.DistLess(dart.firstChunk.pos, self.firstChunk.pos, 75.0f))
                        {
                            dart.mode = DartMaggot.Mode.Free;
                            dart.firstChunk.vel = Vector2.zero;

                            dart.room.DeflectEffect(dart.firstChunk.pos);
                            shouldActivate = true;
                        }
                    }
                }
            }
        }

        if (shouldActivate)
        {
            playerModule.ActivateVisualShield();
        }
    }
}
