using Music;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheSacrifice
{
    internal partial class Hooks
    {
        private static void ApplyMusicHooks()
        {
            On.Music.MusicPlayer.NewRegion += MusicPlayer_NewRegion;

            On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
        }

        private static void ProceduralMusicInstruction_ctor(On.Music.ProceduralMusic.ProceduralMusicInstruction.orig_ctor orig, ProceduralMusic.ProceduralMusicInstruction self, string name)
        {
            orig(self, name);

            string path = AssetManager.ResolveFilePath("Music" + Path.DirectorySeparatorChar.ToString() + "Procedural" + Path.DirectorySeparatorChar.ToString() + name + ".txt");
            
            if (!File.Exists(path)) return;

            string[] array = File.ReadAllLines(path);

            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], " : ");
                if (array2.Length != 0 && array2[0].Length > 4 && array2[0] == "Layer")
                {
                    self.layers.Add(new ProceduralMusic.ProceduralMusicInstruction.Layer(self.layers.Count));
                    string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
                    for (int j = 0; j < array3.Length; j++)
                    {
                        if (array3[j].Length > 0)
                        {
                            for (int k = 0; k < self.tracks.Count; k++)
                            {
                                string text2 = "";
                                string a;
                                if (array3[j].Length > 3 && array3[j].Substring(0, 1) == "{" && array3[j].Contains("}"))
                                {
                                    text2 = array3[j].Substring(1, array3[j].IndexOf("}") - 1);
                                    a = array3[j].Substring(array3[j].IndexOf("}") + 1);
                                }
                                else
                                {
                                    a = array3[j];
                                }
                                if (a == self.tracks[k].name)
                                {
                                    string[]? subRegions = null;
                                    int dayNight = 0;
                                    bool mushroom = false;
                                    if (text2 != "")
                                    {
                                        if (text2 == "D")
                                        {
                                            dayNight = 1;
                                        }
                                        else if (text2 == "N")
                                        {
                                            dayNight = 2;
                                        }
                                        else if (text2 == "M")
                                        {
                                            mushroom = true;
                                        }
                                        else
                                        {
                                            subRegions = text2.Split(new char[]
                                            {
                                                    '|'
                                            });
                                        }
                                    }
                                    self.tracks[k].subRegions = subRegions;
                                    self.tracks[k].dayNight = dayNight;
                                    self.tracks[k].mushroom = mushroom;
                                    self.layers[self.layers.Count - 1].tracks.Add(self.tracks[k]);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (array2.Length != 0 && array2[0].Length > 0)
                {
                    self.tracks.Add(new ProceduralMusic.ProceduralMusicInstruction.Track(array2[0]));
                    string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
                    for (int l = 0; l < array4.Length; l++)
                    {
                        if (array4[l].Length > 0)
                        {
                            if (array4[l] == "<PA>")
                            {
                                self.tracks[self.tracks.Count - 1].remainInPanicMode = true;
                            }
                            else
                            {
                                self.tracks[self.tracks.Count - 1].tags.Add(array4[l]);
                            }
                        }
                    }
                }
            }
        }

        private static void MusicPlayer_NewRegion(On.Music.MusicPlayer.orig_NewRegion orig, MusicPlayer self, string newRegion)
        {
            orig(self, "AS");
        }
    }
}
