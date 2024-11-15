using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.PearlEffect;

namespace Pearlcat;

public static partial class PearlEffectManager
{
    public static Dictionary<DataPearlType, PearlEffect> PearlEffects { get; } = new();
    public static Dictionary<string, PearlEffect> CustomPearlEffects { get; } = new();


    public static Dictionary<float, MajorEffectType> EffectColors { get; } = new()
    {
        { 0.0f, MajorEffectType.RAGE },

        { 0.1f, MajorEffectType.SHIELD },
        { 0.16f, MajorEffectType.SHIELD },

        { 0.32f, MajorEffectType.REVIVE },
        { 0.55f, MajorEffectType.AGILITY },

        { 1.0f, MajorEffectType.RAGE },
    };
    public static MajorEffectType GetEffectByHue(float hue)
    {
        return EffectColors.OrderBy(x => Mathf.Abs(hue - x.Key)).First().Value;
    }


    public static PearlEffect GetPearlEffect(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject is DataPearl.AbstractDataPearl abstractPearl)
        {    
            var effect = abstractPearl.GetPearlEffect();
        
            if ((effect.MajorEffect == MajorEffectType.AGILITY && ModOptions.DisableAgility.Value)
                || (effect.MajorEffect == MajorEffectType.SPEAR_CREATION && ModOptions.DisableSpear.Value)
                || (effect.MajorEffect == MajorEffectType.RAGE && ModOptions.DisableRage.Value)
                || (effect.MajorEffect == MajorEffectType.REVIVE && ModOptions.DisableRevive.Value)
                || (effect.MajorEffect == MajorEffectType.SHIELD && ModOptions.DisableShield.Value)
                || (effect.MajorEffect == MajorEffectType.CAMOFLAGUE && ModOptions.DisableCamoflague.Value))
            {
                var newEffect = Misc;

                newEffect.MajorEffect = effect.MajorEffect;
                newEffect.ThreatMusic = effect.ThreatMusic;

                return newEffect;
            }

            return effect;
        }

        return None;
    }

    public static PearlEffect GetPearlEffect(this DataPearl.AbstractDataPearl abstractPearl)
    {
        if (abstractPearl is PebblesPearl.AbstractPebblesPearl pebblesPearl)
        {
            return pebblesPearl.color switch
            {
                -1 => IteratorBlue,
                0 => IteratorOrange,
                1 => IteratorWhite,
                2 => IteratorBlack,

                _ => IteratorBlack,
            };
        }

        if (PearlEffects.TryGetValue(abstractPearl.dataPearlType, out var POEffect))
        {
            return POEffect;
        }

        if (CustomPearlEffects.TryGetValue(abstractPearl.dataPearlType.ToString(), out var CustomPOEffect))
        {
            return CustomPOEffect;
        }

        var pearlColor = abstractPearl.GetObjectColor();
        var hsl = Custom.RGB2HSL(pearlColor);

        var hue = hsl.x;
        var sat = hsl.y;
        var lit = hsl.z;

        if (lit < 0.05f)
        {
            return AsPearlBlack;
        }
        
        if (lit > 0.95f || sat < 0.1f)
        {
            return Misc;
        }

        var effect = Color;
        effect.MajorEffect = GetEffectByHue(hue);

        return effect;
    }
}
