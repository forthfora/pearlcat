using RWCustom;
using System;
using System.Linq;
using UnityEngine;
using static Pearlcat.PearlEffect;
using MoreSlugcats;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using Music;

namespace Pearlcat;

public class PearlSentry : UpdatableAndDeletable, IDrawable
{
    public WeakReference<AbstractPhysicalObject> OwnerRef { get; }
    public Vector2 InitialPos { get; }

    public float HaloScale { get; set; } = 1.0f;
    public int AnimCounter { get; set; }

    public bool SpearBombArmed { get; set; }
    public AbstractRoom? SpearBombRoom { get; set; }

    public float ShieldTimer { get; set; } = -1;
    public DynamicSoundLoop? ShieldHoldLoop { get; set; }
    public Dictionary<ShortcutData, FSprite> LockedShortcutsSprites { get; } = new();

    public int RageCounter { get; set; } = 3;
    public WeakReference<Creature>? RageTarget { get; set; }

    public Vector2? AgilityPos { get; set; }
    public AbstractRoom? AgilityRoom { get; set; }

    public float HoloLightScale { get; set; }
    public float HoloLightAlpha { get; set; }
    public bool HoloLightActive { get; set; }

    public bool PlaysMusic { get; set; }
    public bool WasPlayingMusic { get; set; }
    public float MusicVolume { get; set; }


    public PearlSentry(AbstractPhysicalObject owner)
    {
        OwnerRef = new(owner);

        if (ModuleManager.SentryData.TryGetValue(owner, out _))
        {
            return;
        }

        if (owner.realizedObject == null)
        {
            return;
        }

        var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (playerModule == null)
        {
            return;
        }

        if (!playerModule.PlayerRef.TryGetTarget(out var player))
        {
            return;
        }

        ModuleManager.SentryData.Add(owner, this);

        room = owner.realizedObject.room;
        InitialPos = player.GetActivePearlPos();

        var effect = owner.GetPearlEffect();

        if (!owner.TryGetPearlGraphicsModule(out var addon))
        {
            return;
        }

        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 2.0f);

        //room.AddObject(new ShockWave(InitialPos, 30.0f, 0.5f, 10));
        room.AddObject(new ExplosionSpikes(room, InitialPos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
        room.AddObject(new LightningMachine.Impact(InitialPos, 0.1f, addon.SymbolColor, true));

        if (!owner.TryGetPlayerPearlModule(out var module))
        {
            return;
        }

        if (module.CooldownTimer != 0)
        {
            return;
        }

        if (effect.MajorEffect == MajorEffectType.REVIVE)
        {
            module.CooldownTimer = 40;
        }
        else if (effect.MajorEffect == MajorEffectType.CAMOFLAGUE)
        {
            module.CooldownTimer = 40;
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        foreach (var shortcut in LockedShortcutsSprites.Values)
        {
            shortcut.RemoveFromContainer();
        }

        if (room == null || !OwnerRef.TryGetTarget(out var owner) || owner.Room != room.abstractRoom)
        {
            Destroy();
            return;
        }

        if (owner.realizedObject is not DataPearl pearl)
        {
            return;
        }

        if (!pearl.abstractPhysicalObject.TryGetPlayerPearlModule(out var module))
        {
            return;
        }


        var pearlType = pearl.AbstractPearl.dataPearlType;
        var targetPos = InitialPos + new Vector2(0.0f, -40.0f);

        // Pearls with floating animation
        if (pearl.IsHalcyonPearl() || pearl.IsHeartPearl() || pearlType == Enums.Pearls.SS_Pearlcat || pearlType == Enums.Pearls.CW_Pearlcat)
        {
            targetPos.y += Mathf.Sin(AnimCounter / 60.0f) * 20.0f;
        }

        pearl.firstChunk.pos = Vector2.Lerp(pearl.firstChunk.pos, targetPos, 0.1f);
        pearl.firstChunk.vel = Vector2.zero;
        pearl.gravity = 0.0f;

        var effect = pearl.abstractPhysicalObject.GetPearlEffect();

        UpdateShieldSentry(owner, module, pearl, effect);
        UpdateRageSentry(owner, module, pearl, effect);
        UpdateAgilitySentry(owner, pearl, effect);
        UpdateReviveSentry(owner, module, pearl, effect);
        UpdateCamoSentry(module, pearl, effect);

        UpdateSpearSentry(owner, module, pearl, effect);
        
        if (pearl.IsHalcyonPearl())
        {
            UpdateMusicSentry(pearl, "NA_19 - Halcyon Memories");
        }
        else if (pearl.IsHeartPearl())
        {
            UpdateMusicSentry(pearl, "Pearlcat_Heartmend");

            UpdateHeartSentry(owner, pearl);
        }
        else if (pearlType == Enums.Pearls.SS_Pearlcat)
        {
            UpdateMusicSentry(pearl, "Pearlcat_Amnesia");
        }
        else if (pearlType == Enums.Pearls.CW_Pearlcat)
        {
            UpdateMusicSentry(pearl, "Pearlcat_Chatoyance");
        }

        AnimCounter++;
    }



    private void UpdateHeartSentry(AbstractPhysicalObject owner, DataPearl pearl)
    {
        if (!owner.TryGetPlayerPearlOwner(out var player))
        {
            return;
        }

        player.mainBodyChunk.vel += Custom.DirVec(player.firstChunk.pos, pearl.firstChunk.pos) * Custom.LerpMap(Custom.Dist(player.firstChunk.pos, pearl.firstChunk.pos), 75.0f, 125.0f, 0.0f, 3.0f, 0.8f);
    }

    // This can actually cause a memory leak if there are 2 or more music player pearls deployed at the same time - stopped it with a check but still dunno what causes the leak
    private void UpdateMusicSentry(DataPearl self, string songName)
    {
        PlaysMusic = true;

        if (room is null)
        {
            return;
        }

        var musicPlayer = room?.game?.manager?.musicPlayer;
        var otherMusicPearlsExist = false;

        foreach (var objLayer in self.room.physicalObjects)
        {
            foreach (var obj in objLayer)
            {
                if (obj == self)
                {
                    continue;
                }

                if (obj is not DataPearl dataPearl)
                {
                    continue;
                }

                if (!dataPearl.AbstractPearl.TryGetSentry(out var sentryModule))
                {
                    continue;
                }

                if (sentryModule.PlaysMusic)
                {
                    otherMusicPearlsExist = true;

                    if (musicPlayer?.song is not null && musicPlayer.song.name == songName)
                    {
                        musicPlayer.song.StopAndDestroy();
                        musicPlayer.song = null;
                    }

                    if (AnimCounter % 30 == 0 && Custom.Dist(self.firstChunk.pos, obj.firstChunk.pos) < 1500.0f)
                    {
                        self.room.ConnectEffect(self.firstChunk.pos, obj.firstChunk.pos, self.abstractPhysicalObject.GetObjectColor(), lifeTime: 60.0f);
                    }
                }
            }
        }

        if (otherMusicPearlsExist)
        {
            return;
        }

        if (musicPlayer is null)
        {
            return;
        }

        if (musicPlayer.song != null)
        {
            if (musicPlayer.song.name == songName)
            {
                var song = musicPlayer.song;
                
                var targetVolume = song.name switch
                {
                    _ => 0.3f,
                };

                MusicVolume = Mathf.Lerp(MusicVolume, targetVolume, 0.025f);

                if (room?.game?.FirstAlivePlayer?.realizedCreature is Player player)
                {
                    song.volume = Custom.LerpMap(Custom.Dist(player.firstChunk.pos, self.firstChunk.pos), 50.0f, 1000.0f, MusicVolume, 0.0f);
                }
                else
                {
                    song.volume = 0.0f;
                }

                var audioData = new float[1024];
                song.subTracks[0].source.GetSpectrumData(audioData, 0, FFTWindow.Hamming);
                
                var amplitude = 0.0f;

                for (var i = 0; i < 1024; i++)
                {
                    amplitude += audioData[i];
                }

                HaloScale = Custom.LerpMap(amplitude, 0.0f, musicPlayer.song.name == "Pearlcat_Amnesia" ? 0.45f : 0.15f, musicPlayer.song.name == "Pearlcat_Amnesia" ? 0.8f : 0.6f, 1.3f);
            }
            else
            {
                musicPlayer.song.StopAndDestroy();
                musicPlayer.song = null;
            }
        }

        if (musicPlayer.song == null && room?.game?.manager is not null)
        {
            musicPlayer.song = new Song(room.game.manager.musicPlayer, songName, MusicPlayer.MusicContext.StoryMode)
            {
                stopAtGate = true,
                stopAtDeath = true,
                fadeInTime = 1.0f,
                playWhenReady = true,
            };

            MusicVolume = 0.0f;
            WasPlayingMusic = true;
        }
    }



    private void UpdateCamoSentry(PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        const float MAX_SCALE = 300.0f;

        if (HoloLightActive)
        {
            HoloLightAlpha = Mathf.Lerp(HoloLightAlpha, 0.0f, Custom.LerpMap(HoloLightAlpha, 1.0f, 0.4f, 0.001f, 0.01f));

            if (HoloLightAlpha <= 0.02f)
            {
                HoloLightAlpha = 0.0f;
                HoloLightActive = false;
            }
         
            HoloLightScale = Mathf.Lerp(HoloLightScale, MAX_SCALE, 0.04f);
        }
        
        if (effect.MajorEffect != MajorEffectType.CAMOFLAGUE)
        {
            return;
        }

        if (module.CooldownTimer != 0)
        {
            return;
        }

        module.CooldownTimer = 200;

        HoloLightScale = 0.0f;
        HoloLightAlpha = 1.0f;
        HoloLightActive = true;

        var color = Custom.hexToColor("fcb103");

        room.AddObject(new LightningMachine.Impact(pearl.firstChunk.pos, 0.4f, color, true));
        room.AddObject(new ExplosionSpikes(room, pearl.firstChunk.pos, 5, 30.0f, 10, 10.0f, 20.0f, color));

        room.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_A, pearl.firstChunk, false, 2.0f, 1.0f);
    }

    
    private void UpdateReviveSentry(AbstractPhysicalObject owner, PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.REVIVE)
        {
            return;
        }

        if (!owner.TryGetPearlGraphicsModule(out var addon))
        {
            return;
        }

        if (module.CooldownTimer != 0)
        {
            return;
        }

        var didHeal = false;

        for (var i = room.updateList.Count - 1; i >= 0; i--)
        {
            var updatable = room.updateList[i];
            if (updatable is not Creature crit)
            {
                continue;
            }

            if (!Custom.DistLess(pearl.firstChunk.pos, crit.firstChunk.pos, 225.0f))
            {
                continue;
            }

            if (crit.State is not HealthState healthState)
            {
                continue;
            }

            if (healthState.dead)
            {
                continue;
            }

            if (healthState.ClampedHealth >= 1.0f)
            {
                continue;
            }

            healthState.health = Mathf.Min(1.0f, healthState.health + 0.25f);

            room.AddObject(new LightningMachine.Impact(crit.mainBodyChunk.pos, 0.6f, addon.SymbolColor, true));
            room.AddObject(new ExplosionSpikes(room, crit.mainBodyChunk.pos, 5, 70.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
            room.AddObject(new ShockWave(pearl.firstChunk.pos, 30.0f, 0.2f, 10));

            room.ConnectEffect(crit.mainBodyChunk.pos, pearl.firstChunk.pos, addon.SymbolColor, 2.0f, 60);
            didHeal = true;
        }

        if (!didHeal)
        {
            return;
        }

        module.CooldownTimer = 160;

        room.AddObject(new LightningMachine.Impact(pearl.firstChunk.pos, 0.1f, addon.SymbolColor, true));
        room.AddObject(new ShockWave(pearl.firstChunk.pos, 30.0f, 0.2f, 10));
        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos);
    }


    private void UpdateAgilitySentry(AbstractPhysicalObject owner, DataPearl pearl, PearlEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.AGILITY || pearl.AbstractPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
        {
            return;
        }

        var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (playerModule is null)
        {
            return;
        }

        if (!playerModule.PlayerRef.TryGetTarget(out var player))
        {
            return;
        }

        var inGate = player.room?.IsGateRoom() ?? false;
        var tooClose = Custom.DistLess(player.firstChunk.pos, pearl.firstChunk.pos, 75.0f);
        var possessingCreature = playerModule.PossessedCreature != null;

        var canTP = !tooClose && !inGate && !possessingCreature;

        AgilityPos = canTP ? pearl.firstChunk.pos : null;
        AgilityRoom = canTP ? pearl.AbstractPearl.Room : null;
    }


    private void UpdateSpearSentry(AbstractPhysicalObject owner, PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.SPEAR_CREATION)
        {
            return;
        }

        var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (playerModule is null)
        {
            return;
        }

        if (!playerModule.PlayerRef.TryGetTarget(out var player))
        {
            return;
        }


        var armCooldown = 80;
        var tooClose = Custom.DistLess(player.firstChunk.pos, pearl.firstChunk.pos, 75.0f);

        if (tooClose)
        {
            module.CooldownTimer = armCooldown;
        }


        var shouldArm = module.CooldownTimer == 0;

        if (!SpearBombArmed && shouldArm)
        {
            pearl.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pearl.firstChunk.pos, 1.0f, 3.0f);
        }

        SpearBombArmed = shouldArm;
        SpearBombRoom = SpearBombArmed ? pearl.room.abstractRoom : null;
    }


    public void UpdateShieldSentry(AbstractPhysicalObject owner, PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.SHIELD)
        {
            return;
        }

        if (!owner.TryGetPearlGraphicsModule(out var addon))
        {
            return;
        }

        var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (playerModule is null)
        {
            return;
        }

        if (!playerModule.PlayerRef.TryGetTarget(out var player))
        {
            return;
        }


        if (module.CooldownTimer == 1)
        {
            room.AddObject(new ExplosionSpikes(room, InitialPos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
            room.AddObject(new LightningMachine.Impact(InitialPos, 0.1f, addon.SymbolColor, true));
        }

        if (module.CooldownTimer == 0 || ShieldTimer > 0)
        {
            var roomObjects = room.physicalObjects;

            for (var i = roomObjects.Length - 1; i >= 0; i--)
            {
                var roomObject = roomObjects[i];

                for (var j = roomObject.Count - 1; j >= 0; j--)
                {
                    var physicalObject = roomObject[j];

                    if (Custom.Dist(physicalObject.firstChunk.pos, pearl.firstChunk.pos) > 75.0f)
                    {
                        continue;
                    }

                    if (physicalObject is Weapon weapon)
                    {
                        if (Custom.DistLess(weapon.thrownPos, pearl.firstChunk.pos, 75.0f))
                        {
                            continue;
                        }

                        if (weapon.mode == Weapon.Mode.Thrown)
                        {
                            weapon.ChangeMode(Weapon.Mode.Free);
                            weapon.SetRandomSpin();
                            weapon.firstChunk.vel *= -0.2f;

                            weapon.room.DeflectEffect(weapon.firstChunk.pos);
                    
                            if (ShieldTimer <= 0)
                            {
                                ShieldTimer = ModOptions.ShieldDuration.Value * 3.0f;
                                room.PlaySound(Enums.Sounds.Pearlcat_ShieldStart, pearl.firstChunk);
                            }
                        }
                    }
                    else if (physicalObject is Creature crit)
                    {
                        if (crit.abstractCreature.controlled)
                        {
                            continue;
                        }

                        if (!player.IsHostileToMe(crit) && crit is not Lizard or Scavenger)
                        {
                            continue;
                        }

                        if (crit.dead)
                        {
                            continue;
                        }

                        crit.mainBodyChunk.vel = Custom.DirVec(pearl.firstChunk.pos, crit.firstChunk.pos) * 10.0f;

                        if (ShieldTimer <= 0)
                        {
                            pearl.room.DeflectEffect(crit.mainBodyChunk.pos);
                            ShieldTimer = ModOptions.ShieldDuration.Value * 3.0f;
                            room.PlaySound(Enums.Sounds.Pearlcat_ShieldStart, pearl.firstChunk);
                        }
                    }
                }
            }
        }

        if (ShieldTimer > 0)
        {
            module.CooldownTimer = ModOptions.ShieldRechargeTime.Value;
            ShieldTimer--;

            if (ShieldTimer == 0)
            {
                room.PlaySound(Enums.Sounds.Pearlcat_ShieldOff, pearl.firstChunk);
            }
        }

        foreach (var shortcut in room.shortcuts)
        {
            if (shortcut.shortCutType != ShortcutData.Type.RoomExit && shortcut.shortCutType != ShortcutData.Type.Normal)
            {
                continue;
            }

            if (!Custom.DistLess(room.MiddleOfTile(shortcut.StartTile), pearl.firstChunk.pos, 75.0f))
            {
                continue;
            }

            if (!LockedShortcutsSprites.TryGetValue(shortcut, out var sprite))
            {
                sprite = new FSprite("pearlcat_hudlockedshortcut")
                {
                    shader = Utils.Shaders["Hologram"],
                    scale = 0.5f,
                };

                LockedShortcutsSprites.Add(shortcut, sprite);
            }
        }


        ShieldHoldLoop ??= new ChunkDynamicSoundLoop(owner.realizedObject.firstChunk)
        {
            sound = Enums.Sounds.Pearlcat_ShieldHold,
            destroyClipWhenDone = false,
            Pitch = 1.0f,
            Volume = 1.0f,
        };

        // i don't fricking trust this
        try
        {
            if (ShieldHoldLoop?.owner?.room != null && (ShieldHoldLoop?.emitter == null || ShieldHoldLoop?.emitter?.room != null))
            {
                if (ShieldHoldLoop != null)
                {
                    ShieldHoldLoop.Update();
                    ShieldHoldLoop.Volume = Mathf.Lerp(ShieldHoldLoop.Volume, ShieldTimer > 0 ? 1.0f : 0.0f, 0.1f);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning("HANDLED: Shield Sentry Sound Exception:\n" + e + "\n" + e.StackTrace);
        }
    }


    public void UpdateRageSentry(AbstractPhysicalObject owner, PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        if (ModOptions.OldRedPearlAbility.Value)
        {
            UpdateOldRageSentry(owner, module, pearl, effect);
            return;
        }

        if (effect.MajorEffect != MajorEffectType.RAGE)
        {
            return;
        }

        if (!owner.TryGetPlayerPearlOwner(out var player))
        {
            return;
        }

        PlayerAbilities_Helpers.RageTargetLogic(pearl, player, true);
    }

    private void UpdateOldRageSentry(AbstractPhysicalObject owner, PlayerPearlModule module, DataPearl pearl, PearlEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.RAGE)
        {
            return;
        }

        if (!owner.TryGetPearlGraphicsModule(out var addon))
        {
            return;
        }

        var shootTime = ModOptions.LaserWindupTime.Value;
        var cooldownTime = ModOptions.LaserRechargeTime.Value;
        var shootDamage = ModOptions.LaserDamage.Value;

        var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (playerModule is null)
        {
            return;
        }

        if (!playerModule.PlayerRef.TryGetTarget(out var player))
        {
            return;
        }

        var playerInRange = Custom.DistLess(pearl.firstChunk.pos, player.firstChunk.pos, 90.0f);

        if (playerInRange)
        {
            if (RageCounter <= 0)
            {
                room.ConnectEffect(player.firstChunk.pos, pearl.firstChunk.pos, addon.SymbolColor, lifeTime: 24);
                room.AddObject(new LightningMachine.Impact(pearl.firstChunk.pos, 0.1f, addon.SymbolColor, true));

                room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 3.0f);
            }

            RageCounter = 3;
        }

        module.LaserLerp = 0.0f;

        if (RageTarget == null || !RageTarget.TryGetTarget(out _))
        {
            module.LaserTimer = shootTime;
        }


        if (RageTarget == null || !RageTarget.TryGetTarget(out var target))
        {
            Creature? bestTarget = null;
            var shortestDist = float.MaxValue;

            foreach (var roomObject in pearl.room.physicalObjects)
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


                    if (!player.IsHostileToMe(creature) && !(pearl.room.roomSettings.name == "T1_CAR2" && creature is Fly))
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


                    var dist = Custom.Dist(creature.mainBodyChunk.pos, pearl.firstChunk.pos);

                    if (dist > 400.0f)
                    {
                        continue;
                    }

                    if (dist > shortestDist)
                    {
                        continue;
                    }


                    if (!pearl.room.VisualContact(pearl.firstChunk.pos, creature.mainBodyChunk.pos))
                    {
                        continue;
                    }

                    shortestDist = dist;
                    bestTarget = creature;
                }
            }

            if (bestTarget != null)
            {
                RageTarget = new(bestTarget);
            }
        }
        else
        {
            // ensure target is still valid
            var invalidTarget = false;

            if (!Custom.DistLess(target.mainBodyChunk.pos, pearl.firstChunk.pos, 500.0f))
            {
                invalidTarget = true;
            }

            if (target.room != pearl.room)
            {
                invalidTarget = true;
            }

            if (target.dead)
            {
                invalidTarget = true;
            }

            if (!pearl.room.VisualContact(pearl.firstChunk.pos, target.mainBodyChunk.pos))
            {
                invalidTarget = true;
            }


            if (invalidTarget)
            {
                RageTarget = null;
            }
        }


        if (RageTarget == null || !RageTarget.TryGetTarget(out target))
        {
            return;
        }

        if (RageCounter <= 0)
        {
            module.CooldownTimer = cooldownTime;
        }

        if (module.CooldownTimer > 0)
        {
            module.LaserTimer = shootTime;
            return;
        }

        if (module.LaserTimer <= 0)
        {
            module.CooldownTimer = cooldownTime;

            var targetPos = target.mainBodyChunk.pos;

            // shoot laser
            pearl.room.PlaySound(SoundID.Bomb_Explode, targetPos, 0.8f, Random.Range(0.7f, 1.3f));
            pearl.room.AddObject(new LightningMachine.Impact(targetPos, 0.5f, addon.SymbolColor, true));

            pearl.room.AddObject(new ShockWave(targetPos, 30.0f, 0.4f, 5));
            room.AddObject(new ExplosionSpikes(pearl.room, targetPos, 5, 20.0f, 10, 20.0f, 20.0f, addon.SymbolColor));

            target.SetKillTag(player.abstractCreature);
            target.Violence(player.mainBodyChunk, null, target.mainBodyChunk, null, Creature.DamageType.Explosion, shootDamage, 5.0f);

            if (!playerInRange)
            {
                RageCounter--;
            }
            else
            {
                room.ConnectEffect(player.firstChunk.pos, pearl.firstChunk.pos, addon.SymbolColor, lifeTime: 24);
            }
        }
        else
        {
            module.LaserTimer--;
        }

        module.LaserLerp = Custom.LerpMap(module.LaserTimer, shootTime, 0, 0.0f, 1.0f);
    }



    public override void Destroy()
    {
        base.Destroy();

        foreach (var shortcut in LockedShortcutsSprites.Values)
        {
            shortcut.RemoveFromContainer();
        }

        if (OwnerRef.TryGetTarget(out var owner) && owner.TryGetPlayerPearlModule(out var module))
        {
            if (ModuleManager.SentryData.TryGetValue(owner, out _))
            {
                ModuleManager.SentryData.Remove(owner);
            }

            if (module.IsSentry)
            {
                module.RemoveSentry(owner);
            }

            var playerModule = owner.Room.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(owner));

            if (owner.TryGetPearlGraphicsModule(out var addon) && playerModule != null && playerModule.PlayerRef.TryGetTarget(out var player))
            {
                if (room != null && owner.realizedObject != null)
                {
                    room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 0.5f);

                    room.AddObject(new ExplosionSpikes(room, owner.realizedObject.firstChunk.pos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
                    room.AddObject(new LightningMachine.Impact(owner.realizedObject.firstChunk.pos, 0.1f, addon.SymbolColor, true));
                }

                // Agility Teleport
                if (module.CooldownTimer == 0 && AgilityPos is Vector2 agilityPos && AgilityRoom == player.abstractCreature.Room)
                {
                    if (room != null && owner.realizedObject != null)
                    {
                        player.ConnectEffect(agilityPos, addon.SymbolColor);
                        room.AddObject(new ShockWave(agilityPos, 100.0f, 0.3f, 20));
                        room.AddObject(new ExplosionSpikes(room, agilityPos, 5, 70.0f, 25, 10.0f, 40.0f, addon.SymbolColor));


                        room.PlaySound(Enums.Sounds.Pearlcat_CamoFade, owner.realizedObject.firstChunk, false, 1.0f, 1.5f);

                        room.PlaySound(SoundID.Fire_Spear_Explode, owner.realizedObject.firstChunk, false, 0.5f, 1.0f);
                        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 1.0f, 0.3f);
                    }

                    player.SuperHardSetPosition(agilityPos);
                    
                    if (player.slugOnBack?.slugcat is Player slugOnBack)
                    {
                        slugOnBack.SuperHardSetPosition(agilityPos);
                        slugOnBack.graphicsModule?.Reset();
                        slugOnBack.bodyChunks.ToList().ForEach(x => x.vel = Vector2.zero);
                    }

                    player.graphicsModule?.Reset();
                    player.bodyChunks.ToList().ForEach(x => x.vel = Vector2.zero);

                    playerModule.FlyTimer = 10;
                    module.CooldownTimer = 1200;
                }

                // Spear Bomb
                if (SpearBombArmed && SpearBombRoom == player.abstractCreature.Room)
                {
                    if (room != null && owner.realizedObject != null)
                    {
                        var pos = owner.realizedObject.firstChunk.pos;
                        var color = addon.SymbolColor;

                        room.AddObject(new SootMark(room, pos, 40f, true));

                        room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
                        room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));

                        room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 120f, color));
                        room.AddObject(new ShockWave(pos, 160f, 0.3f, 10));

                        room.AddObject(new Explosion(room,
                            owner.realizedObject,
                            pos,
                            7,
                            125.0f,
                            10.0f,
                           2.0f,
                            280.0f,
                            0.25f,
                            player,
                            0.7f,
                            160.0f,
                            1.0f));

                        room.PlaySound(SoundID.Bomb_Explode, pos);
                        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 1.2f, 0.75f);

                        for (var i = 0; i < 25; i++)
                        {
                            var randVec = Custom.RNV();

                            if (room.GetTile(pos + randVec * 20f).Solid)
                            {
                                if (!room.GetTile(pos - randVec * 20f).Solid)
                                {
                                    randVec *= -1f;
                                }
                                else
                                {
                                    randVec = Custom.RNV();
                                }
                            }

                            for (var j = 0; j < 3; j++)
                            {
                                room.AddObject(new Spark(pos + randVec * Mathf.Lerp(30f, 60f, Random.value),
                                    randVec * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value,
                                    Color.Lerp(color, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                            }

                            room.AddObject(new Explosion.FlashingSmoke(pos + randVec * 40f * Random.value,
                                randVec * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)),
                                1f + 0.05f * Random.value, Color.white,
                                color, Random.Range(3, 11)));
                        }

                        for (var i = 0; i < 6; i++)
                        {
                            room.AddObject(new ScavengerBomb.BombFragment(pos, Custom.DegToVec((i + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
                        }

                        room.ScreenMovement(pos, default, 1.3f);


                        player.RemoveFromInventory(owner);

                        owner.destroyOnAbstraction = true;
                        owner.Abstractize(owner.pos);
                    }
                }
            }
        }

        if (WasPlayingMusic && room != null)
        {
            var musicPlayer = room.game.manager.musicPlayer;

            var songsToStop = new List<string>()
            {
                "NA_19 - Halcyon Memories",
                "Pearlcat_Amnesia",
                "Pearlcat_Heartmend",
                "Pearlcat_Chatoyance",
            };

            if (songsToStop.Contains(musicPlayer.song.name))
            {
                musicPlayer.song?.StopAndDestroy();
                musicPlayer.song = null;
            }
        }
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[7];

        sLeaser.sprites[0] = new("pixel");

        sLeaser.sprites[1] = new("pearlcat_sentryhalo")
        {
            //shader = Utils.Shaders["GateHologram"],
        };

        sLeaser.sprites[2] = new("pearlcat_shieldsentry")
        {
            shader = Utils.Shaders["Hologram"],
        };

        sLeaser.sprites[3] = new("Futile_White")
        {
            shader = Utils.Shaders["GravityDisruptor"],
            scale = 0.0f,
        };

        sLeaser.sprites[4] = new("pixel")
        {
            shader = Utils.Shaders["HologramBehindTerrain"],
        };

        sLeaser.sprites[5] = new("pixel")
        {
            shader = Utils.Shaders["GateHologram"],
        };

        sLeaser.sprites[6] = new("Futile_White")
        {
            shader = Utils.Shaders["HoloGrid"],
            scale = 0.0f,
        };


        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || !OwnerRef.TryGetTarget(out var owner))
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        if (owner.realizedObject is not DataPearl pearl)
        {
            return;
        }

        if (!pearl.abstractPhysicalObject.TryGetPearlGraphicsModule(out var addon))
        {
            return;
        }

        var effect = pearl.abstractPhysicalObject.GetPearlEffect();
        
        var targetPos = Vector2.Lerp(pearl.firstChunk.lastPos, pearl.firstChunk.pos, timeStacker) - camPos;

        var symbolSprite = sLeaser.sprites[0];
        var haloSprite = sLeaser.sprites[1];
        var guideSprite = sLeaser.sprites[2];
        var shieldSprite = sLeaser.sprites[3];
        var laserSprite = sLeaser.sprites[4];
        var counterSprite = sLeaser.sprites[5];
        var holoLightSprite = sLeaser.sprites[6];

        symbolSprite.element = Futile.atlasManager.GetElementWithName(addon.DrawSymbolCooldown && RageCounter > 0 ? "pearlcat_glyphcooldown" : PearlGraphics.SpriteFromPearl(pearl.abstractPhysicalObject));
        symbolSprite.SetPosition(targetPos + new Vector2(15.0f, 15.0f));
        symbolSprite.color = addon.SymbolColor;


        haloSprite.SetPosition(targetPos);
        haloSprite.color = addon.SymbolColor;
        haloSprite.scale = HaloScale;

        if (effect.MajorEffect == MajorEffectType.NONE || pearl.AbstractPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
        {
            haloSprite.color = addon.ActiveHaloColor;
            haloSprite.element = Futile.atlasManager.GetElementWithName("LizardBubble6");
        }
        else
        {
            haloSprite.rotation = AnimCounter;
        }

        guideSprite.SetPosition(targetPos);
        guideSprite.color = effect.MajorEffect == MajorEffectType.AGILITY && AgilityPos == null ? Color.red : addon.SymbolColor;
        guideSprite.isVisible =
            effect.MajorEffect == MajorEffectType.SHIELD
            || effect.MajorEffect == MajorEffectType.AGILITY
            || effect.MajorEffect == MajorEffectType.REVIVE
            || effect.MajorEffect == MajorEffectType.RAGE
            || effect.MajorEffect == MajorEffectType.SPEAR_CREATION;

        guideSprite.element = Futile.atlasManager.GetElementWithName(effect.MajorEffect switch
        {
            MajorEffectType.AGILITY => "pearlcat_agilitysentry",
            MajorEffectType.REVIVE => "pearlcat_revivesentry",
            MajorEffectType.RAGE => ModOptions.OldRedPearlAbility.Value ? "pearlcat_agilitysentry" : "pearlcat_ragesentry",
            MajorEffectType.SPEAR_CREATION => "pearlcat_agilitysentry",

            _ => "pearlcat_shieldsentry",
        });

        if (pearl.AbstractPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
        {
            guideSprite.isVisible = false;
        }


        shieldSprite.SetPosition(targetPos);
        shieldSprite.scale = Mathf.Lerp(shieldSprite.scale, ShieldTimer > 5 ? 8.0f : 0.0f, 0.1f);


        laserSprite.isVisible = RageTarget != null && RageCounter > 0;
        var laserLerp = addon.LaserLerp;

        laserSprite.alpha = Custom.LerpMap(laserLerp, 0.0f, 1.0f, 0.75f, 1.0f);
        laserSprite.color = laserLerp > 0.97f || laserLerp == 0.0 ? Color.white : addon.SymbolColor;

        Creature? target = null;
        RageTarget?.TryGetTarget(out target);

        var startPos = targetPos;
        var endPos = (target?.mainBodyChunk?.pos ?? Vector2.zero) - camPos;

        var dir = Custom.DirVec(startPos, endPos);

        var laserWidth = laserLerp > 0.97 ? 10.0f : Custom.LerpMap(laserLerp, 0.0f, 1.0f, 1.5f, 5.0f);
        var laserLength = Custom.Dist(startPos, endPos);

        laserSprite.rotation = Custom.VecToDeg(dir);
        laserSprite.scaleX = laserWidth;
        laserSprite.scaleY = laserLength;

        laserSprite.SetPosition(startPos + dir * laserLength / 2.0f);

        counterSprite.SetPosition(targetPos + new Vector2(-15.0f, 15.0f));
        counterSprite.color = addon.SymbolColor;
        counterSprite.isVisible = false;

        if (effect.MajorEffect == MajorEffectType.RAGE && ModOptions.OldRedPearlAbility.Value)
        {
            counterSprite.isVisible = true;
            counterSprite.element = Futile.atlasManager.GetElementWithName(PearlGraphics.SpriteFromNumber(RageCounter) ?? "pearlcat_glyphcooldown");
        }


        foreach (var shortcut in room.shortcuts)
        {
            if (!LockedShortcutsSprites.TryGetValue(shortcut, out var sprite))
            {
                continue;
            }

            if (sprite.container == null)
            {
                rCam.ReturnFContainer("HUD").AddChild(sprite);
            }

            sprite.color = addon.SymbolColor;
            sprite.SetPosition(room.MiddleOfTile(shortcut.startCoord) - camPos);

            sprite.isVisible = !addon.DrawSymbolCooldown || ShieldTimer > 0;
        }

        holoLightSprite.SetPosition(targetPos);
        holoLightSprite.scale = HoloLightScale;
        holoLightSprite.alpha = Custom.LerpMap(HoloLightAlpha, 0.3f, 0.0f, 1.0f, 0.0f);
        holoLightSprite.color = new(0.384f, 0.184f, 0.984f, HoloLightAlpha);
    }


    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        foreach (var sprite in sLeaser.sprites)
        {
            newContatiner.AddChild(sprite);
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }
}
