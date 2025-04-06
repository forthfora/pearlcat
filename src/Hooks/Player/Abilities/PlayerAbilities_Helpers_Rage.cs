using MoreSlugcats;
using RWCustom;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_Rage
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        foreach (var item in playerModule.Inventory)
        {
            if (item.TryGetPearlGraphicsModule(out var pearlGraphics))
            {
                pearlGraphics.IsActiveRagePearl = false;
            }
        }

        playerModule.RageAnimTimer++;


        if (ModOptions.OldRedPearlAbility)
        {
            UpdateOldRage(self, playerModule, effect);
            return;
        }

        if (ModOptions.DisableRage || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Rage);
            return;
        }


        if (playerModule.StoreObjectTimer > 0)
        {
            return;
        }

        if (effect.MajorEffect != PearlEffect.MajorEffectType.Rage)
        {
            return;
        }

        if (self.room is null)
        {
            return;
        }

        if (!self.Consious)
        {
            return;
        }

        if (self.Sleeping)
        {
            return;
        }


        // Get all rage pearls in inventory
        List<DataPearl> ragePearls = [];

        foreach (var item in playerModule.Inventory)
        {
            var itemEffect = item.GetPearlEffect();

            if (itemEffect.MajorEffect != PearlEffect.MajorEffectType.Rage)
            {
                continue;
            }

            if (item.TryGetSentry(out _))
            {
                continue;
            }

            if (item.realizedObject is not DataPearl pearl)
            {
                continue;
            }

            ragePearls.Add(pearl);
        }


        // Update the pearls positions and abilities
        var origin = self.firstChunk.pos;
        var angleFrameAddition = -Custom.LerpMap(ragePearls.Count, 1, 6, 0.05f, 0.025f);
        var radius = 80.0f;

        // Restrict how many rage pearls can be active in the inventory, targeting doesn't work well with too many
        var maxActiveCount = 8;

        for (var i = 0; i < ragePearls.Count; i++)
        {
            if (i >= maxActiveCount)
            {
                break;
            }

            var ragePearl = ragePearls[i];

            if (!ragePearl.abstractPhysicalObject.TryGetPearlGraphicsModule(out var pearlGraphics))
            {
                continue;
            }

            pearlGraphics.IsActiveRagePearl = true;

            var angle = (i * Mathf.PI * 2.0f / Mathf.Clamp(ragePearls.Count, 1, maxActiveCount)) + angleFrameAddition * playerModule.RageAnimTimer;
            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

            PlayerPearl_Helpers_Data.AnimateToTargetPos(ragePearl.abstractPhysicalObject, targetPos, playerModule);
        }

        foreach (var ragePearl in ragePearls)
        {
            RageTargetLogic(ragePearl, self, false);
        }
    }

    public static void RageTargetLogic(DataPearl pearl, Player player, bool isSentry)
    {
        if (!pearl.abstractPhysicalObject.TryGetPlayerPearlModule(out var module))
        {
            return;
        }

        if (pearl.room is null)
        {
            return;
        }

        var targetPearlRange = 1500.0f;
        var targetEnemyRange = 1500.0f;
        var redirectRange = isSentry ? 50.0f : 30.0f;

        var riccochetVel = 60.0f;

        var riccochetDamageMult = 1.25f;
        var riccochetDamageMultUpDownThrow = 2.0f;
        var riccochetDamageMultSentry = 1.25f;


        // Target Finding
        // KVP = target : distance
        var availableEnemies = new List<KeyValuePair<Creature, float>>();
        var availableReds = new List<KeyValuePair<PhysicalObject, float>>();

        foreach (var roomObject in pearl.room.physicalObjects)
        {
            foreach (var physObj in roomObject)
            {
                // Reset visited objects
                if (physObj is Weapon weapon)
                {
                    if (weapon.mode == Weapon.Mode.Carried && module.VisitedObjects.TryGetValue(physObj, out _))
                    {
                        module.VisitedObjects.Remove(physObj);
                    }
                }
                // Targeting other rage pearls
                else if (physObj.abstractPhysicalObject.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.Rage)
                {
                    if (physObj == pearl)
                    {
                        continue;
                    }

                    if (isSentry)
                    {
                        // Sentry redirections only target other sentries
                        if (!physObj.abstractPhysicalObject.TryGetSentry(out _))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Inventory redirections can target sentries and active red pearls (so in theory they could even ping off another Pearlcat's red pearls)
                        if (!physObj.abstractPhysicalObject.TryGetSentry(out _))
                        {
                            // Active red check
                            if (!physObj.abstractPhysicalObject.TryGetPearlGraphicsModule(out var graphics) ||
                                !graphics.IsActiveRagePearl)
                            {
                                continue;
                            }

                            // Underground check
                            if (player.canJump > 0 && physObj.firstChunk.pos.y < player.firstChunk.pos.y + 20.0f)
                            {
                                continue;
                            }
                        }
                    }

                    if (!pearl.room.VisualContact(pearl.firstChunk.pos, physObj.firstChunk.pos))
                    {
                        continue;
                    }


                    var dist = Custom.Dist(physObj.firstChunk.pos, pearl.firstChunk.pos);

                    if (dist > targetPearlRange)
                    {
                        continue;
                    }

                    availableReds.Add(new(physObj, dist));
                }
                // Targeting creatures
                else if (physObj is Creature creature)
                {
                    if (creature is Cicada)
                    {
                        continue;
                    }

                    if (creature is Centipede centipede && centipede.Small)
                    {
                        continue;
                    }

                    // Tutorial flies are VERY HOSTILE
                    if (!player.IsHostileToMe(creature) && !(pearl.abstractPhysicalObject.Room.name == "T1_CAR2" && creature is Fly))
                    {
                        // Exception for PVP - Pearlcat who deployed the sentry is still a valid target
                        if (!creature.abstractCreature.world.game.IsFriendlyFireEnabled() || creature != player)
                        {
                            continue;
                        }
                    }

                    if (creature.dead)
                    {
                        continue;
                    }

                    if (creature.VisibilityBonus < -0.5f)
                    {
                        continue;
                    }


                    if (!pearl.room.VisualContact(pearl.firstChunk.pos, creature.mainBodyChunk.pos))
                    {
                        continue;
                    }


                    var dist = Custom.Dist(creature.mainBodyChunk.pos, pearl.firstChunk.pos);

                    if (dist > targetEnemyRange)
                    {
                        continue;
                    }

                    availableEnemies.Add(new(creature, dist));
                }
            }
        }

        // Order by distance
        availableReds = availableReds.OrderBy(x => x.Value).ToList();
        availableEnemies = availableEnemies.OrderBy(x => x.Value).ToList();

        // Redirection
        foreach (var layer in pearl.room.physicalObjects)
        {
            foreach (var physObj in layer)
            {
                if (!Custom.DistLess(pearl.firstChunk.pos, physObj.firstChunk.pos, redirectRange))
                {
                    continue;
                }

                if (physObj is not Weapon weapon)
                {
                    continue;
                }

                if (weapon.mode != Weapon.Mode.Thrown)
                {
                    continue;
                }

                if (module.VisitedObjects.TryGetValue(physObj, out _))
                {
                    continue;
                }


                PhysicalObject? bestRed = null;

                foreach (var kvp in availableReds)
                {
                    if (!kvp.Key.abstractPhysicalObject.TryGetPlayerPearlModule(out var otherSentryModule))
                    {
                        continue;
                    }

                    if (otherSentryModule.VisitedObjects.TryGetValue(weapon, out _))
                    {
                        continue;
                    }

                    bestRed = kvp.Key;
                    break;
                }


                Creature? bestEnemy = null;

                foreach (var kvp in availableEnemies)
                {
                    // Never target the thrower of the weapon
                    if (kvp.Key == weapon.thrownBy)
                    {
                        continue;
                    }

                    bestEnemy = kvp.Key;
                    break;
                }

                PhysicalObject? bestTarget = null;
                Vector2? bestTargetPos = null!;
                var bestTargetVel = Vector2.zero;

                if (bestRed is not null && bestEnemy is not null)
                {
                    if (player.room.VisualContact(bestRed.firstChunk.pos, bestEnemy.firstChunk.pos))
                    {
                        bestTarget = bestRed;
                    }
                    else
                    {
                        bestTarget = bestEnemy;
                    }
                }
                else if (bestRed is not null && bestRed.abstractPhysicalObject.TryGetSentry(out _))
                {
                    bestTarget = bestRed;
                }
                else if (bestEnemy is not null)
                {
                    bestTarget = bestEnemy;
                }


                if (bestTarget is not null)
                {
                    if (bestTarget == bestEnemy)
                    {
                        bestTargetPos = bestEnemy.mainBodyChunk.pos;
                        bestTargetVel = bestEnemy.mainBodyChunk.vel;

                        if (bestEnemy is Vulture vulture)
                        {
                            bestTargetPos = vulture.Head().pos;
                            bestTargetVel = vulture.Head().vel;
                        }
                    }
                    else if (bestTarget == bestRed)
                    {
                        bestTargetPos = bestRed.firstChunk.pos;
                        bestTargetVel = bestRed.firstChunk.vel;
                    }
                }


                var pearlColor = pearl.abstractPhysicalObject.GetObjectColor();

                // No valid targets
                if (bestTargetPos is null || bestTarget is null)
                {
                    // Only stop if sentry
                    if (isSentry)
                    {
                        weapon.firstChunk.vel = Vector2.zero;
                        weapon.room.AddObject(new LightningMachine.Impact(weapon.firstChunk.pos, 0.5f, pearlColor, true));
                        pearl.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos, 0.5f, 0.6f);
                    }
                    continue;
                }


                if (weapon is Spear spear)
                {
                    float mult;

                    if (isSentry)
                    {
                        mult = riccochetDamageMultSentry;
                    }
                    else
                    {
                        if (weapon.throwDir.y != 0 && weapon.throwModeFrames < 90)
                        {
                            mult = riccochetDamageMultUpDownThrow;
                            pearl.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos, 1.5f, 6.0f);
                        }
                        else
                        {
                            mult = riccochetDamageMult;
                        }
                    }

                    spear.spearDamageBonus *= mult;
                }


                var targetLeadPos = GetTargetLeadPos(weapon.firstChunk.pos, weapon.thrownBy?.firstChunk?.vel ?? Vector2.zero, (Vector2)bestTargetPos, bestTargetVel, riccochetVel, weapon.gravity);
                var leadDir = Custom.DirVec(weapon.firstChunk.pos, targetLeadPos);

                weapon.firstChunk.vel = leadDir * riccochetVel;
                weapon.setRotation = leadDir;

                weapon.rotationSpeed = 0.0f;
                weapon.throwModeFrames = 180;

                module.VisitedObjects.Add(physObj, new());

                var room = pearl.room;

                if (bestTarget == bestEnemy)
                {
                    room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos, 1.0f, 1.5f);
                    room.PlaySound(SoundID.Fire_Spear_Explode, pearl.firstChunk.pos, 0.6f, 1.5f);
                }
                else
                {
                    room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos, 0.5f, 3.0f);
                }

                room.AddObject(new LightningMachine.Impact(pearl.firstChunk.pos, 0.5f, pearlColor, true));
                room.AddObject(new ExplosionSpikes(pearl.room, pearl.firstChunk.pos, 10, 15.0f, 15, 5.0f, 70.0f,
                    pearlColor));

                if (pearl.abstractPhysicalObject.TryGetPearlGraphicsModule(out var pearlGraphics))
                {
                    pearlGraphics.LaserTarget = (Vector2)bestTargetPos;
                    pearlGraphics.LaserLerp = 1.0f;
                }
            }
        }
    }

    // Returns the relevant lead position given a target and shooter so that a projectile fired from the shooter will hit the target
    // https://www.youtube.com/watch?v=ZjMjxj_blMQ <- great vid
    private static Vector2 GetTargetLeadPos(Vector2 shooterPos, Vector2 shooterVel, Vector2 targetPos, Vector2 targetVel, float projectileSpeed, float projectileGravity)
    {
        var a = projectileSpeed * projectileSpeed - Vector2.Dot(targetVel, targetVel);
        var b = 2.0f * Vector2.Dot(targetVel,  targetPos - shooterPos);
        var c = Vector2.Dot(targetPos - shooterPos, targetPos - shooterPos);

        var time = 0.0f;

        if (projectileSpeed > targetVel.magnitude)
        {
            // good 'ol quadratic formula
            time = (b + Mathf.Sqrt(b * b + 4 * a * c)) / (2 * a);
        }

        var targetPredictedPos = targetPos + time * targetVel;

        // Compensate for shooter's initial velocity (e.g. if player throws spear while falling)
        var leadPos = targetPredictedPos - shooterVel;

        // Compensate for weapon drop due to gravity
        // for some reason, compensation is too strong if we just consider weapon gravity
        var compensationFac = Custom.LerpMap(Custom.Dist(shooterPos, targetPos), 0.0f, 1000.0f, 1.0f, 0.5f);

        // 1/2 * a * t^2
        leadPos += Vector2.up * 0.5f * projectileGravity * time * time * compensationFac;

        return leadPos;
    }

    private static void UpdateOldRage(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        var shootTime = ModOptions.LaserWindupTime;
        var cooldownTime = ModOptions.LaserRechargeTime;
        var shootDamage = ModOptions.LaserDamage;

        var ragePearlCounter = 0;

        foreach (var item in playerModule.Inventory)
        {
            if (!item.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            var itemEffect = item.GetPearlEffect();

            if (itemEffect.MajorEffect != PearlEffect.MajorEffectType.Rage)
            {
                continue;
            }

            if (item.TryGetSentry(out _))
            {
                continue;
            }

            module.LaserLerp = 0.0f;

            if (effect.MajorEffect != PearlEffect.MajorEffectType.Rage || playerModule.RageTarget is null ||
                !playerModule.RageTarget.TryGetTarget(out _))
            {
                module.LaserTimer = shootTime + ragePearlCounter * 5;
            }

            ragePearlCounter++;
        }

        if (ModOptions.DisableRage || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Rage);
            return;
        }

        if (effect.MajorEffect != PearlEffect.MajorEffectType.Rage)
        {
            return;
        }

        if (self.room is null)
        {
            return;
        }

        if (!self.Consious)
        {
            return;
        }


        var playerRoom = self.room;

        // search for target
        if (playerModule.RageTarget is null || !playerModule.RageTarget.TryGetTarget(out var target))
        {
            Creature? bestTarget = null;
            var shortestDist = float.MaxValue;

            foreach (var roomObject in playerRoom.physicalObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not Creature creature)
                    {
                        continue;
                    }

                    if (creature is Cicada)
                    {
                        continue;
                    }

                    if (creature is Centipede centipede && centipede.Small)
                    {
                        continue;
                    }

                    // Fly exception for the tutorial
                    if (!self.IsHostileToMe(creature) && !(self.abstractPhysicalObject.Room.name == "T1_CAR2" && creature is Fly))
                    {
                        continue;
                    }


                    if (creature.dead)
                    {
                        continue;
                    }

                    if (creature.VisibilityBonus < -0.5f)
                    {
                        continue;
                    }


                    var dist = Custom.Dist(creature.mainBodyChunk.pos, self.firstChunk.pos);

                    if (dist > 400.0f)
                    {
                        continue;
                    }

                    if (dist > shortestDist)
                    {
                        continue;
                    }


                    if (!self.room.VisualContact(self.mainBodyChunk.pos, creature.mainBodyChunk.pos))
                    {
                        continue;
                    }

                    shortestDist = dist;
                    bestTarget = creature;
                }
            }

            if (bestTarget is not null)
            {
                playerModule.RageTarget = new(bestTarget);

                ragePearlCounter = 0;

                if (bestTarget is Spider)
                {
                    foreach (var item in playerModule.Inventory)
                    {
                        if (!item.TryGetPlayerPearlModule(out var module))
                        {
                            continue;
                        }

                        var itemEffect = item.GetPearlEffect();

                        if (itemEffect.MajorEffect != PearlEffect.MajorEffectType.Rage)
                        {
                            continue;
                        }

                        module.LaserTimer = 7 + 3 * ragePearlCounter;
                        ragePearlCounter++;
                    }
                }
            }
        }
        else
        {
            // ensure target is still valid
            var invalidTarget = false;

            if (!Custom.DistLess(target.mainBodyChunk.pos, self.mainBodyChunk.pos, 500.0f))
            {
                invalidTarget = true;
            }

            if (target.room != self.room)
            {
                invalidTarget = true;
            }

            if (target.dead)
            {
                invalidTarget = true;
            }

            if (!self.room.VisualContact(self.mainBodyChunk.pos, target.mainBodyChunk.pos))
            {
                invalidTarget = true;
            }


            if (invalidTarget)
            {
                playerModule.RageTarget = null;
            }
        }


        if (playerModule.RageTarget is null || !playerModule.RageTarget.TryGetTarget(out target))
        {
            return;
        }

        foreach (var item in playerModule.Inventory)
        {
            if (!item.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            if (!item.TryGetPearlGraphicsModule(out var pearlGraphics))
            {
                continue;
            }


            var itemEffect = item.GetPearlEffect();

            if (itemEffect.MajorEffect != PearlEffect.MajorEffectType.Rage)
            {
                continue;
            }

            if (item.TryGetSentry(out _))
            {
                continue;
            }

            if (module.CooldownTimer > 0)
            {
                module.LaserTimer = shootTime;
                continue;
            }

            if (module.LaserTimer <= 0)
            {
                module.CooldownTimer = cooldownTime;

                var targetPos = target.mainBodyChunk.pos;

                // shoot laser
                self.room.PlaySound(SoundID.Bomb_Explode, targetPos, 0.8f, Random.Range(0.7f, 1.3f));
                self.room.AddObject(new LightningMachine.Impact(targetPos, 0.5f, pearlGraphics.SymbolColor, true));

                self.room.AddObject(new ShockWave(targetPos, 30.0f, 0.4f, 5));
                self.room.AddObject(new ExplosionSpikes(self.room, targetPos, 5, 20.0f, 10, 20.0f, 20.0f,
                    pearlGraphics.SymbolColor));

                target.SetKillTag(self.abstractCreature);
                target.Violence(self.mainBodyChunk, null, target.mainBodyChunk, null, Creature.DamageType.Explosion,
                    shootDamage, 5.0f);
            }
            else
            {
                module.LaserTimer--;
            }

            module.LaserLerp = Custom.LerpMap(module.LaserTimer, shootTime, 0, 0.0f, 1.0f);
        }
    }

}
