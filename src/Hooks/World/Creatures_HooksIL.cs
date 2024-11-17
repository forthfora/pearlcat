
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Pearlcat;

public static class Creatures_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.BigEel.JawsSnap += BigEel_JawsSnap;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Lizard.SpearStick += Lizard_SpearStick;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Lizard.Violence += Lizard_Violence;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }


    // Bypass lizard armor if deflected
    // This is so it actually deals damage
    private static void Lizard_Violence(ILContext il)
    {
        var c = new ILCursor(il);
        
        var dest = c.DefineLabel();

        // Grab dest after so these don't count these as mouth shots
        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarga(2),
                _ => true,
                x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitInMouth)),
                x => x.MatchBrfalse(out dest)))
        {
            throw new Exception("Goto Failed");
        }

        // Make this deal damage
        if (!c.TryGotoPrev(MoveType.Before,
                x => x.MatchLdarga(2),
                _ => true,
                x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitHeadShield)),
                x => x.MatchBrfalse(out _)))
        {
            throw new Exception("Goto Failed");
        }

        // Ldarg 0 on the stack
        c.Emit(OpCodes.Ldarg_1); // BodyChunk
        c.EmitDelegate<Func<Lizard, BodyChunk, bool>>((self, bodyChunk) =>
        {
            if (bodyChunk?.owner is not Spear weapon)
            {
                return false;
            }

            var playerData = self.abstractCreature?.world?.game?.GetAllPearlcatModules();

            if (playerData == null)
            {
                return false;
            }

            // wow
            foreach (var module in playerData)
            {
                foreach (var item in module.Inventory)
                {
                    if (item.TryGetPlayerPearlModule(out var poModule))
                    {
                        if (poModule.VisitedObjects.TryGetValue(weapon, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        });

        c.Emit(OpCodes.Brtrue, dest); // dest skips the HitHead check

        c.Emit(OpCodes.Ldarg_0); // return the value we consumed
    }

    // This is so the spear actually embeds
    private static void Lizard_SpearStick(ILContext il)
    {
        var c = new ILCursor(il);

        var dest = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitHeadShield)),
                x => x.MatchBrfalse(out dest)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.After,
                x => x.MatchLdarg(3)))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1); // Weapon
        c.EmitDelegate<Func<Lizard, Weapon, bool>>((self, weapon) =>
        {
            var playerData = self.abstractCreature?.world?.game?.GetAllPearlcatModules();

            if (playerData == null)
            {
                return false;
            }

            // wow
            foreach (var module in playerData)
            {
                foreach (var item in module.Inventory)
                {
                    if (item.TryGetPlayerPearlModule(out var poModule))
                    {
                        if (poModule.VisitedObjects.TryGetValue(weapon, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        });



        //c.Emit(OpCodes.Dup); // need 2 bools: the delegate will consume 1 and the branch the other

        //c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldarg_3); // Body Chunk
        //c.EmitDelegate<Func<bool, Lizard, BodyChunk, BodyChunk>>((wasRedPearlWeapon, self, bodyChunk) =>
        //{
        //    if (wasRedPearlWeapon)
        //    {
        //        // Prevent hitting head directly, this won't actually deal damage unless it's considered a mouth shot (which means unwanted additional checks)
        //        if (bodyChunk.index == 0 || bodyChunk.index == 1)
        //        {
        //            if (self.bodyChunks.Length >= 3)
        //            {
        //                bodyChunk = self.bodyChunks[UnityEngine.Random.Range(2, self.bodyChunks.Length - 1)];
        //            }
        //        }
        //    }

        //    return bodyChunk;
        //});

        //c.Emit(OpCodes.Starg, 3);


        c.Emit(OpCodes.Brtrue, dest); // branch past HitHead check

        c.Emit(OpCodes.Ldarg, 3); // return the value we popped at the start
    }

    // Leviathan
    private static void BigEel_JawsSnap(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<SoundID>(nameof(SoundID.Leviathan_Crush_NPC))))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.After,
                x => x.MatchConvI4(),
                x => x.MatchBlt(out _)))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<BigEel>>((self) =>
        {
            var didRevive = false;

            for (var i = self.clampedObjects.Count - 1; i >= 0; i--)
            {
                var clampedObj = self.clampedObjects[i];
                var obj = clampedObj.chunk?.owner;

                if (obj is not Player player)
                {
                    continue;
                }

                if (!player.TryGetPearlcatModule(out var playerModule))
                {
                    continue;
                }

                if (playerModule.ReviveCount <= 0)
                {
                    continue;
                }


                if (player.graphicsModule != null)
                {
                    self.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(player);
                }

                foreach (var item in playerModule.PostDeathInventory)
                {
                    if (item == obj.abstractPhysicalObject)
                    {
                        self.clampedObjects.Remove(clampedObj);
                    }

                    var graphics = item.realizedObject?.graphicsModule;

                    if (graphics == null)
                    {
                        continue;
                    }

                    self.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(graphics);
                }

                self.Stun(100);

                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, player.firstChunk, false, 2.0f, 0.5f);
                self.room.AddObject(new ShockWave(player.firstChunk.pos, 700f, 0.6f, 90));

                didRevive = true;
            }

            if (didRevive)
            {
                self.clampedObjects.Clear();
            }

            //foreach (var item in self.clampedObjects)
            //    Plugin.Logger.LogWarning(item?.chunk?.owner?.GetType());
        });
    }
}
