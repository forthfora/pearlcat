namespace Pearlcat;

public static class PlayerAbilities_Helpers_Revive
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (ModOptions.DisableRevive || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Revive);
            return;
        }

        if (playerModule.ActivePearl is null ||
            !playerModule.ActivePearl.TryGetPlayerPearlModule(out var pearlModule))
        {
            return;
        }

        var abilityInput = self.IsReviveKeybindPressed(playerModule);

        if (effect.MajorEffect != PearlEffect.MajorEffectType.Revive || !abilityInput)
        {
            playerModule.ReviveTimer = 0;
            return;
        }

        if (pearlModule.CooldownTimer != 0)
        {
            return;
        }

        var shouldResetRevive = true;

        foreach (var grasp in self.grasps)
        {
            if (grasp?.grabbed is not Creature creature)
            {
                continue;
            }

            // maybe i dunno
            if (!creature.dead && !creature.State.dead && !(creature is Player deadPlayer &&
                                                            (deadPlayer.playerState.dead ||
                                                             deadPlayer.playerState.permaDead)))
            {
                continue;
            }

            self.Blink(5);

            if (playerModule.ReviveTimer % 3 == 0 && !pearlModule.IsReturningSentry)
            {
                playerModule.ActivePearl.realizedObject.ConnectEffect(creature.firstChunk.pos, syncOnline: true);
            }

            if (playerModule.ReviveTimer > 100)
            {
                playerModule.SetReviveCooldown(-1);

                if (creature is Player player)
                {
                    player.RevivePlayer();
                }
                else
                {
                    creature.Revive();

                    if (creature.killTag != self.abstractCreature)
                    {
                        creature.abstractCreature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(
                            creature.abstractCreature.creatureTemplate.communityID,
                            creature.abstractCreature.world.RegionNumber,
                            self.playerState.playerNumber,
                            1.0f, 0.0f, 0.0f);
                    }
                }
            }

            shouldResetRevive = false;
            playerModule.BlockInput = true;
            break;
        }

        if (shouldResetRevive)
        {
            playerModule.ReviveTimer = 0;
        }
        else
        {
            pearlModule.ReturnSentry(playerModule.ActivePearl);

            if (!pearlModule.IsReturningSentry)
            {
                playerModule.ReviveTimer++;
            }
        }
    }
}
