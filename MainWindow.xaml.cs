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

        //private Rectangle croppingRectangle;
        //private Thumb topLeft, topRight, bottomLeft, bottomRight;
        //private Point _startPoint;
        //private bool _isDragging;

        private CroppingTool croppingTool;

        private double scale = 1.0;

        public MainWindow()
        {
            InitializeComponent();
            notesManager = new GalleryNotesManager();

            croppingTool = new CroppingTool(OverlayCanvas);
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
        //private void HideCroppingTools()
        //{
        //    croppingRectangle.Visibility = Visibility.Collapsed;  // Hide the rectangle

        //    // Hide all thumbs
        //    topLeft.Visibility = Visibility.Collapsed;
        //    topRight.Visibility = Visibility.Collapsed;
        //    bottomLeft.Visibility = Visibility.Collapsed;
        //    bottomRight.Visibility = Visibility.Collapsed;
        //}
        //private void ShowCroppingTools()
        //{
        //    if (croppingRectangle != null)
        //    {
        //        croppingRectangle.Visibility = Visibility.Visible;  // Show the rectangle

        //        // Show all thumbs
        //        topLeft.Visibility = Visibility.Visible;
        //        topRight.Visibility = Visibility.Visible;
        //        bottomLeft.Visibility = Visibility.Visible;
        //        bottomRight.Visibility = Visibility.Visible;

        //        // Ensure they are correctly positioned around the rectangle
        //        PositionThumbs();
        //    }
        //    else
        //    {
        //        // If the rectangle hasn't been created yet, create it and add thumbs
        //        CreateCroppingRectangle();
        //        AddThumbs();
        //    }
        //}
        //private void CreateCroppingRectangle()
        //{
        //    croppingRectangle = new Rectangle
        //    {
        //        Stroke = Brushes.Blue,
        //        StrokeThickness = 2,
        //        Width = 100,
        //        Height = 100,
        //        Visibility = Visibility.Visible
        //    };

        //    OverlayCanvas.Children.Add(croppingRectangle);
        //    Canvas.SetLeft(croppingRectangle, 50);  // Initial position
        //    Canvas.SetTop(croppingRectangle, 50);   // Initial position
        //    croppingRectangle.MouseLeftButtonDown += CroppingRectangle_MouseLeftButtonDown;
        //    croppingRectangle.MouseMove += CroppingRectangle_MouseMove;
        //    croppingRectangle.MouseLeftButtonUp += CroppingRectangle_MouseLeftButtonUp;
        //}
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

        //private void CroppingRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var rect = sender as Rectangle;
        //    _startPoint = e.GetPosition(OverlayCanvas);
        //    rect.CaptureMouse();
        //    _isDragging = true;
        //}

        //private void CroppingRectangle_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_isDragging && sender is Rectangle rect)
        //    {
        //        Point currentPoint = e.GetPosition(OverlayCanvas);
        //        double offsetX = currentPoint.X - _startPoint.X;
        //        double offsetY = currentPoint.Y - _startPoint.Y;

        //        double newLeft = Math.Max(0, Canvas.GetLeft(rect) + offsetX);
        //        double newTop = Math.Max(0, Canvas.GetTop(rect) + offsetY);

        //        // Prevent the rectangle from moving outside the right boundary
        //        if (newLeft + rect.Width > OverlayCanvas.ActualWidth)
        //        {
        //            newLeft = OverlayCanvas.ActualWidth - rect.Width;
        //        }

        //        // Prevent the rectangle from moving outside the bottom boundary
        //        if (newTop + rect.Height > OverlayCanvas.ActualHeight)
        //        {
        //            newTop = OverlayCanvas.ActualHeight - rect.Height;
        //        }

        //        Canvas.SetLeft(rect, newLeft);
        //        Canvas.SetTop(rect, newTop);
        //        _startPoint = currentPoint;

        //        PositionThumbs();
        //    }
        //}

        //private void CroppingRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    var rect = sender as Rectangle;
        //    rect.ReleaseMouseCapture();
        //    _isDragging = false;
        //}
        //private void AddThumbs()
        //{
        //    InitializeOrReuseThumbs();

        //    // Attach event handlers every time thumbs are initialized or reused
        //    AttachResizeHandler(topLeft, HorizontalAlignment.Left, VerticalAlignment.Top);
        //    AttachResizeHandler(topRight, HorizontalAlignment.Right, VerticalAlignment.Top);
        //    AttachResizeHandler(bottomLeft, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        //    AttachResizeHandler(bottomRight, HorizontalAlignment.Right, VerticalAlignment.Bottom);

        //    PositionThumbs();
        //}
        //private void InitializeOrReuseThumbs()
        //{
        //    // Check and create thumbs if they do not already exist
        //    topLeft ??= CreateThumb(Brushes.Red, Cursors.SizeNWSE);
        //    topRight ??= CreateThumb(Brushes.Red, Cursors.SizeNESW);
        //    bottomLeft ??= CreateThumb(Brushes.Red, Cursors.SizeNESW);
        //    bottomRight ??= CreateThumb(Brushes.Red, Cursors.SizeNWSE);

        //    // Ensure thumbs are added to canvas only if they are not already part of it
        //    AddThumbToCanvas(topLeft);
        //    AddThumbToCanvas(topRight);
        //    AddThumbToCanvas(bottomLeft);
        //    AddThumbToCanvas(bottomRight);
        //}
        //private Thumb CreateThumb(SolidColorBrush background, Cursor cursor)
        //{
        //    return new Thumb
        //    {
        //        Width = 10,
        //        Height = 10,
        //        Background = background,
        //        Cursor = cursor
        //    };
        //}
        //private void AddThumbToCanvas(Thumb thumb)
        //{
        //    // Check if the thumb is already a child of the OverlayCanvas and remove it first if it is
        //    if (OverlayCanvas.Children.Contains(thumb))
        //        OverlayCanvas.Children.Remove(thumb);

        //    OverlayCanvas.Children.Add(thumb);
        //}

        //private void PositionThumbs()
        //{
        //    if (croppingRectangle != null)
        //    {
        //        // Correctly calculate the positions based on the current state of croppingRectangle
        //        Canvas.SetLeft(topLeft, Canvas.GetLeft(croppingRectangle) - topLeft.Width / 2);
        //        Canvas.SetTop(topLeft, Canvas.GetTop(croppingRectangle) - topLeft.Height / 2);

        //        Canvas.SetLeft(topRight, Canvas.GetLeft(croppingRectangle) + croppingRectangle.Width - topRight.Width / 2);
        //        Canvas.SetTop(topRight, Canvas.GetTop(croppingRectangle) - topRight.Height / 2);

        //        Canvas.SetLeft(bottomLeft, Canvas.GetLeft(croppingRectangle) - bottomLeft.Width / 2);
        //        Canvas.SetTop(bottomLeft, Canvas.GetTop(croppingRectangle) + croppingRectangle.Height - bottomLeft.Height / 2);

        //        Canvas.SetLeft(bottomRight, Canvas.GetLeft(croppingRectangle) + croppingRectangle.Width - bottomRight.Width / 2);
        //        Canvas.SetTop(bottomRight, Canvas.GetTop(croppingRectangle) + croppingRectangle.Height - bottomRight.Height / 2);
        //    }
        //}

        //private void AttachResizeHandler(Thumb thumb, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        //{
        //    thumb.DragDelta += (sender, e) =>
        //    {
        //        if (!(sender is Thumb handle))
        //            return;

        //        double originalWidth = croppingRectangle.Width;
        //        double originalHeight = croppingRectangle.Height;
        //        double newX = Canvas.GetLeft(croppingRectangle);
        //        double newY = Canvas.GetTop(croppingRectangle);

        //        // Handling right resizing thumb
        //        if (horizontalAlignment == HorizontalAlignment.Right)
        //        {
        //            double newWidth = originalWidth + e.HorizontalChange;
        //            // Restrict the new width to a minimum and within the canvas bounds
        //            if (newWidth > 10 && newX + newWidth <= OverlayCanvas.ActualWidth)
        //            {
        //                croppingRectangle.Width = newWidth;
        //            }
        //        }
        //        // Handling left resizing thumb
        //        else if (horizontalAlignment == HorizontalAlignment.Left)
        //        {
        //            double newWidth = originalWidth - e.HorizontalChange;
        //            if (newWidth > 10)
        //            {
        //                newX = newX + e.HorizontalChange;
        //                if (newX >= 0 && newX + newWidth <= OverlayCanvas.ActualWidth)  // Ensure the new X is within bounds
        //                {
        //                    croppingRectangle.Width = newWidth;
        //                    Canvas.SetLeft(croppingRectangle, newX);
        //                }
        //            }
        //        }

        //        // Handling bottom resizing thumb
        //        if (verticalAlignment == VerticalAlignment.Bottom)
        //        {
        //            double newHeight = originalHeight + e.VerticalChange;
        //            // Restrict the new height to a minimum and within the canvas bounds
        //            if (newHeight > 10 && newY + newHeight <= OverlayCanvas.ActualHeight)
        //            {
        //                croppingRectangle.Height = newHeight;
        //            }
        //        }
        //        // Handling top resizing thumb
        //        else if (verticalAlignment == VerticalAlignment.Top)
        //        {
        //            double newHeight = originalHeight - e.VerticalChange;
        //            if (newHeight > 10)
        //            {
        //                newY = newY + e.VerticalChange;
        //                if (newY >= 0 && newY + newHeight <= OverlayCanvas.ActualHeight)  // Ensure the new Y is within bounds
        //                {
        //                    croppingRectangle.Height = newHeight;
        //                    Canvas.SetTop(croppingRectangle, newY);
        //                }
        //            }
        //        }

        //        PositionThumbs(); // Update the thumbs' positions after adjusting the rectangle
        //    };
        //}

        private void zoomBtn_Click(object sender, RoutedEventArgs e)
        {
            scale += 0.1;
            DisplayedImage.LayoutTransform = new ScaleTransform(scale, scale);
        }
    }
}
