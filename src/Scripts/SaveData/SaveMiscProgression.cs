﻿using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class SaveMiscProgression
{
    // Meta
    public bool IsNewPearlcatSave { get; set; } = true;
    public bool IsMSCSave { get; set; }


    // Menu Pearls
    [JsonProperty(ItemConverterType = typeof(JsonColorHandler))]
    public List<Color> StoredPearlColors { get; } = new();

    [JsonConverter(typeof(JsonColorHandler))]
    public Color? ActivePearlColor { get; set; }


    // Story
    public bool HasPearlpup { get; set; }
    public bool IsPearlpupSick { get; set; }
    public bool HasOEEnding { get; set; }
    public bool JustAscended { get; set; }
    public bool Ascended { get; set; }



    // DEPRECATED
    public bool AltEnd { get; set; }
}