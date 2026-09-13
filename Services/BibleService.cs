using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RevivalSong.Models;

namespace RevivalSong.Services;

public static class BibleService
{
    public static List<BibleTranslation> LoadAvailableTranslations(string biblesDirectoryPath)
    {
        var translations = new List<BibleTranslation>();
        
        if (!Directory.Exists(biblesDirectoryPath))
        {
            return translations;
        }

        var xmlFiles = Directory.GetFiles(biblesDirectoryPath, "*.xml");
        foreach (var file in xmlFiles)
        {
            translations.Add(new BibleTranslation
            {
                Name = Path.GetFileNameWithoutExtension(file),
                FilePath = file
            });
        }

        return translations.OrderBy(t => t.Name).ToList();
    }

    public static BibleTranslation? LoadTranslationData(BibleTranslation translation)
    {
        if (!File.Exists(translation.FilePath)) return null;

        try
        {
            var doc = XDocument.Load(translation.FilePath);
            
            // Assuming the root has <Verses> elements
            var versesElements = doc.Root?.Elements("Verses");
            if (versesElements == null) return translation;

            var currentBook = new BibleBook();
            var currentChapter = new BibleChapter();

            foreach (var vElement in versesElements)
            {
                string bookName = vElement.Element("Book")?.Value ?? "";
                string chapterStr = vElement.Element("Chapter")?.Value ?? "";
                string verseStr = vElement.Element("Verse")?.Value ?? "";
                string text = vElement.Element("VerseText")?.Value ?? "";

                int.TryParse(chapterStr, out int chapterNum);
                int.TryParse(verseStr, out int verseNum);

                // Check if book changed
                if (currentBook.Name != bookName)
                {
                    currentBook = new BibleBook { Name = bookName };
                    translation.Books.Add(currentBook);
                    currentChapter = new BibleChapter { Number = -1 }; // Force chapter creation
                }

                // Check if chapter changed
                if (currentChapter.Number != chapterNum)
                {
                    currentChapter = new BibleChapter { Number = chapterNum };
                    currentBook.Chapters.Add(currentChapter);
                }

                // Add verse
                currentChapter.Verses.Add(new BibleVerse
                {
                    Number = verseNum,
                    Text = text
                });
            }
            
            return translation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing bible XML: {ex.Message}");
            return null;
        }
    }
}
