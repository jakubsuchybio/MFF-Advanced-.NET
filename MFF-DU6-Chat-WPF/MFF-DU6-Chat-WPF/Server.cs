using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MFF_DU6_Chat_WPF
{
    /// <summary>
    /// Trida pro reprezentaci a praci se serverem (podpora verzi 1.0 a 1.1)
    /// </summary>
    public class ChatServer
    {
        private MainWindow window; //Pro aktualizaci prvku okna
        private TcpListener ChatListener; //Pro vytvoreni listenera serveru
        private List<TcpClient> Clients; //Seznam pripojenych clientu

        /// <summary>
        /// Konstruktor, vytvari instanci listenera na ip localhostu
        /// </summary>
        /// <param name="window">Okno spusteni</param>
        public ChatServer( MainWindow window ) {
            Clients = new List<TcpClient>();
            this.window = window;

            IPEndPoint localAddr = new IPEndPoint(IPAddress.Parse( "127.0.0.1" ), 4586);
            ChatListener = new TcpListener( localAddr );
        }

        /* Algoritmus vytvoreni serveru
         * 1. se zapne listener na localhostu na portu 4586 
         * 2. vytvori se task pro prijem clientu
         * 3. pokud se pripoji nejaky client, tak se provede handshake
         * 4. pokud handshake probehl v poradku a vratil verzi pro spojeni s clientem, tak se vytvori task pro obsluhu zprav od clienta
         */

        /// <summary>
        /// Nastartuje server
        /// Alg 1.
        /// </summary>
        /// <returns></returns>
        public bool Start() {
            try {
                ChatListener.Start();
            }
            catch( SocketException ) {
                window.AddTextLine( "ERROR Server: Another is already running." );
                return false;
            }

            StartAccepting();
            return true;
        }

        /// <summary>
        /// Vypne server a odpoji pripojene clienty
        /// </summary>
        public void Stop() {
            if( Clients == null )
                return;
            ChatListener.Stop();
            foreach( var client in Clients ) {
                client.Close();
            }
            Clients = null;
            window.SetState( WinState.Offline );
        }

        /// <summary>
        /// Task pro prijem clientu
        /// Alg 2.
        /// </summary>
        private async void StartAccepting() {
            while( true ) {
                try {
                    TcpClient client = await ChatListener.AcceptTcpClientAsync();
                    if( Clients.Contains( client ) )
                        return;

                    Clients.Add( client );
                    var networkStream = client.GetStream();
                    int version = await SendHandshake( client );
                    HandleMessages( client, version );
                }
                catch( ObjectDisposedException ) {
                    window.AddTextLine( "Error Server: StartAccepting() ObjectDisposedException" );
                    this.Stop();
                    return;
                }
                catch( InvalidOperationException ) {
                    window.AddTextLine( "Error Server: StartAccepting() InvalidOperationException" );
                    this.Stop();
                    return;
                }
                catch( Exception e ) {
                    System.Windows.MessageBox.Show( "Unknow Server error: " + e.Message );
                    this.Stop();
                    return;
                }
            }
        }

        /// <summary>
        /// Alg 3.
        /// </summary>
        /// <param name="client">Instance clienta</param>
        /// <returns></returns>
        private async Task<int> SendHandshake( TcpClient client ) {
            var networkStream = client.GetStream();

            byte[] buffer = new byte[ 4096 ];

            //WAITING FOR HELLO
            int bufferSize = await networkStream.ReadAsync( buffer, 0, buffer.Length );
            var helloMsg = Message.ParseMsgFromBytes( buffer, bufferSize );
            if(helloMsg.MsgType != Msg.HELLO ) //Pokud prislo neco jinyho nez hello, tak odpojim clienta (zpatny pro DDOS)
                DisconnectClient( client );
            
            string[] bodySplits = helloMsg.Body.Split( ' ' );
            string latestVersion = bodySplits[ bodySplits.Length - 1 ];
            int intVersion = latestVersion == "1.1" ? 1 : 0;

            //ACCEPTED HELLO, SENDING OLLEH
            var ollehBytes = new Message( Msg.OLLEH, DateTime.Now, intVersion ).ToByteArray();
            await networkStream.WriteAsync( ollehBytes, 0, ollehBytes.Length );

            //OLLEH SENT, WAITING FOR ACK
            bufferSize = await networkStream.ReadAsync( buffer, 0, buffer.Length );
            var ackMsg = Message.ParseMsgFromBytes( buffer, bufferSize );
            if( ackMsg.MsgType != Msg.ACK ) //Pokud prislo neco jinyho nez ack, tak odpojim clienta (zpatny protokol)
                DisconnectClient( client );
            
            //ACCEPTED ACK
            return intVersion;
        }
        
        /// <summary>
        /// Alg 4.
        /// </summary>
        /// <param name="client">Instance clienta</param>
        /// <param name="version">Domluvena verze</param>
        private async void HandleMessages( TcpClient client, int version ) {
            var networkStream = client.GetStream();
            var cancelClient = new CancellationTokenSource();
            try {
                while( true ) {
                    //Vytvoreni tasku pro odpojeni clienta, pokud 180sec neposle ping
                    if( version > 0 )
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run( () => Task.Delay( 180000, cancelClient.Token ).
                            ContinueWith( ( x ) => DisconnectClient( client ), cancelClient.Token ) );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    //Task pro prijmuti zpravy od clienta 
                    byte[] buffer = new byte[ 4096 ];
                    int bufferSize = await networkStream.ReadAsync( buffer, 0, buffer.Length );
                    Message msg = Message.ParseMsgFromBytes( buffer, bufferSize );

                    //Prisla zprava typu Msg, tak ji preposlu vsem clientum
                    if( msg.MsgType == Msg.MSG ) {
                        byte[] toBroadcast = msg.ToByteArray();
                        foreach( var other in Clients ) {
                            if(other.Connected)
                                await other.GetStream().WriteAsync( toBroadcast, 0, toBroadcast.Length );
                        }
                    }

                    //Prisla zprava typu Ping, takze zrusim predchozi task na pingovani a vytvorim novy pro dalsich 180sec 
                    //a jeste odeslu clientovi pong
                    if(version > 0 && msg.MsgType == Msg.PING ) {
                        cancelClient.Cancel();
                        cancelClient = new CancellationTokenSource();
                        var pong = new Message( Msg.PONG, DateTime.Now, 1 ).ToByteArray();
                        await networkStream.WriteAsync( pong, 0, pong.Length );
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run( () => Task.Delay( 180000, cancelClient.Token ).
                            ContinueWith( ( x ) => DisconnectClient( client ), cancelClient.Token ) );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
            catch( ObjectDisposedException ) {
                window.AddTextLine( "Error Server: HandleMessages() ObjectDisposedException" );
                this.Stop();
                return;
            }
            catch( System.IO.IOException ) {
                window.AddTextLine( "Error Server: HandleMessages() IOException" );
                this.Stop();
                return;
            }
            catch( Exception e ) {
                System.Windows.MessageBox.Show( "Unknown Server error: " + e.Message );
                this.Stop();
                return;
            }
        }

        private void DisconnectClient(TcpClient client ) {
            if( Clients == null )
                return;

            client.Close();
            Clients.Remove( client );
        }
    }
}