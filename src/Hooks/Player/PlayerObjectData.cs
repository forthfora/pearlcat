using RWCustom;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsPlayerObject(this AbstractPhysicalObject targetObject)
    {
        var playerData = targetObject.world.game.GetAllPlayerData();

        foreach (var playerModule in playerData)
            if (playerModule.Inventory.Any(abstractObject => abstractObject == targetObject))
                return true;

        return false;
    }

    public static bool IsStorable(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is DataPearl.AbstractDataPearl abstractPearl)
        {
            if (abstractPearl.IsHeartPearl()) return false;
        }

        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) return true;

        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return true;

        // Pearl Spear
        if (abstractObject is AbstractSpear spear && spear.TryGetSpearModule(out _)) return true;

        return false;
    }

    public static Player? TryGetPlayerObjectOwner(this AbstractPhysicalObject targetObject)
    {
        var playerData = targetObject.world.game.GetAllPlayerData();

        foreach (var playerModule in playerData)
        {
            var obj = playerModule.Inventory.FirstOrDefault(abstractObject => abstractObject == targetObject);

            if (obj != null && playerModule.PlayerRef.TryGetTarget(out var player))
            {
                return player;
            }
        }

        return null;
    }

    public static Color GetObjectColor(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject == null)
        {
            return Color.white;
        }

        if (abstractObject is DataPearl.AbstractDataPearl dataPearl)
        {
            if (dataPearl is PebblesPearl.AbstractPebblesPearl pebblesPearl)
            {
                return GetDataPearlColor(dataPearl.dataPearlType, pebblesPearl.color);
            }

            return GetDataPearlColor(dataPearl.dataPearlType);
        }

        if (abstractObject is AbstractCreature abstractCreature)
        {
            var critSymbolData = CreatureSymbol.SymbolDataFromCreature(abstractCreature);

            return CreatureSymbol.ColorOfCreature(critSymbolData);
        }
        else
        {
            var symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);
            
            if (symbolData != null)
            {
                return ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData);
            }
        }

        return Color.white;
    }

    public static Color GetDataPearlColor(this DataPearl.AbstractDataPearl.DataPearlType type, int pebblesPearlColor = 0)
    {
        if (type == null)
            return Color.white;

        if (type == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
        {
            switch (Mathf.Abs(pebblesPearlColor))
            {
                case 1:
                    return new(0.7f, 0.7f, 0.7f);

                case 2:
                    if (pebblesPearlColor < 0)
                        return new(1f, 122f / 255f, 2f / 255f);

                    return new(0.01f, 0.01f, 0.01f);

                default:
                    if (pebblesPearlColor < 0)
                        return new(0f, 116f / 255f, 163f / 255f);

                    return new(1f, 122f / 255f, 2f / 255f);
            }
        }

        return DataPearl.UniquePearlMainColor(type);
    }


    public static Vector2 GetActiveObjectPos(this Player self, Vector2? overrideOffset = null, float timeStacker = 1.0f)
    {
        if (!ActiveObjectOffset.TryGet(self, out var activeObjectOffset))
            activeObjectOffset = Vector2.zero;

        if (overrideOffset != null)
            activeObjectOffset = overrideOffset.Value;

        var playerGraphics = (PlayerGraphics)self.graphicsModule;

        var pos = Vector2.Lerp(playerGraphics.head.lastPos, playerGraphics.head.pos, timeStacker) + activeObjectOffset;
        pos.x += self.mainBodyChunk.vel.x * 1.0f;

        if (self.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0 || self.onBack?.IsPearlcat() == true)
            pos.y += self.onBack?.IsPearlcat() == true ? 40.0f : 30.0f;

        return pos;
    }


    public static ConditionalWeakTable<AbstractPhysicalObject, StrongBox<Vector2>> TargetPositions { get; } = new();

    public static void MoveToTargetPos(this AbstractPhysicalObject abstractObject, Player player, Vector2 targetPos)
    {
        var pos = TargetPositions.GetValue(abstractObject, x => new StrongBox<Vector2>());
        pos.Value = targetPos;

        if (abstractObject.TryGetSentry(out _)) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (abstractObject.realizedObject == null) return;

        if (!MinFricSpeed.TryGet(player, out var minFricSpeed)) return;
        if (!MaxFricSpeed.TryGet(player, out var maxFricSpeed)) return;
        if (!MinFric.TryGet(player, out var minFric)) return;
        if (!MaxFric.TryGet(player, out var maxFric)) return;

        if (!CutoffDist.TryGet(player, out var cutoffDist)) return;
        if (!CutoffMinSpeed.TryGet(player, out var cutoffMinSpeed)) return;
        if (!CutoffMaxSpeed.TryGet(player, out var cutoffMaxSpeed)) return;
        if (!DazeMaxSpeed.TryGet(player, out var dazeMaxSpeed)) return;

        if (!MaxDist.TryGet(player, out var maxDist)) return;
        if (!MinSpeed.TryGet(player, out var minSpeed)) return;
        if (!MaxSpeed.TryGet(player, out var maxSpeed)) return;

        var firstChunk = abstractObject.realizedObject.firstChunk;
        var dir = (targetPos - firstChunk.pos).normalized;
        var dist = Custom.Dist(firstChunk.pos, targetPos);

        float speed = dist < cutoffDist ? Custom.LerpMap(dist, 0.0f, cutoffDist, cutoffMinSpeed, playerModule.IsDazed ? dazeMaxSpeed : cutoffMaxSpeed) : Custom.LerpMap(dist, cutoffDist, maxDist, minSpeed, maxSpeed);

        firstChunk.vel *= Custom.LerpMap(firstChunk.vel.magnitude, minFricSpeed, maxFricSpeed, minFric, maxFric);
        firstChunk.vel += dir * speed;

        if (dist < 0.1f)
            firstChunk.pos = targetPos;
    }
}
