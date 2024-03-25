using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
        }

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
            string lastPart = urlOfImage[urlOfImage.Length - 1];

            NameOfImage.Text = lastPart;

            var converter = new ImageSourceConverter();
            DisplayedImage.Source = (ImageSource)converter.ConvertFromString(selectedFileName);

        }

        private void AddNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            Note_Editor note = new Note_Editor();
            note.ShowDialog();
        }
    }
}
