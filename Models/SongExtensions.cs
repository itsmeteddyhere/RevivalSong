using System.Linq;

namespace RevivalSong.Models;

public partial class Song
{
    public string PrimaryTitle => SongTranslations?.FirstOrDefault()?.Title ?? "Untitled";
}