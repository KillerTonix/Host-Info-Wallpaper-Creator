using Microsoft.Win32;
using System.Windows;

namespace Host_Info_Wallpaper_Creator
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



        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Base Wallpaper",
                DefaultDirectory = "C:\\Windows\\Web\\Screen"

            };
            if (ofd.ShowDialog() == true)
            {
                txtImagePath.Text = ofd.FileName;
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (rbUseImage.IsChecked == true && string.IsNullOrWhiteSpace(txtImagePath.Text))
            {
                MessageBox.Show("Please select an image or switch to Solid Color.", "Missing Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                btnApply.IsEnabled = false;
                btnApply.Content = "Applying... Please wait.";
                var BackgroundColor = cpBgColor.SelectedColor.HasValue ? System.Drawing.Color.FromArgb(cpBgColor.SelectedColor.Value.A, cpBgColor.SelectedColor.Value.R, cpBgColor.SelectedColor.Value.G, cpBgColor.SelectedColor.Value.B) : System.Drawing.Color.Black;
                WallpaperConfig config = new()
                {
                    UseImage = rbUseImage.IsChecked ?? true,
                    ImagePath = txtImagePath.Text,
                    BackgroundColor = BackgroundColor,
                    FontFamilyName = txtFontFamily.Text,
                    BigFontSize = int.Parse(numBigFont.Text),
                    SmallFontSize = int.Parse(numSmallFont.Text),
                    ShowHostname = chkShowHost.IsChecked ?? true,
                    ShowIp = chkShowIp.IsChecked ?? true,
                    ShowCores = chkShowCores.IsChecked ?? true,
                    ShowRam = chkShowRam.IsChecked ?? true,
                    ShowDrives = chkShowDrives.IsChecked ?? true
                };

                WallpaperEngine.ApplySysInfoWallpaper(config);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnApply.IsEnabled = true;
                btnApply.Content = "Apply Wallpaper";
            }
        }
    }
}