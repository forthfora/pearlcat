using Menu;
using System;
using UnityEngine;

namespace Pearlcat;

public class MenuPearlModule
{
    public readonly int index;
    public readonly WeakReference<MenuDepthIllustration> IllustrationRef;

    public Vector2 initialPos = Vector2.zero;
    public Vector2 vel = Vector2.zero;

    public MenuPearlModule(MenuDepthIllustration illustration, int index)
    {
        this.index = index;
        IllustrationRef = new(illustration);

        initialPos = illustration.pos;
    }
}
