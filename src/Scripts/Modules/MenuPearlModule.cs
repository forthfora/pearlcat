using Menu;
using System;
using UnityEngine;

namespace Pearlcat;

public class MenuPearlModule
{
    public WeakReference<MenuDepthIllustration> IllustrationRef { get; set; }
    public int Index { get; private set; }
    public Vector2 InitialPos { get; private set; }

    public Vector2 Vel { get; set; }

    public MenuPearlModule(MenuDepthIllustration illustration, int index)
    {
        IllustrationRef = new(illustration);
        Index = index;
        InitialPos = illustration.pos;
    }
}
