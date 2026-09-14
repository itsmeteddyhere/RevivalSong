using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevivalSong.Models;

[Table("scripturelists")]
public class Scripturelist
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }
}

[Table("scripturelist_items")]
public class ScripturelistItem
{
    [Column("id")]
    public int Id { get; set; }

    [Column("scripturelist_id")]
    public int ScripturelistId { get; set; }

    [Column("item_order")]
    public int ItemOrder { get; set; }

    [Column("book")]
    public string Book { get; set; } = string.Empty;

    [Column("chapter")]
    public int Chapter { get; set; }

    [Column("verse")]
    public int Verse { get; set; }
}