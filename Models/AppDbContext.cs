using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace RevivalSong.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<SongTranslation> SongTranslations { get; set; }

    public virtual DbSet<Songlist> Songlists { get; set; }

    public virtual DbSet<SonglistItem> SonglistItems { get; set; }

    public virtual DbSet<Stanza> Stanzas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string appDir = AppContext.BaseDirectory;
        string dbPath = Path.Combine(appDir, "songs_database.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Song>(entity =>
        {
            entity.ToTable("songs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assembly).HasColumnName("assembly");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Bpm).HasColumnName("bpm");
            entity.Property(e => e.CcliNumber).HasColumnName("ccli_number");
            entity.Property(e => e.Copyright).HasColumnName("copyright");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DefaultKey).HasColumnName("default_key");
            entity.Property(e => e.DefaultLanguageOrder)
                .HasDefaultValue("CS")
                .HasColumnName("default_language_order");
            entity.Property(e => e.Grk).HasColumnName("grk");
            entity.Property(e => e.Gsn).HasColumnName("gsn");
            entity.Property(e => e.Scripture).HasColumnName("scripture");
            entity.Property(e => e.SongNumber).HasColumnName("song_number");
            entity.Property(e => e.SongType)
                .HasDefaultValue("worship")
                .HasColumnName("song_type");
            entity.Property(e => e.TimeSignature).HasColumnName("time_signature");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<SongTranslation>(entity =>
        {
            entity.ToTable("song_translations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AltTitle).HasColumnName("alt_title");
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.SongId).HasColumnName("song_id");
            entity.Property(e => e.StanzaOrder).HasColumnName("stanza_order");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.TitleClean).HasColumnName("title_clean");
            entity.Property(e => e.Translator).HasColumnName("translator");

            entity.HasOne(d => d.Song).WithMany(p => p.SongTranslations).HasForeignKey(d => d.SongId);
        });

        modelBuilder.Entity<Songlist>(entity =>
        {
            entity.ToTable("songlists");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EventDate)
                .HasColumnType("DATE")
                .HasColumnName("event_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<SonglistItem>(entity =>
        {
            entity.ToTable("songlist_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomKey).HasColumnName("custom_key");
            entity.Property(e => e.CustomLanguageOrder).HasColumnName("custom_language_order");
            entity.Property(e => e.CustomStanzaOrder).HasColumnName("custom_stanza_order");
            entity.Property(e => e.ItemOrder).HasColumnName("item_order");
            entity.Property(e => e.ItemType)
                .HasDefaultValue("song")
                .HasColumnName("item_type");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SectionTitle).HasColumnName("section_title");
            entity.Property(e => e.SongId).HasColumnName("song_id");
            entity.Property(e => e.SonglistId).HasColumnName("songlist_id");
        });

        modelBuilder.Entity<Stanza>(entity =>
        {
            entity.ToTable("stanzas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KeyChange).HasColumnName("key_change");
            entity.Property(e => e.Label).HasColumnName("label");
            entity.Property(e => e.Language)
                .HasDefaultValue("CS")
                .HasColumnName("language");
            entity.Property(e => e.Lyrics).HasColumnName("lyrics");
            entity.Property(e => e.SectionNumber)
                .HasColumnName("section_number");
            entity.Property(e => e.SectionType).HasColumnName("section_type");
            entity.Property(e => e.SongId).HasColumnName("song_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.SplitParenthesis)
                .HasDefaultValue(0)
                .HasColumnName("split_parenthesis");

            entity.HasOne(d => d.Song).WithMany(p => p.Stanzas).HasForeignKey(d => d.SongId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
