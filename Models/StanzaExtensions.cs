using System.ComponentModel.DataAnnotations.Schema;

namespace RevivalSong.Models;

public partial class Stanza
{
    [NotMapped] public string Disabled { get; set; } = "False";
    [NotMapped]
    public string LangFgColor { get; set; } = "#60a5fa";
    [NotMapped]
    public string NameFgColor { get; set; } = "#aaa";
    [NotMapped]
    public string BgColor { get; set; }
    
    public Stanza Clone()
    {
        return (Stanza)this.MemberwiseClone();
    }
}