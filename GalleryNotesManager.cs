using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace Image_Viewer
{
    public class GalleryNotesManager
    {
        private Dictionary<string, List<string>> imageNotes = new Dictionary<string, List<string>>();
        private readonly string dataFilePath = "GalleryNotes.json";

        public GalleryNotesManager()
        {
            LoadNotes();
        }

        public void AddOrUpdateNote(string imageName, string note)
        {
            if (imageNotes.ContainsKey(imageName))
            {
                imageNotes[imageName].Add(note);
            }
            else
            {
                imageNotes[imageName] = new List<string> { note };
            }

            SaveNotes();
        }

        public List<string> GetNotesForImage(string imageName)
        {
            if (imageNotes.TryGetValue(imageName, out var notes))
            {
                return notes;
            }

            return new List<string>(); // Return an empty list if no notes are found
        }

        private void SaveNotes()
        {
            var json = JsonConvert.SerializeObject(imageNotes, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(dataFilePath, json);
        }

        private void LoadNotes()
        {
            if (File.Exists(dataFilePath))
            {
                var json = File.ReadAllText(dataFilePath);
                imageNotes = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            }
        }
    }
}
