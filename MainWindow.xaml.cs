using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace Image_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GalleryNotesManager notesManager;

        private CroppingTool croppingTool;

        private bool isZoomEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
            notesManager = new GalleryNotesManager();

            croppingTool = new CroppingTool(OverlayCanvas);

            DisplayedImage.MouseWheel += DisplayedImage_MouseWheel;
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
            var notes = notesManager.GetNotesForImage(imageName).ToList(); // Ensure it's a list for easy removal

            foreach (var note in notes)
            {
                var noteTextBlock = new TextBlock
                {
                    Text = note,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Height = 50,
                    Width = 100,
                    Margin = new Thickness(5)
                };

                var deleteButton = new Button
                {
                    Content = "X",
                    Height = 20,
                    Width = 15,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.White
                };

                // Attach an event handler to the delete button
                deleteButton.Click += DeleteButton_Click;

                var dockPanel = new DockPanel();
                DockPanel.SetDock(deleteButton, Dock.Right); // Position the delete button on the right
                dockPanel.Children.Add(deleteButton);
                dockPanel.Children.Add(noteTextBlock); // The TextBlock fills the remaining space

                var noteBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(5),
                    Child = dockPanel
                };
                deleteButton.Tag = note;

                NotesStackPanel.Children.Add(noteBorder);
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton)
            {
                var noteToDelete = deleteButton.Tag.ToString();
                var nameOfImg = imageName;

                notesManager.RemoveNoteForImage(nameOfImg, noteToDelete);

                DisplayNotesForImage(nameOfImg);
            }
        }

        private void saveImgBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedImage != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                    Title = "Save Image"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        BitmapSource bitmapSource = (BitmapSource)DisplayedImage.Source;

                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }

                        MessageBox.Show("Image saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save the image. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void rotLBtn_Click(object sender, RoutedEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)DisplayedImage.RenderTransform;

            RotateTransform rotateTransform = null;
            foreach (var transform in transformGroup.Children)
            {
                if (transform is RotateTransform)
                {
                    rotateTransform = (RotateTransform)transform;
                    break;
                }
            }

            if (rotateTransform != null)
            {
                rotateTransform.Angle -= 90;
                if (rotateTransform.Angle >= 360)
                {
                    rotateTransform.Angle = 0;
                }
            }
        }

        private void rotRBtn_Click(object sender, RoutedEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)DisplayedImage.RenderTransform;

            RotateTransform rotateTransform = null;
            foreach (var transform in transformGroup.Children)
            {
                if (transform is RotateTransform)
                {
                    rotateTransform = (RotateTransform)transform;
                    break;
                }
            }

            if (rotateTransform != null)
            {
                rotateTransform.Angle += 90;
                if (rotateTransform.Angle >= 360)
                {
                    rotateTransform.Angle = 0;
                }
            }
        }

        //
        // Následující sekce je dělaná s velikou pomocí ChatGPT
        // Vyskytl se problém, že ořezávání je mnohem těžší, než jsem si myslel
        // Nechám zde i komentáře kódu, které mi k tomu napsal
        //

        private void crop_Click(object sender, RoutedEventArgs e)
        {
            if (croppingTool.croppingRectangle.Visibility == Visibility.Visible)
            {
                CropImage();
                croppingTool.HideCroppingTools(); // This will hide the rectangle and thumbs after cropping
            }
            else
            {
                croppingTool.ShowCroppingTools(); // This should show the rectangle and thumbs for a new cropping session
            }
        }
        private void CropImage()
        {
            var rect = new Rect(Canvas.GetLeft(croppingTool.croppingRectangle), Canvas.GetTop(croppingTool.croppingRectangle), croppingTool.croppingRectangle.Width, croppingTool.croppingRectangle.Height);
            var imageSource = (BitmapSource)DisplayedImage.Source;

            var scaleX = imageSource.PixelWidth / DisplayedImage.ActualWidth;
            var scaleY = imageSource.PixelHeight / DisplayedImage.ActualHeight;

            var x = (int)(rect.X * scaleX);
            var y = (int)(rect.Y * scaleY);
            var width = (int)(rect.Width * scaleX);
            var height = (int)(rect.Height * scaleY);

            var croppedBitmap = new CroppedBitmap(imageSource, new Int32Rect(x, y, width, height));
            DisplayedImage.Source = croppedBitmap;

            croppingTool.HideCroppingTools();
        }

        // Konec sekce

        private void zoomBtn_Click(object sender, RoutedEventArgs e)
        {
            isZoomEnabled = !isZoomEnabled;
            DisplayedImage.Focus();
            zoomBtn.Tag = isZoomEnabled ? "Active" : "Inactive";
        }

        private void DisplayedImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!isZoomEnabled) return;

            var scaleTransform = (ScaleTransform)((TransformGroup)DisplayedImage.RenderTransform).Children[0];

            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double newScaleX = scaleTransform.ScaleX * zoomFactor;
            double newScaleY = scaleTransform.ScaleY * zoomFactor;

            // Assuming initial sizes are captured somewhere as originalWidth and originalHeight
            double newWidth = DisplayedImage.ActualWidth * newScaleX;
            double newHeight = DisplayedImage.ActualHeight * newScaleY;

            // Check if the new dimensions exceed the container's dimensions
            if (newWidth > DisplayedImage.MaxWidth || newHeight > DisplayedImage.MaxHeight)
            {
                // If they do, calculate the maximum possible scale while staying within the bounds
                double scaleXToFit = DisplayedImage.MaxWidth / DisplayedImage.ActualWidth;
                double scaleYToFit = DisplayedImage.MaxHeight / DisplayedImage.ActualHeight;
                double maxScale = Math.Min(scaleXToFit, scaleYToFit);

                scaleTransform.ScaleX = maxScale;
                scaleTransform.ScaleY = maxScale;
            }
            else
            {
                // Apply the new scale
                scaleTransform.ScaleX = newScaleX;
                scaleTransform.ScaleY = newScaleY;
            }
        }
    }
}
