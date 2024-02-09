using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class SaveMiscProgression
{
    // Meta
    public bool IsNewPearlcatSave { get; set; } = true;
    public bool IsMSCSave { get; set; }
    
    public bool IsMiraSkipEnabled { get; set; }
    public bool IsSecretEnabled { get; set; }


    // Menu Pearls
    [JsonProperty(ItemConverterType = typeof(JsonColorHandler))]
    public List<Color> StoredPearlColors { get; } = new();

    [JsonConverter(typeof(JsonColorHandler))]
    public Color? ActivePearlColor { get; set; }


    // Story
    public bool HasPearlpup { get; set; }
    public bool DidHavePearlpup { get; set; }
    
    public bool IsPearlpupSick { get; set; }
    public bool HasOEEnding { get; set; }
    
    public bool JustAscended { get; set; }
    public bool Ascended { get; set; }

    public bool UnlockedMira { get; set; }
    public bool HasTrueEnding { get; set; }


    public void ResetSave()
    {
        HasPearlpup = false;
        DidHavePearlpup = false;
        
        IsPearlpupSick = false;
        HasOEEnding = false;

        Ascended = false;

        UnlockedMira = false;
        HasTrueEnding = false;
    }
}