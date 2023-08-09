using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Pearlcat.POEffect;
using MoreSlugcats;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace Pearlcat;

public class POSentry : UpdatableAndDeletable, IDrawable
{
    public static ConditionalWeakTable<AbstractPhysicalObject, POSentry> SentryData { get; } = new();

    public WeakReference<AbstractPhysicalObject> OwnerRef { get; }
    public Vector2 InitialPos { get; }
    public LightSource? LightSource { get; set; }
    public DynamicSoundLoop ShieldHoldLoop { get; } = null!;

    public POSentry(AbstractPhysicalObject owner)
    {
        OwnerRef = new(owner);

        if (SentryData.TryGetValue(owner, out _)) return;

        if (owner.realizedObject == null) return;

        SentryData.Add(owner, this);

        room = owner.realizedObject.room;
        InitialPos = owner.realizedObject.firstChunk.pos;

        var effect = owner.GetPOEffect();

        if (effect.MajorEffect == MajorEffectType.RAGE)
            AbilityCounter = 3;

        if (!owner.TryGetAddon(out var addon)) return;

        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 2.0f);

        //room.AddObject(new ShockWave(InitialPos, 30.0f, 0.5f, 10));
        room.AddObject(new ExplosionSpikes(room, InitialPos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
        room.AddObject(new LightningMachine.Impact(InitialPos, 0.1f, addon.SymbolColor, true));

        ShieldHoldLoop = new ChunkDynamicSoundLoop(owner.realizedObject.firstChunk)
        {
            sound = Enums.Sounds.Pearlcat_ShieldHold,
            destroyClipWhenDone = false,
            Pitch = 1.0f,
            Volume = 1.0f,
        };
    }

    public Dictionary<ShortcutData, FSprite> LockedShortcutsSprites = new();

    public float ShieldTimer { get; set; } = -1;
    public int? AbilityCounter { get; set; }
    public float AnimCounter { get; set; }
    public WeakReference<Creature>? RageTarget { get; set; }

    public override void Update(bool eu)
    {
        base.Update(eu);

        foreach (var shortcut in LockedShortcutsSprites.Values)
            shortcut.RemoveFromContainer();

        if (room == null || !OwnerRef.TryGetTarget(out var owner) || owner.Room != room.abstractRoom)
        {
            Destroy();
            return;
        }

        if (owner.realizedObject is not DataPearl pearl) return;

        if (!pearl.abstractPhysicalObject.TryGetModule(out var module)) return;


        var targetPos = InitialPos + new Vector2(0.0f, -40.0f);

        pearl.firstChunk.pos = Vector2.Lerp(pearl.firstChunk.pos, targetPos, 0.1f);
        pearl.firstChunk.vel = Vector2.zero;
        pearl.gravity = 0.0f;

        var effect = pearl.abstractPhysicalObject.GetPOEffect();

        UpdateShieldSentry(owner, module, pearl, effect);
        UpdateRageSentry(owner, module, pearl, effect);
        UpdateCamoSentry(owner, module, pearl, effect);

        AnimCounter++;
    }

    private void UpdateCamoSentry(AbstractPhysicalObject owner, PlayerObjectModule module, DataPearl pearl, POEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.CAMOFLAGUE) return;

        if (LightSource == null)
        {
            LightSource = new(pearl.firstChunk.pos, false, Color.white, this)
            {
                requireUpKeep = true,
                setRad = 0.0f,
                setAlpha = 1.0f,
                color = Color.white,
                flat = false,
            };
            room.AddObject(LightSource);
        }

        LightSource.stayAlive = true;

        LightSource.setRad = Mathf.Lerp(LightSource.Rad, 500.0f, 0.1f);
        LightSource.setPos = pearl.firstChunk.pos;
    }

    public void UpdateShieldSentry(AbstractPhysicalObject owner, PlayerObjectModule module, DataPearl pearl, POEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.SHIELD) return;

        if (!owner.TryGetAddon(out var addon)) return;

        var playerModule = owner.Room.world.game.GetAllPlayerData().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (!playerModule.PlayerRef.TryGetTarget(out var player)) return;


        if (module.CooldownTimer == 1)
        {
            room.AddObject(new ExplosionSpikes(room, InitialPos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
            room.AddObject(new LightningMachine.Impact(InitialPos, 0.1f, addon.SymbolColor, true));
        }

        if (module.CooldownTimer == 0 || ShieldTimer > 0)
        {
            var roomObjects = room.physicalObjects;

            for (int i = roomObjects.Length - 1; i >= 0; i--)
            {
                var roomObject = roomObjects[i];

                for (int j = roomObject.Count - 1; j >= 0; j--)
                {
                    var physicalObject = roomObject[j];

                    if (Custom.Dist(physicalObject.firstChunk.pos, pearl.firstChunk.pos) > 75.0f) continue;

                    if (physicalObject is Weapon weapon)
                    {
                        if (Custom.DistLess(weapon.thrownPos, pearl.firstChunk.pos, 75.0f)) continue;

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
                        if (crit.dead) continue;

                        if (crit is Player) continue;

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
                room.PlaySound(Enums.Sounds.Pearlcat_ShieldOff, pearl.firstChunk);
        }

        foreach (var shortcut in room.shortcuts)
        {
            if (shortcut.shortCutType != ShortcutData.Type.RoomExit && shortcut.shortCutType != ShortcutData.Type.Normal) continue;

            if (!Custom.DistLess(room.MiddleOfTile(shortcut.StartTile), pearl.firstChunk.pos, 75.0f)) continue;

            if (!LockedShortcutsSprites.TryGetValue(shortcut, out var sprite))
            {
                sprite = new FSprite("pearlcat_hudlockedshortcut")
                {
                    shader = room.game.rainWorld.Shaders["Hologram"],
                    scale = 0.5f,
                };

                LockedShortcutsSprites.Add(shortcut, sprite);
            }
        }

        if (ShieldHoldLoop != null)
        {
            ShieldHoldLoop.Update();
            ShieldHoldLoop.Volume = Mathf.Lerp(ShieldHoldLoop.Volume, ShieldTimer > 0 ? 1.0f : 0.0f, 0.1f);
        }
    }

    public void UpdateRageSentry(AbstractPhysicalObject owner, PlayerObjectModule module, DataPearl pearl, POEffect effect)
    {
        if (effect.MajorEffect != MajorEffectType.RAGE) return;

        if (!owner.TryGetAddon(out var addon)) return;

        var shootTime = ModOptions.LaserWindupTime.Value;
        var cooldownTime = ModOptions.LaserRechargeTime.Value;
        var shootDamage = ModOptions.LaserDamage.Value;

        var playerModule = owner.Room.world.game.GetAllPlayerData().FirstOrDefault(x => x.Inventory.Contains(owner));

        if (!playerModule.PlayerRef.TryGetTarget(out var player)) return;

        var playerInRange = Custom.DistLess(pearl.firstChunk.pos, player.firstChunk.pos, 90.0f);

        if (playerInRange)
        {
            if (AbilityCounter <= 0)
            {
                room.ConnectEffect(player.firstChunk.pos, pearl.firstChunk.pos, addon.SymbolColor, lifeTime: 24);
                room.AddObject(new LightningMachine.Impact(pearl.firstChunk.pos, 0.1f, addon.SymbolColor, true));

                room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 3.0f);
            }

            AbilityCounter = 3;
        }

        module.LaserLerp = 0.0f;

        if (RageTarget == null || !RageTarget.TryGetTarget(out _))
            module.LaserTimer = shootTime;


        if (RageTarget == null || !RageTarget.TryGetTarget(out var target))
        {
            Creature? bestTarget = null;
            var shortestDist = float.MaxValue;

            foreach (var roomObject in pearl.room.physicalObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not Creature creature) continue;

                    if (creature is Cicada) continue;

                    if (creature is Centipede centipede && centipede.Small) continue;


                    if (!player.IsHostileToMe(creature) && !(pearl.room.roomSettings.name == "T1_CAR2" && creature is Fly)) continue;

                    if (creature.dead) continue;

                    if (creature.VisibilityBonus < -0.5f) continue;


                    var dist = Custom.Dist(creature.mainBodyChunk.pos, pearl.firstChunk.pos);

                    if (dist > 400.0f) continue;

                    if (dist > shortestDist) continue;


                    if (!pearl.room.VisualContact(pearl.firstChunk.pos, creature.mainBodyChunk.pos)) continue;

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
            bool invalidTarget = false;

            if (!Custom.DistLess(target.mainBodyChunk.pos, pearl.firstChunk.pos, 500.0f))
                invalidTarget = true;

            if (target.room != pearl.room)
                invalidTarget = true;

            if (target.dead)
                invalidTarget = true;

            if (!pearl.room.VisualContact(pearl.firstChunk.pos, target.mainBodyChunk.pos))
                invalidTarget = true;


            if (invalidTarget)
                RageTarget = null;
        }


        if (RageTarget == null || !RageTarget.TryGetTarget(out target)) return;
        if (AbilityCounter <= 0)
            module.CooldownTimer = cooldownTime;

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

            pearl.room.AddObject(new ShockWave(targetPos, 30.0f, 0.4f, 5, false));
            room.AddObject(new ExplosionSpikes(pearl.room, targetPos, 5, 20.0f, 10, 20.0f, 20.0f, addon.SymbolColor));

            target.Violence(player.mainBodyChunk, null, target.mainBodyChunk, null, Creature.DamageType.Explosion, shootDamage, 5.0f);

            if (!playerInRange)
                AbilityCounter--;

            else
                room.ConnectEffect(player.firstChunk.pos, pearl.firstChunk.pos, addon.SymbolColor, lifeTime: 24);
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

        LightSource?.Destroy();
        ShieldHoldLoop?.Stop();

        foreach (var shortcut in LockedShortcutsSprites.Values)
            shortcut.RemoveFromContainer();

        if (OwnerRef.TryGetTarget(out var owner) && owner.TryGetModule(out var module))
        {
            if (room != null && owner.realizedObject != null && owner.TryGetAddon(out var addon))
            {
                room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 0.5f, 0.5f);

                room.AddObject(new ExplosionSpikes(room, owner.realizedObject.firstChunk.pos, 5, 30.0f, 10, 10.0f, 20.0f, addon.SymbolColor));
                room.AddObject(new LightningMachine.Impact(owner.realizedObject.firstChunk.pos, 0.1f, addon.SymbolColor, true));
            }

            if (SentryData.TryGetValue(owner, out _))
                SentryData.Remove(owner);

            if (module.IsSentry)
                module.RemoveSentry(owner);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[6];

        sLeaser.sprites[0] = new FSprite("pixel");

        sLeaser.sprites[1] = new FSprite("pearlcat_sentryhalo")
        {
            //shader = rCam.room.game.rainWorld.Shaders["GateHologram"],
        };

        sLeaser.sprites[2] = new FSprite("pearlcat_shieldsentry")
        {
            shader = rCam.room.game.rainWorld.Shaders["Hologram"],
        };

        sLeaser.sprites[3] = new FSprite("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["GravityDisruptor"],
            scale = 0.0f,
        };

        sLeaser.sprites[4] = new("pixel")
        {
            shader = rCam.room.game.rainWorld.Shaders["HologramBehindTerrain"],
        };

        sLeaser.sprites[5] = new("pixel")
        {
            shader = rCam.room.game.rainWorld.Shaders["GateHologram"],
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

        if (owner.realizedObject is not DataPearl pearl) return;

        if (!pearl.abstractPhysicalObject.TryGetAddon(out var addon)) return;

        var effect = pearl.abstractPhysicalObject.GetPOEffect();

        var targetPos = Vector2.Lerp(pearl.firstChunk.lastPos, pearl.firstChunk.pos, timeStacker) - camPos;

        var symbolSprite = sLeaser.sprites[0];
        var haloSprite = sLeaser.sprites[1];
        var guideSprite = sLeaser.sprites[2];
        var shieldSprite = sLeaser.sprites[3];
        var laserSprite = sLeaser.sprites[4];
        var counterSprite = sLeaser.sprites[5];

        symbolSprite.element = Futile.atlasManager.GetElementWithName(addon.DrawSymbolCooldown && AbilityCounter > 0 ? "pearlcat_glyphcooldown" : ObjectAddon.SpriteFromMajorEffect(effect.MajorEffect));
        symbolSprite.SetPosition(targetPos + new Vector2(15.0f, 15.0f));
        symbolSprite.color = addon.SymbolColor;

        haloSprite.SetPosition(targetPos);
        haloSprite.rotation = AnimCounter;
        haloSprite.color = addon.SymbolColor;

        guideSprite.SetPosition(targetPos);
        guideSprite.color = addon.SymbolColor;
        guideSprite.isVisible = effect.MajorEffect == MajorEffectType.SHIELD || (effect.MajorEffect == MajorEffectType.RAGE && AbilityCounter <= 0);
        guideSprite.element = Futile.atlasManager.GetElementWithName(effect.MajorEffect == MajorEffectType.RAGE ? "pearlcat_shieldsentry" : "pearlcat_shieldsentry");

        shieldSprite.SetPosition(targetPos);
        shieldSprite.scale = Mathf.Lerp(shieldSprite.scale, ShieldTimer > 5 ? 8.0f : 0.0f, 0.1f);


        laserSprite.isVisible = RageTarget != null && AbilityCounter > 0;
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
        counterSprite.isVisible = AbilityCounter != null;
        counterSprite.color = addon.SymbolColor;
        counterSprite.element = Futile.atlasManager.GetElementWithName(ObjectAddon.SpriteFromNumber(AbilityCounter ?? -1) ?? "pearlcat_glyphcooldown");


        foreach (var shortcut in room.shortcuts)
        {
            if (!LockedShortcutsSprites.TryGetValue(shortcut, out var sprite)) continue;

            if (sprite.container == null)
                rCam.ReturnFContainer("HUD").AddChild(sprite);

            sprite.color = addon.SymbolColor;
            sprite.SetPosition(room.MiddleOfTile(shortcut.startCoord) - camPos);

            sprite.isVisible = !addon.DrawSymbolCooldown || ShieldTimer > 0;
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        foreach (var sprite in sLeaser.sprites)
        {
            newContatiner.AddChild(sprite);
        }
    }
}
