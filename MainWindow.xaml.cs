using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Ookii.Dialogs.Wpf;


namespace Legion_Tactical_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    

    public partial class MainWindow : Window
    {
        //Images
        List<string> images = new List<string>();
        Slides slides = null;
        int currentImageIndex = 0;
        string targetPath = null;
        string imagePath = string.Empty;
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Legion Tactical Launcher";
        string imageFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Legion Tactical Launcher" + "\\LauncherImages";
        string syncFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Legion Tactical Launcher" + "\\Sync";
        BackgroundWorker initialWorker = new BackgroundWorker();
        BackgroundWorker picWorker = new BackgroundWorker();

        //COLORS
        SolidColorBrush ltacOrange = new SolidColorBrush();
        SolidColorBrush ltacGray = new SolidColorBrush();
        SolidColorBrush ltacBlack = new SolidColorBrush();

        //Timer
        System.Windows.Threading.DispatcherTimer picTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer animateTimer = new System.Windows.Threading.DispatcherTimer();

        //BOOLS
        bool a3Good = false;
        bool addonGood = false;
        bool ts3Good = false;

        //INI File
        IniFile ini; 

        //Addons Object
        //public ObservableCollection<CheckedListItem<Addon>> Addons { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //Background Worker
            initialWorker.DoWork += new DoWorkEventHandler(initailWorker_DoWork);
            initialWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(initialWorker_RunWorkerCompleted);
            picWorker.DoWork += new DoWorkEventHandler(picWorker_DoWork);
            picWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(picWorker_RunWorkerCompleted);

            initialWorker.RunWorkerAsync();

            //Timer
            picTimer.Tick += new EventHandler(picTimer_Tick);
            picTimer.Interval = new TimeSpan(0, 0, 10);
            animateTimer.Tick += new EventHandler(animateTimer_Tick);
            animateTimer.Interval = new TimeSpan(0, 0, 8);

            //Setting Colors;
            ltacOrange.Color = Color.FromRgb(245, 134, 32);
            ltacGray.Color = Color.FromRgb(132, 132, 132);
            ltacBlack.Color = Color.FromRgb(28, 28, 28);

            //Hiding Unused Canvases
            txt_Launcher.Foreground = ltacOrange;
            scrollView_Addons.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_Options.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Collapsed;

            ss_LeftText.Content = "Program Started!";

            ini = new IniFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Legion Tactical Launcher\\settings.ini");

            iniSetup();
            arma3DirCheck();
            addonDirCheck();
            ts3DirCheck();

            //Addons = new ObservableCollection<CheckedListItem<Addon>>();
            //Addons.Add(new CheckedListItem<Addon>(new Addon(){Name="Test1"}));
            //Addons.Add(new CheckedListItem<Addon>(new Addon() { Name = "Test2" }));
            //Addons.Add(new CheckedListItem<Addon>(new Addon() { Name = "Test3" }));

            //DataContext = this;
        }

        #region BGWorker

        private void initailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            checkPrereqs();            
            slides = LtacXML.getImageSlides(@"http://www.legiontac.com/ltl-content/launcher.xml");
            DownloadImage(slides.contentRoot + slides.imageSlides[0].imgUri, imageFolder + "\\" + 0 + ".jpg");
            
        }

        private void initialWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            imgBox.Source = new BitmapImage(new Uri(imageFolder + "\\0.jpg"));
            targetPath = slides.imageSlides[currentImageIndex].targetUri;
            currentImageIndex++;
            picWorker.RunWorkerAsync();

        }

        private void picWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            loadPictures();
        }

        private void picWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //This might be a setting we change later
            picTimer.Start();
            animateTimer.Start();

            tb_arma3dir.TextChanged += tb_arma3dir_TextChanged;
            tb_addondir.TextChanged += tb_addondir_TextChanged;
            tb_ts3dir.TextChanged += tb_ts3dir_TextChanged;

        }

        #endregion

        #region Helper Class

        private void iniSetup()
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Legion Tactical Launcher\\settings.ini"))
            {
                MessageBox.Show("No settings have been applied, please fill in the fields");
                ShowAppOtions();
            }

            //Directory Read
            tb_arma3dir.Text = ini.IniReadValue("Directory", "ARMA3");
            tb_addondir.Text = ini.IniReadValue("Directory", "ADDON");
            tb_ts3dir.Text = ini.IniReadValue("Directory", "TS3");
            //FTP Settings
            tb_ftpServer.Text = ini.IniReadValue("FTP", "SERVER");
            tb_ftpPort.Text = ini.IniReadValue("FTP", "PORT");
            tb_ftpUser.Text = ini.IniReadValue("FTP", "USER");
            tb_ftpPass.Text = ini.IniReadValue("FTP", "PASS");
        }

        private void arma3DirCheck()
        {
            string arma3Path = tb_arma3dir.Text;

            if (arma3Path.EndsWith(@"\"))
            arma3Path = arma3Path.TrimEnd('\'');

            if(!File.Exists(arma3Path + "\\arma3.exe"))
            {
                a3dir_pic.Source = new BitmapImage(new Uri("./Resources/redX.png", UriKind.Relative));
                a3Good = false;
            }
            else
            {
                a3dir_pic.Source = new BitmapImage(new Uri("./Resources/greenCheck.png", UriKind.Relative));
                a3Good = true;
            }

        }        

        private void addonDirCheck()
        {

            string addonPath = tb_addondir.Text;

            if (addonPath.EndsWith(@"\"))
                addonPath = addonPath.TrimEnd('\'');

            if (!Directory.Exists(addonPath))
            {
                addondir_pic.Source = new BitmapImage(new Uri("./Resources/redX.png", UriKind.Relative));
                addonGood = false;
            }
            else
            {
                addondir_pic.Source = new BitmapImage(new Uri("./Resources/greenCheck.png", UriKind.Relative));
                addonGood = true;
            }

        }

        private void ts3DirCheck()
        {

            string ts3Path = tb_ts3dir.Text;

            if (ts3Path.EndsWith(@"\"))
                ts3Path = ts3Path.TrimEnd('\'');

            if (!File.Exists(ts3Path + "\\ts3client_win64.exe") && (!File.Exists(ts3Path + "\\ts3client_win32.exe")))
            {
                ts3dir_pic.Source = new BitmapImage(new Uri("./Resources/redX.png", UriKind.Relative));
                ts3Good = false;
            }
            else
            {
                ts3dir_pic.Source = new BitmapImage(new Uri("./Resources/greenCheck.png", UriKind.Relative));
                ts3Good = true;
            }

        }

        private void checkPrereqs()
        {
            if (!Directory.Exists(appData))
            {
                try
                {
                    Directory.CreateDirectory(appData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: Could not create Application Directory, you must run this program as admin!\n" + ex.ToString(), "Fatal Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }

            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
            else
            {
                foreach (string f in Directory.GetFiles(imageFolder))
                {
                    File.Delete(f);
                }
            }

            if (!Directory.Exists(syncFolder))
            {
                Directory.CreateDirectory(syncFolder);
            }
            
        }
        
        private void DownloadImage(string remoteUrl, string localFileName)
        {
            WebClient webclient = new WebClient();
            webclient.DownloadFile(remoteUrl, localFileName);
            webclient.Dispose();
        }

        private void animateOut()
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 1;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            da.AutoReverse = true;
            imgBox.BeginAnimation(OpacityProperty, da);
        }

        private void loadPictures()
        {
            if (slides.imageSlides.Length > 1)
            {
                for (int i = 1; i < slides.imageSlides.Length; i++)
                {
                    DownloadImage(slides.contentRoot + slides.imageSlides[i].imgUri, imageFolder + "\\" + i + ".jpg");
                }
            }

        }

        private void animateTimer_Tick(object sender, EventArgs e)
        {
            animateOut();
            animateTimer.Stop();
        }

        private void picTimer_Tick(object sender, EventArgs e)
        {

            if (currentImageIndex >= slides.imageSlides.Length)
            {

                currentImageIndex = 0;
            }

            imagePath = slides.imageSlides[currentImageIndex].imgUri;
            targetPath = slides.imageSlides[currentImageIndex].targetUri;

            imgBox.Source = new BitmapImage(new Uri(imageFolder + "\\" + currentImageIndex + ".jpg"));
            imgBox.Visibility = System.Windows.Visibility.Collapsed;
            imgBox.Visibility = System.Windows.Visibility.Visible;

            currentImageIndex++;
            animateTimer.Start();
        }

        private void ShowLauncher()
        {
            txt_Launcher.Foreground = ltacOrange;
            txt_Addons.Foreground = ltacGray;
            txt_Options.Foreground = ltacGray;
            txt_LtacUpdates.Foreground = ltacGray;

            scrollView_Addons.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_Options.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowAddons()
        {
            txt_Launcher.Foreground = ltacGray;
            txt_Addons.Foreground = ltacOrange;
            txt_Options.Foreground = ltacGray;
            txt_LtacUpdates.Foreground = ltacGray;

            scrollView_Addons.Visibility = System.Windows.Visibility.Visible;
            scrollView_Options.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowUpdates()
        {
            txt_Launcher.Foreground = ltacGray;
            txt_Addons.Foreground = ltacGray;
            txt_Options.Foreground = ltacGray;
            txt_LtacUpdates.Foreground = ltacOrange;

            scrollView_Addons.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_Options.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Visible;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowAppOtions()
        {
            txt_Launcher.Foreground = ltacGray;
            txt_Addons.Foreground = ltacGray;
            txt_Options.Foreground = ltacGray;
            txt_LtacUpdates.Foreground = ltacGray;

            scrollView_Addons.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_Options.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion

        #region MouseHover Events

        private void image_Close_MouseEnter(object sender, MouseEventArgs e)
        {
            image_Close.Source = new BitmapImage(new Uri("pack://application:,,,/Legion Tactical Launcher;component/Resources/close_black.png", UriKind.Absolute));
        }

        private void image_Close_MouseLeave(object sender, MouseEventArgs e)
        {
            image_Close.Source = new BitmapImage(new Uri("pack://application:,,,/Legion Tactical Launcher;component/Resources/close_gray.png", UriKind.Absolute));
        }

        private void image_Settings_MouseEnter(object sender, MouseEventArgs e)
        {
            image_Settings.Source = new BitmapImage(new Uri("pack://application:,,,/Legion Tactical Launcher;component/Resources/settings_black.png", UriKind.Absolute));
        }

        private void image_Settings_MouseLeave(object sender, MouseEventArgs e)
        {
            image_Settings.Source = new BitmapImage(new Uri("pack://application:,,,/Legion Tactical Launcher;component/Resources/settings_gray.png", UriKind.Absolute));
        }

        private void txt_Launcher_MouseEnter(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Launcher.Foreground.ToString() != "#FFF58620")
            {
                txt_Launcher.Foreground = ltacBlack;
            }
        }

        private void txt_Launcher_MouseLeave(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Launcher.Foreground.ToString() != "#FFF58620")
            {
                txt_Launcher.Foreground = ltacGray;
            }

        }

        private void txt_Addons_MouseEnter(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Addons.Foreground.ToString() != "#FFF58620")
            {
                txt_Addons.Foreground = ltacBlack;
            }

        }

        private void txt_Addons_MouseLeave(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Addons.Foreground.ToString() != "#FFF58620")
            {
                txt_Addons.Foreground = ltacGray;
            }

        }

        private void txt_Options_MouseEnter(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Options.Foreground.ToString() != "#FFF58620")
            {
                txt_Options.Foreground = ltacBlack;
            }

        }

        private void txt_Options_MouseLeave(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_Options.Foreground.ToString() != "#FFF58620")
            {
                txt_Options.Foreground = ltacGray;
            }
        }

        private void txt_LtacUpdates_MouseEnter(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_LtacUpdates.Foreground.ToString() != "#FFF58620")
            {
                txt_LtacUpdates.Foreground = ltacBlack;
            }

        }

        private void txt_LtacUpdates_MouseLeave(object sender, MouseEventArgs e)
        {
            //Checking to see if it's not orange
            if (txt_LtacUpdates.Foreground.ToString() != "#FFF58620")
            {
                txt_LtacUpdates.Foreground = ltacGray;
            }
        }

        private void imgBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (targetPath != null)
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void imgBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void imgBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((targetPath != null) && imgBox.IsMouseOver == true)
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion

        #region Left Click Events

        private void imgBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (targetPath != null)
            {
                Process.Start(targetPath);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void image_Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }        

        private void ShowOptions()
        {
            txt_Launcher.Foreground = ltacGray;
            txt_Addons.Foreground = ltacGray;
            txt_Options.Foreground = ltacOrange;
            txt_LtacUpdates.Foreground = ltacGray;

            scrollView_Addons.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_Options.Visibility = System.Windows.Visibility.Visible;
            scrollView_LtacUpdates.Visibility = System.Windows.Visibility.Collapsed;
            scrollView_AppSettings.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void txt_Launcher_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowLauncher();
        }

        private void txt_Addons_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowAddons();
        }

        private void txt_Options_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowOptions();
        }

        private void txt_LtacUpdates_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowUpdates();
        }

        private void image_Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowAppOtions();
        }

        private void btn_a3dirOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog diag = new VistaFolderBrowserDialog();
            diag.ShowDialog();
            if (diag.SelectedPath != null)
            {
                tb_arma3dir.Text = diag.SelectedPath;
            }
        }

        private void btn_addondirOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog diag = new VistaFolderBrowserDialog();
            diag.ShowDialog();
            if (diag.SelectedPath != null)
            {
                tb_addondir.Text = diag.SelectedPath;
            }
        }

        private void btn_ts3dirOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog diag = new VistaFolderBrowserDialog();
            diag.ShowDialog();
            if (diag.SelectedPath != null)
            {
                tb_ts3dir.Text = diag.SelectedPath;
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            ini.IniWriteValue("Directory", "ARMA3", tb_arma3dir.Text);
            ini.IniWriteValue("Directory", "ADDON", tb_addondir.Text);
            ini.IniWriteValue("Directory", "TS3", tb_ts3dir.Text);
            ini.IniWriteValue("FTP", "SERVER", tb_ftpServer.Text);
            ini.IniWriteValue("FTP", "PORT", tb_ftpPort.Text);
            ini.IniWriteValue("FTP", "USER", tb_ftpUser.Text);
            ini.IniWriteValue("FTP", "PASS", tb_ftpPass.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (a3Good & addonGood & ts3Good)
            {
                //Verified all 3 directories exist, and they have the required exe files present.
                workingTitle(tb_addondir.Text);
            }
            else
            {
                MessageBox.Show("Not all settings are configured, Please click the gear in the top right to define options!");
            }
        }

        #endregion        

        #region LauncherOptions

        private void tb_arma3dir_TextChanged(object sender, TextChangedEventArgs e)
        {
           arma3DirCheck();
        }

        private void tb_addondir_TextChanged(object sender, TextChangedEventArgs e)
        {
            addonDirCheck();
        }

        private void tb_ts3dir_TextChanged(object sender, TextChangedEventArgs e)
        {
            ts3DirCheck();
        }

        #endregion        

        #region FTP

        private bool ServerGood(string ftpIP, string userName, string password)
        {
            bool result = false;
            ftp ftp = new ftp(ftpIP, userName, password);
            string[] temp = ftp.directoryListSimple("");
            
            if (temp.Length < 1)
            {
                MessageBox.Show("Unable to connect to FTP site!");
            }


            return result;
        }
        #endregion

        private void workingTitle(string addonDir)
        {
            //Checking if Master File Already Exists
            if(File.Exists(syncFolder + "\\client_master.sync"))
            {
                //client_master.sync exists, checking high level if everything matches from server;
                //Checking FTP Server First

                if(!ServerGood(tb_ftpServer.Text, tb_ftpUser.Text, tb_ftpPass.Text))
                {
                    MessageBox.Show("FTP Server settings didn't work, sorries.");
                }
                

            }
            else
            {
                //client_master.sync has not been created, we need to do so;
                //Checking FTP Server First
                if (!ServerGood(tb_ftpServer.Text, tb_ftpUser.Text, tb_ftpPass.Text))
                {
                    MessageBox.Show("FTP Server settings didn't work, sorries.");
                }

            }
        }
       
        
    }
}
