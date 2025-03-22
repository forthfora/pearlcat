using RWCustom;
using System.Runtime.CompilerServices;
using Color = UnityEngine.Color;

namespace Pearlcat;

public static class PlayerPearl_Helpers_Data
{
    public static ConditionalWeakTable<AbstractPhysicalObject, StrongBox<Vector2>> TargetPositions { get; } = new();

    public static bool IsPlayerPearl(this AbstractPhysicalObject targetObject)
    {
        return targetObject.TryGetPlayerPearlModule(out var playerPearlModule) && playerPearlModule.IsCurrentlyStored;
    }

    public static bool IsObjectStorable(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is DataPearl.AbstractDataPearl abstractPearl)
        {
            if (abstractPearl.IsHeartPearl())
            {
                return false;
            }

            return true;
        }

        // these could be exceptions, I guess (don't remember if I was smart when I wrote this)
        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        {
            return true;
        }

        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
        {
            return true;
        }

        if (ModManager.MSC)
        {
            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
            {
                return true;
            }

            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl)
            {
                return true;
            }
        }

        // Pearl Spear
        if (abstractObject is AbstractSpear spear && spear.TryGetSpearModule(out _))
        {
            return true;
        }

        return false;
    }


    public static bool TryGetPlayerPearlOwner(this AbstractPhysicalObject targetObject, out Player player)
    {
        var playerData = targetObject.world.game.GetAllPearlcatModules();

        foreach (var playerModule in playerData)
        {
            var pearl = playerModule.Inventory.FirstOrDefault(abstractObject => abstractObject == targetObject);

            if (pearl is null || playerModule.PlayerRef is null)
            {
                continue;
            }

            player = playerModule.PlayerRef;
            return true;
        }

        player = null!;
        return false;
    }


    public static Color GetObjectColor(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is null)
        {
            return Color.white;
        }

        if (abstractObject is DataPearl.AbstractDataPearl dataPearl && abstractObject.type.value != "CWPearl") // CW Pearls are a bit different it seems
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

        var symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);
            
        if (symbolData is not null)
        {
            return ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData);
        }

        return Color.white;
    }

    public static Color GetDataPearlColor(this DataPearl.AbstractDataPearl.DataPearlType type, int pebblesPearlColor = 0)
    {
        if (type is null)
        {
            return Color.white;
        }

        if (type == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
        {
            // 0 = orange, 1 = white, 2 = black, -1 = blue, -2 = orange
            switch (Mathf.Abs(pebblesPearlColor))
            {
                case 1:
                    return new(0.7f, 0.7f, 0.7f);

                case 2:
                    if (pebblesPearlColor < 0)
                    {
                        return new(1f, 122f / 255f, 2f / 255f);
                    }

                    return new(0.01f, 0.01f, 0.01f);

                default:
                    if (pebblesPearlColor < 0)
                    {
                        return new(0f, 116f / 255f, 163f / 255f);
                    }

                    return new(1f, 122f / 255f, 2f / 255f);
            }
        }

        if (type == Enums.Pearls.CW_Pearlcat)
        {
            return Custom.hexToColor("0077ff");
        }

        if (type == Enums.Pearls.BigGoldenPearl)
        {
            return Custom.hexToColor("dea700");
        }

        return DataPearl.UniquePearlMainColor(type);
    }


    public static Vector2 GetActivePearlPos(this Player self, Vector2? overrideOffset = null, float timeStacker = 1.0f)
    {
        var activePearlOffset = new Vector2(0.0f, 50.0f);

        if (overrideOffset is not null)
        {
            activePearlOffset = overrideOffset.Value;
        }

        if (self.graphicsModule is null)
        {
            return activePearlOffset;
        }

        var playerGraphics = (PlayerGraphics)self.graphicsModule;

        var pos = Vector2.Lerp(playerGraphics.head.lastPos, playerGraphics.head.pos, timeStacker) + activePearlOffset;
        pos.x += self.mainBodyChunk.vel.x * 1.0f;

        if (self.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0 || self.onBack?.IsPearlcat() == true)
        {
            pos.y += self.onBack?.IsPearlcat() == true ? 40.0f : 30.0f;
        }

        return pos;
    }

    public static void TryAnimateToTargetPos(this AbstractPhysicalObject abstractObject, Player player, Vector2 targetPos)
    {
        var pos = TargetPositions.GetValue(abstractObject, _ => new StrongBox<Vector2>());
        pos.Value = targetPos;

        // If the object's position is being handled by Meadow, don't interfere
        if (!ModCompat_Helpers.RainMeadow_IsMine(abstractObject) && ModCompat_Helpers.RainMeadow_IsPosSynced(abstractObject))
        {
            return;
        }

        if (abstractObject.TryGetSentry(out _))
        {
            return;
        }

        if (abstractObject.TryGetPearlGraphicsModule(out var pearlGraphics) && pearlGraphics.IsActiveRagePearl)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (abstractObject.realizedObject is null)
        {
            return;
        }

        AnimateToTargetPos(abstractObject, targetPos, playerModule);
    }

    public static void AnimateToTargetPos(AbstractPhysicalObject abstractObject, Vector2 targetPos, PlayerModule playerModule)
    {
        // Magic numbers ^^
        const float minFricSpeed = 100.0f;
        const float maxFricSpeed = 70.0f;

        const float minFricMult = 0.999f;
        const float maxFricMult = 0.5f;

        const float cutoffDist = 50.0f;

        const float cutoffMinSpeed = 0.1f;
        const float cutoffMaxSpeed = 12.0f;

        const float dazeMaxSpeed = 2.0f;

        const float maxDist = 1000.0f;

        const float minSpeed = 8.0f;
        const float maxSpeed = 20.0f;


        var firstChunk = abstractObject.realizedObject.firstChunk;
        var dir = (targetPos - firstChunk.pos).normalized;
        var dist = Custom.Dist(firstChunk.pos, targetPos);

        var speed = dist < cutoffDist ? Custom.LerpMap(dist, 0.0f, cutoffDist, cutoffMinSpeed, playerModule.IsDazed ? dazeMaxSpeed : cutoffMaxSpeed) : Custom.LerpMap(dist, cutoffDist, maxDist, minSpeed, maxSpeed);

        firstChunk.vel *= Custom.LerpMap(firstChunk.vel.magnitude, minFricSpeed, maxFricSpeed, minFricMult, maxFricMult);
        firstChunk.vel += dir * speed;

        if (dist < 0.1f)
        {
            firstChunk.pos = targetPos;
        }
    }
}
