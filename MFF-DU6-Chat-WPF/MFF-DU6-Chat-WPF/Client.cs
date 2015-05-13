using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MFF_DU6_Chat_WPF
{


    /// <summary>
    /// Trida pro reprezentaci a praci s clientem v1.1
    /// </summary>
    public class ChatClient
    {
        private MainWindow window; //Pro aktualizaci prvku okna
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource pingCancelation; //Pro pingovani
        private string username;

        public ChatClient( MainWindow window ) {
            client = new TcpClient();
            pingCancelation = new CancellationTokenSource();
            this.window = window;
        }

       /*Algoritmus pripojeni clienta:
        * 1. Po stisku pripojeni se nastavi okno na disabled
        * 2. Provedou se kontroly a spravnost IP, portu a Jmena
        * 3. Pokusim se pripojit
        * 4. Kdyz se pripojim, tak navazu spojeni podle protokolu
        * 5. A vytvorim task pro zpracovani prichozich zprav ze serveru
        */
        
        /// <summary>
        /// Pripojeni clienta.
        /// Alg 1. a 2.
        /// </summary>
        /// <param name="inputIP">Vstupni host (ip:port) nebo jen ip</param>
        /// <param name="username">Jmeno bez mezer</param>
        /// <returns></returns>
        public bool Connect(string inputIP, string username) {
            this.username = username;
            int port;
            IPAddress host;
            if( inputIP.Contains( ":" ) ) {
                if( !int.TryParse( inputIP.Substring( inputIP.IndexOf( ':' ) + 1 ), out port ) ) {
                    window.AddTextLine( "ERROR Client: Wrong port." );
                    return false;
                }
                if( !IPAddress.TryParse( inputIP.Substring( 0, inputIP.IndexOf( ':' ) ), out host ) ) {
                    window.AddTextLine( "ERROR Client: Wrong host." );
                    return false;
                }

            }
            else {
                port = 4586;
                if( !IPAddress.TryParse( inputIP, out host ) ) {
                    window.AddTextLine( "ERROR Client: Wrong host." );
                    return false;
                }
            }

            if( username.Contains( " " ) ) {
                window.AddTextLine( "ERROR Client: Wrong username." );
                return false;
            }
            ConnectTask( username, host, port );
            return true;
        }

        /// <summary>
        /// Alg 4. a 5.
        /// </summary>
        /// <param name="username">Zkontrolovane jmeno bez mezer</param>
        /// <param name="host">Zkontrolovane IP</param>
        /// <param name="port">Zkontrolovany port</param>
        private async void ConnectTask(string username, IPAddress host, int port ) {
            try {
                await client.ConnectAsync( host, port );
                if( client == null || !client.Connected ) {
                    window.AddTextLine( "Error Client: Unable to connect." );
                    this.Disconnect();
                    return;
                }
                stream = client.GetStream();

                await InitializeConnection( username );

                if(window.State != WinState.Hosting)
                    window.SetState( WinState.Connected );

                Task.Run(()=> SendPing(), pingCancelation.Token );

                ReceiveMessages();
            }
            catch( SocketException ) {
                window.AddTextLine( "Error Client: Couldn't connect to server." );
                this.Disconnect();
                return;
            }
            catch( Exception e ) {
                System.Windows.MessageBox.Show( "Unknown Client error: " + e.Message );
                this.Disconnect();
                return;
            }
        }

        /// <summary>
        /// Nekonecny task pro posilani pingu po 60sec.
        /// Vytvari se s pingCancelation tokenem pro jeho zruseni pri Disconnectu
        /// </summary>
        private async void SendPing() {
            var ping = new Message( Msg.PING, DateTime.Now, 1 ).ToByteArray();
            await stream.WriteAsync( ping, 0, ping.Length );
            try {
                await Task.Delay( 60000 ).ContinueWith( ( x ) => SendPing(), pingCancelation.Token );
            }
            catch( TaskCanceledException ) {
            }
        }

        /// <summary>
        /// Alg 4.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task InitializeConnection( string username ) {
            byte[] buffer = new byte[ 4096 ];

            //SENDING HELLO
            var hello = new Message( Msg.HELLO, DateTime.Now, 1 );
            var helloBytes = hello.ToByteArray();
            await stream.WriteAsync( helloBytes, 0, helloBytes.Length );

            //WAITING FOR OLLEH
            int bufferSize = await stream.ReadAsync( buffer, 0, buffer.Length );
            var olleh = Message.ParseMsgFromBytes( buffer, bufferSize );
            if( olleh.MsgType != Msg.OLLEH )
                throw new Exception( "Got something else than OLLEH packet" );

            //ACCEPTED OLLEH, SENDING ACK
            var ackBytes = new Message( Msg.ACK, DateTime.Now, 1 ).ToByteArray();
            await stream.WriteAsync( ackBytes, 0, ackBytes.Length );

            //CONNECTED
        }

        /// <summary>
        /// Alg 5.
        /// </summary>
        private async void ReceiveMessages() {
            byte[] buffer = new byte[ 4096 ];
            try {
                while( true ) {
                    int bufferSize = await stream.ReadAsync( buffer, 0, buffer.Length );
                    if( bufferSize == 0)
                        throw new IOException();

                    var message = Message.ParseMsgFromBytes( buffer, bufferSize );
                    if( message.MsgType == Msg.MSG ) {
                        var splits = message.Body.Split( ' ' );
                        string username = splits[ 1 ];
                        string text = string.Join( " ", splits, 2, splits.Length - 2 );
                        window.AddTextLine( message.TimeStamp.ToString() + " " + username + ": " + text );
                    }
                }
            }
            catch( ObjectDisposedException ) {
                window.AddTextLine( "Error Client: ReceiveMessages() ObjectDisposedException" );
                Disconnect();
                return;
            }
            catch( IOException ) {
                window.AddTextLine( "Error Client: Server is down." );
                Disconnect();
                return;
            }
            catch( Exception e ) {
                System.Windows.MessageBox.Show( "Unknown Client error: " + e.Message );
                Disconnect();
                return;
            }
        }

        /// <summary>
        /// Metoda pro odesilani zprav typu "MSG" na server
        /// </summary>
        /// <param name="message"></param>
        public async void SendMessage( string message ) {
            if( !client.Connected ) {
                System.Windows.MessageBox.Show( "Cannot send messages (not connected)" );
                return;
            }

            var msgBytes = new Message( Msg.MSG, DateTime.Now, 1, message, username ).ToByteArray();
            
            await stream.WriteAsync( msgBytes, 0, msgBytes.Length );
        }

        /// <summary>
        /// Odpojeni clienta, zruseni pingovani
        /// </summary>
        public void Disconnect() {
            if( client == null )
                return;

            if(!pingCancelation.Token.IsCancellationRequested)
                pingCancelation.Cancel();

            client.Close();
            window.SetState( WinState.Offline );
        }
    }
}