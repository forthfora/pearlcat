namespace Pearlcat;

public class SaveMiscProgression
{
    // Meta
    public bool IsNewPearlcatSave { get; set; } = true;
    public bool IsMSCSave { get; set; } = ModManager.MSC;
    
    public bool IsMiraSkipEnabled { get; set; }
    public bool IsSecretEnabled { get; set; }


    // Menu Pearls
    public List<StoredPearlData> StoredNonActivePearls { get; set; } = [];
    public StoredPearlData? StoredActivePearl { get; set; }

    public class StoredPearlData
    {
        public string DataPearlType { get; set; } = "";
        public int PebblesPearlType { get; set; }

        public Color GetPearlColor(bool includeUnique = false)
        {
            if (Menu_Helpers.GetUniquePearlIllustration(DataPearlType) is not null && !includeUnique)
            {
                return Color.white;
            }

            if (!ExtEnumBase.TryParse(typeof(DataPearl.AbstractDataPearl.DataPearlType), DataPearlType, false, out var type))
            {
                return Color.white;
            }

            if (type is not DataPearl.AbstractDataPearl.DataPearlType dataPearlType)
            {
                return Color.white;
            }

            return dataPearlType.GetDataPearlColor(PebblesPearlType);
        }
    }


    // Story
    public bool HasPearlpup { get; set; }
    public bool HasDeadPearlpup { get; set; }
    public bool DidHavePearlpup { get; set; }
    
    public bool IsPearlpupSick { get; set; }
    public bool HasOEEnding { get; set; }
    
    public bool JustAscended { get; set; }
    public bool Ascended { get; set; }
    public bool AscendedWithPup { get; set; }

    public bool UnlockedMira { get; set; }
    public bool HasTrueEnding { get; set; }


    public void ResetSave()
    {
        StoredNonActivePearls.Clear();
        StoredActivePearl = null;

        IsNewPearlcatSave = true;

        HasPearlpup = false;
        HasDeadPearlpup = false;
        DidHavePearlpup = false;
        
        IsPearlpupSick = false;
        HasOEEnding = false;

        JustAscended = false;
        Ascended = false;
        AscendedWithPup = false;

        UnlockedMira = false;
        HasTrueEnding = false;

        IsMSCSave = ModManager.MSC;
        UnlockedMira = !ModManager.MSC;


        IsSecretEnabled = false;
        IsMiraSkipEnabled = false;
    }
}
