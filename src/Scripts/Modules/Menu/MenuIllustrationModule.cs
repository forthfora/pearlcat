using Menu;
using UnityEngine;

namespace Pearlcat;

public class MenuIllustrationModule
{
    public IllustrationType Type { get; set; }

    public enum IllustrationType
    {
        Default,

        PearlNonActive,
        PearlActive,
        PearlActiveHalo,

        PearlHeart,
        PearlPlaceHolder,
    }

    public int NonActivePearlIndex { get; set; }
    public bool HasUniquePearlIllustration { get; set; }

    public Vector2 InitialPos;
    public Vector2 SetPos;
    public Vector2 Vel;

    public void Init(MenuIllustration illustration, IllustrationType type, int nonActivePearlIndex = 0, bool hasUniquePearlIllustration = false)
    {
        Type = type;
        NonActivePearlIndex = nonActivePearlIndex;
        HasUniquePearlIllustration = hasUniquePearlIllustration;

        if (illustration.owner is MenuScene menuScene)
        {
            Menu_Helpers.DetermineIllustrationPosDepthLayer(illustration, menuScene, this);
        }
    }
}
