using Menu;
using System;
using UnityEngine;

namespace Pearlcat;

public class MenuIllustrationModule
{
    public WeakReference<MenuDepthIllustration> IllustrationRef { get; set; }
    public int Index { get; }
    public Vector2 InitialPos { get; }

    public Vector2 setPos;
    public Vector2 vel;

    public MenuIllustrationModule(MenuDepthIllustration illustration, int index)
    {
        IllustrationRef = new(illustration);
        Index = index;
        InitialPos = illustration.pos;
        
        setPos = illustration.pos;
    }
}
