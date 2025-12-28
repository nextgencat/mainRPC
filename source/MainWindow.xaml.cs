using DiscordRPC;
using DiscordRPC.Logging;
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
using System.Windows.Threading;

namespace mainRPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static DiscordRpcClient _client;
        private string clientId;
        private string details;
        private string state;
        private string Button1_Label;
        private string Button1_Url;
        private string Button2_Label;
        private string Button2_Url;
        private bool autoInit = false;
        private bool isConnected = false;

        private DispatcherTimer _activityTimer;
        private DateTime _activityStartTime;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void ActivityTimer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _activityStartTime;

            if (elapsed.TotalHours >= 1)
            {
                // 01:12:45
                Timestamp_TextBlock.Text =
                    $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }
            else
            {
                // 0:12
                Timestamp_TextBlock.Text =
                    $"{elapsed.Minutes}:{elapsed.Seconds:D2}";
            }
        }


        private void WinLoaded(object sender, RoutedEventArgs e)
        {
            Setup();
            _activityTimer = new DispatcherTimer();
            _activityTimer.Interval = TimeSpan.FromSeconds(1);
            _activityTimer.Tick += ActivityTimer_Tick;
        }

        private void SetImage(string source, Image element)
        {
            if (!string.IsNullOrEmpty(source))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(source);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    element.Source = image;
                }
                catch
                {
                    MessageBox.Show($"Cannot get image for {element}");
                }
            }
        }

        private void Close_ButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditConfig_ButtonClick(object sender, RoutedEventArgs e)
        {
            ConfigEditorGrid.Visibility = Visibility.Visible;
        }

        private void SaveConfig_ButtonClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.details = Details_TextBox.Text;
            Properties.Settings.Default.state = State_TextBox.Text;
            Properties.Settings.Default.Button1Label = Button1Label_TextBox.Text;
            Properties.Settings.Default.Button1Url = Button1Url_TextBox.Text;
            Properties.Settings.Default.Button2Label = Button2Label_TextBox.Text;
            Properties.Settings.Default.Button2Url = Button2Url_TextBox.Text;
            Properties.Settings.Default.clientId = ClientID_TextBox.Text;

            Properties.Settings.Default.Save();
            ConfigEditorGrid.Visibility = Visibility.Hidden;

            details = Properties.Settings.Default.details;
            state = Properties.Settings.Default.state;
            Button1_Label = Properties.Settings.Default.Button1Label;
            Button1_Url = Properties.Settings.Default.Button1Url;
            Button2_Label = Properties.Settings.Default.Button2Label;
            Button2_Url = Properties.Settings.Default.Button2Url;
            clientId = Properties.Settings.Default.clientId;

            ActivityDetails_TextBlock.Text = details;
            ActivityState_TextBlock.Text = state;
            Button1_TextBlock.Text = Button1_Label;
            Button2_TextBlock.Text = Button2_Label;
            ActivityID_TextBlock.Text = clientId;

            fetchConfig();

        }
        private void CloseConfigEditor_ButtonClick(object sender, RoutedEventArgs e)
        {
            ConfigEditorGrid.Visibility = Visibility.Hidden;
        }

        private void Initialize_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                Initialize();
            }
        }

        private void Dispose_ButtonClick(object sender, RoutedEventArgs e)
        {
            _activityTimer?.Stop();

            _client?.Dispose();
            _client = null;
            isConnected = false;

            Timestamp_TextBlock.Text = "0:00";

            Dispose_Button.IsEnabled = false;
            Initialize_Button.IsEnabled = true;

        }

        private void fetchConfig()
        {
            details = Properties.Settings.Default.details;
            state = Properties.Settings.Default.state;
            Button1_Label = Properties.Settings.Default.Button1Label;
            Button1_Url = Properties.Settings.Default.Button1Url;
            Button2_Label = Properties.Settings.Default.Button2Label;
            Button2_Url = Properties.Settings.Default.Button2Url;
            clientId = Properties.Settings.Default.clientId;

            ActivityDetails_TextBlock.Text = details;
            ActivityState_TextBlock.Text = state;
            Button1_TextBlock.Text = Button1_Label;
            Button2_TextBlock.Text = Button2_Label;
            ActivityID_TextBlock.Text = clientId;

            Details_TextBox.Text = Properties.Settings.Default.details;
            State_TextBox.Text = Properties.Settings.Default.state;
            Button1Label_TextBox.Text = Properties.Settings.Default.Button1Label;
            Button1Url_TextBox.Text = Properties.Settings.Default.Button1Url;
            Button2Label_TextBox.Text = Properties.Settings.Default.Button2Label;
            Button2Url_TextBox.Text = Properties.Settings.Default.Button2Url;
            ClientID_TextBox.Text = Properties.Settings.Default.clientId;
        }

        private void Initialize()
        {

            _client = new DiscordRpcClient(clientId);

            _client.OnReady += (sender, e) =>
            {
                isConnected = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetImage(e.User.GetAvatarURL(), Avatar_Image);
                    SetImage(e.User.GetAvatarDecorationURL(), AvatarDecoration_Image);
                    Username_TextBlock.Text = e.User.Username;

                    Initialize_Button.IsEnabled = false;
                    Dispose_Button.IsEnabled = true;
                });

                var buttons = new List<DiscordRPC.Button>();

                if (!string.IsNullOrWhiteSpace(Button1_Label))
                {
                    buttons.Add(new DiscordRPC.Button() { Label = Button1_Label, Url = Button1_Url });
                }

                if (!string.IsNullOrWhiteSpace(Button2_Label))
                {
                    buttons.Add(new DiscordRPC.Button() { Label = Button2_Label, Url = Button2_Url });
                }

                _client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    
                    Buttons = buttons.ToArray()
                });

                _activityStartTime = DateTime.Now;
                _activityTimer.Start();

            };

            _client.OnError += (sender, e) =>
            {
                isConnected = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Initialize_Button.IsEnabled = true;
                    Dispose_Button.IsEnabled = false;
                    _client = null;
                });
            };

            _client.Initialize();
        }

        private void Setup()
        {
            //GRABING CONFIG SETTINGS
            fetchConfig();    

            if (!string.IsNullOrEmpty(clientId))
            {
                try
                {         

                    if (autoInit)
                    {
                        Initialize(); 
                    }
                    else
                    {
                        Initialize_Button.IsEnabled = true;
                        Dispose_Button.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot initialize client: {ex}");
                    Initialize_Button.IsEnabled = true;
                    Dispose_Button.IsEnabled = false;
                }
            }
            else
            {
                Properties.Settings.Default.details = "DETAILS HERE";
                Properties.Settings.Default.state = "STATE HERE";
                Properties.Settings.Default.Button1Label = "LABEL 1";
                Properties.Settings.Default.Button1Url = "https://example.com/";
                Properties.Settings.Default.Button2Label = "LABEL 2";
                Properties.Settings.Default.Button2Url = "https://example.com/";
                Properties.Settings.Default.clientId = "YOUR APPLICATION ID HERE";

                Properties.Settings.Default.Save();
                fetchConfig();

                MessageBox.Show("Start config created.");

                Initialize_Button.IsEnabled = true;
                Dispose_Button.IsEnabled = false;
            }

        }

        
    }
}
