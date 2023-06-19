using IL.Menu;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class MenuIllustrationModule
{
    public bool isMenuIllustrationInit = false;
    public readonly string name;

    public readonly WeakReference<Menu.MenuDepthIllustration> IllustrationRef;
    public readonly WeakReference<Menu.MenuScene> MenuSceneRef;

    public readonly WeakReference<Menu.MenuDepthIllustration>? GlowRef = null!;

    public MenuIllustrationModule(Menu.MenuScene menuScene, Menu.MenuDepthIllustration illustration)
    {
        name = illustration.fileName;
        IllustrationRef = new WeakReference<Menu.MenuDepthIllustration>(illustration);
        MenuSceneRef = new WeakReference<Menu.MenuScene>(menuScene);

        GlowRef = new WeakReference<Menu.MenuDepthIllustration>(menuScene.depthIllustrations.FirstOrDefault(illustration => illustration.fileName == name + "_glow"));
    }

    public Vector2 pos = Vector2.zero;
    public Color color = Color.white;

    public int animationStacker = 0;

    public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    public const int framesToCycle = 150;

    public Vector2 maxPos;
    public Vector2 minPos;
    public int dir = 1;

    public void Update()
    {
        if (!IllustrationRef.TryGetTarget(out var illustration)) return;

        Menu.MenuDepthIllustration? glow = null;
        //GlowRef?.TryGetTarget(out glow);

        if (Path.GetFileNameWithoutExtension(name).Contains("pearl"))
            UpdatePearl(illustration, glow);

        isMenuIllustrationInit = true;
    }

    public void UpdatePearl(Menu.MenuDepthIllustration illustration, Menu.MenuDepthIllustration? glow)
    {
        if (!isMenuIllustrationInit)
        {
            illustration.color = Random.ColorHSV(0.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f);

            if (glow != null)
                glow.color = name.StartsWith("activepearl") ? Color.red : Color.blue;
                

            maxPos = illustration.pos + new Vector2(0.0f, 35.0f * Mathf.InverseLerp(6.0f, 1.0f, illustration.depth));
            minPos = illustration.pos + new Vector2(0.0f, -35.0f * Mathf.InverseLerp(6.0f, 1.0f, illustration.depth));

            if (char.IsDigit(name.Last()))
            {
                animationStacker += name.Last() * 40;
                animationStacker %= framesToCycle;
            }
        }

        UpdateLinearMovement(illustration, glow);
    }

    public void UpdateLinearMovement(Menu.MenuDepthIllustration illustration, Menu.MenuDepthIllustration? glow)
    {
        Vector2 targetPos = Vector2.Lerp(minPos, maxPos, curve.Evaluate((float)animationStacker / framesToCycle));

        illustration.pos = targetPos;

        if (glow != null) 
            glow.pos = targetPos;


        if (animationStacker + dir > framesToCycle || animationStacker + dir < 0)
            dir *= -1;
        
        animationStacker += dir;
    }
}
