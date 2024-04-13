using Menu;
using System;
using UnityEngine;

namespace Pearlcat;

public class MenuIllustrationModule
{
    public WeakReference<MenuIllustration> IllustrationRef { get; set; }
    
    public int Index { get; }

    public Vector2 InitialPos;
    public Vector2 SetPos;
    public Vector2 Vel;

    public MenuIllustrationModule(MenuIllustration illustration, int index)
    {
        IllustrationRef = new(illustration);
        Index = index;

        InitialPos = illustration.pos;
        SetPos = illustration.pos;
    }
}
