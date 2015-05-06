using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MFF_DU6_Chat_WPF
{
    /// <summary>
    /// Urcuje stav prvku v okne
    /// </summary>
    public enum WinState
    {
        Offline,
        Hosting,
        Connected,
        InMiddle
    }

    public partial class MainWindow : Window
    {
        //Instance serveru a clienta okna
        ChatServer server;
        ChatClient client;

        // Urcuje stav prvku v okne (disabled, texty a tak)
        public WinState State { get; private set; }

        public MainWindow() {
            InitializeComponent();
        }

        #region Helper methods
        /// <summary>
        /// Vlozi jeden radek textu do textblocku
        /// </summary>
        /// <param name="text">Radek ke vlozeni</param>
        public void AddTextLine( string text ) {
            messages.Text += text + "\n";
            Scroller.ScrollToBottom();
        }

        /// <summary>
        /// Nastavuje stav okna a nastavuje hodnotu State
        /// </summary>
        /// <param name="newState">Hodnota stavu k nastaveni</param>
        public void SetState( WinState newState ) {
            switch( newState ) {
                case WinState.Offline:
                    HostBtn.Content = "Host a server";
                    ConnectBtn.Content = "Connect to host";
                    SetStateAll( true );
                    SetState( true, "OFFLINE", Colors.Red );
                    break;
                case WinState.Connected:
                    ConnectBtn.Content = "Disconnect";
                    SetStateAll( true );
                    HostBtn.IsEnabled = false;
                    SetState( false, "CONNECTED", Colors.LawnGreen );
                    break;
                case WinState.Hosting:
                    HostBtn.Content = "Stop server";
                    SetStateAll( true );
                    ConnectBtn.IsEnabled = false;
                    SetState( false, "HOSTING", Colors.DarkCyan );
                    break;
                case WinState.InMiddle:
                    SetStateAll( false );
                    break;
            }
            State = newState;
        }

        /// <summary>
        /// Pomocna metoda pro nastaveni textu v connection a tlacitek
        /// </summary>
        /// <param name="textBoxes"></param>
        /// <param name="ConnectionText"></param>
        /// <param name="ConnectionForeground"></param>
        private void SetState( bool textBoxes, string ConnectionText, Color ConnectionForeground ) {
            ConnectionLbl.Foreground = new SolidColorBrush( ConnectionForeground );
            ConnectionLbl.Content = ConnectionText;
            IPTB.IsEnabled = textBoxes;
            UsernameTB.IsEnabled = textBoxes;
        }

        /// <summary>
        /// Pomocna metoda pro enable/disable vsech menitelnych prvku
        /// </summary>
        /// <param name="change"></param>
        private void SetStateAll(bool change) {
            HostBtn.IsEnabled = change;
            ConnectBtn.IsEnabled = change;
            IPTB.IsEnabled = change;
            UsernameTB.IsEnabled = change;
        }
        #endregion

        /// <summary>
        /// Klik na tlacitko zapnuti serveru. 
        /// Pokud jsme offline, tak se pokusi zalozit server a pripojit s clientem.
        /// Pokud hostujeme server, tak ho vypne.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HostBtn_Click( object sender, RoutedEventArgs e ) {
            if( State == WinState.Offline ) {
                SetState( WinState.InMiddle ); // Nez zacnu neco delat, tak vsechno zdisabluju
                server = new ChatServer( this );
                client = new ChatClient( this );
                if( server.Start() )
                    if( client.Connect( "127.0.0.1:4586", UsernameTB.Text ) ) // Pripojuji se na localhost, takze neresim textbox s IP
                        SetState( WinState.Hosting );
                    else { // Pokud se pripojeni clienta nepodarilo, tak vypnu server
                        server.Stop();
                        SetState( WinState.Offline );
                    }
                else // Pokud se vytvoreni serveru nepodarilo, tak se vratim do stavu offline
                    SetState( WinState.Offline );
            }
            else if( State == WinState.Hosting ) { // Kdyz hostuji, tak odpojim clienta a vypnu server
                client.Disconnect();
                server.Stop();
            }

            MessageTB.Focus();
        }

        /// <summary>
        /// Klik na tlacitko pripojeni k serveru.
        /// Pokud jsem offline, tak se skusim pripojit.
        /// Pokud jsem pripojen, tak se odpojim.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectBtn_Click( object sender, RoutedEventArgs e ) {
            if( State == WinState.Offline ) {
                SetState( WinState.InMiddle );
                client = new ChatClient( this );
                if( client.Connect( IPTB.Text, UsernameTB.Text ) )
                    SetState( WinState.InMiddle );
                else
                    SetState( WinState.Offline );
            }
            else if( State == WinState.Connected ) {
                client.Disconnect();
            }

            MessageTB.Focus();
        }

        /// <summary>
        /// Klick na tlacitko odeslani zpravy.
        /// Kdyz jsem offline nebo inMiddle, tak vypisu errory.
        /// Jinak se pokusim poslat zpravu clientem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendBtn_Click( object sender, RoutedEventArgs e ) {
            if(MessageTB.Text == "" ) {
                MessageTB.Focus();
                return;
            }

            switch( State ) {
                case WinState.Offline:
                    AddTextLine( "ERROR: Message wasn't sent because you are not connected or hosting." );
                    break;
                case WinState.InMiddle:
                    AddTextLine( "ERROR: Message wasn't sent because you are connecting." );
                    break;
                default:
                    client.SendMessage( MessageTB.Text );
                    break;
            }

            MessageTB.Text = "";
            MessageTB.Focus();
        }

        /// <summary>
        /// Automaticke smazani defaultni zpravy "Type a message here" pri focusu texboxu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageTB_GotFocus( object sender, RoutedEventArgs e ) {
            if( MessageTB.Text == "Type a message here" )
                MessageTB.Text = "";
        }

        /// <summary>
        /// Odeslani zpravy klavesou 'enter'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Enter )
                SendBtn_Click( sender, e );
        }
    }
}