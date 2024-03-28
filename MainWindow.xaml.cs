using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Image_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GalleryNotesManager notesManager;
        public MainWindow()
        {
            InitializeComponent();
            notesManager = new GalleryNotesManager();
        }
        public string imageName;

        private void chooseImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            // Corrected filter syntax and expanded for both jpg and png formats
            openFileDialog1.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            // Adjusted to compare against true for WPF
            if (openFileDialog1.ShowDialog() != true)
            {
                return;
            }

            string selectedFileName = openFileDialog1.FileName;
            string[] urlOfImage = selectedFileName.Split("\\");
            imageName = urlOfImage[urlOfImage.Length - 1];

            NameOfImage.Text = imageName;

            var converter = new ImageSourceConverter();
            DisplayedImage.Source = (ImageSource)converter.ConvertFromString(selectedFileName);
            DisplayNotesForImage(imageName);
        }

        private void AddNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            Note_Editor newNote = new Note_Editor();
            if (newNote.ShowDialog() == true)
            {
                string note = newNote.NoteText;
                notesManager.AddOrUpdateNote(imageName, note);
                DisplayNotesForImage(imageName);
            }
        }

        private void DisplayNotesForImage(string imageName)
        {
            NotesStackPanel.Children.Clear();
            var notes = notesManager.GetNotesForImage(imageName);

            foreach (var note in notes)
            {
                var noteTextBlock = new TextBlock
                {
                    Text = note,
                    TextAlignment = TextAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 50,
                    Width = 110,
                    Margin = new Thickness(5)
                };

                var noteBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(5),
                    Child = noteTextBlock
                };

                NotesStackPanel.Children.Add(noteBorder);
            }
        }
    }
}
