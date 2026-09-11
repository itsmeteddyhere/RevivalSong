namespace RevivalSong.Models;

public partial class Stanza
{
    public bool IsSkipped { get; set; }
    
    public Stanza Clone()
    {
        return (Stanza)this.MemberwiseClone();
    }
}