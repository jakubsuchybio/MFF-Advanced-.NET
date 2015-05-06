using System;
using System.Collections.Generic;
using System.Text;

namespace MFF_DU6_Chat_WPF
{
    /// <summary>
    /// Vycet vsech moznych typu zprav mezi clientem a serverem
    /// </summary>
    public enum Msg
    {
        HELLO,
        ACK,
        ERROR,
        MSG,
        OLLEH,
        PING,
        PONG
    }
    
    /// <summary>
    /// Pomocna trida pro konverzi Msg -> String a zpet
    /// </summary>
    public static class MsgConvertor
    {
        private static Dictionary<string, Msg> strings;
        private static Dictionary<Msg, string> messages;

        static MsgConvertor() {
            strings = new Dictionary<string, Msg>( 7 );
            messages = new Dictionary<Msg, string>( 7 );
            foreach( Msg msg in Enum.GetValues( typeof(Msg) ) ) {
                var str = msg.ToString();
                strings.Add( str, msg );
                messages.Add( msg, str );
            }
        }

        public static string MsgToString( Msg msg ) {
            return messages[ msg ];
        }


        public static Msg StringToMsg( string str ) {
            Msg result;
            return strings.TryGetValue( str, out result ) ? result : Msg.ERROR; //Pokud se konverze nezdarila, tak vratim ERROR typ
        }
    }

    /// <summary>
    /// Vnitrni reprezentace zprav mezi clientem a serverem
    /// </summary>
    public class Message
    {
        //Slouzi pro regist znamych zprav
        private static Dictionary<Msg, string> KnownMessages;

        //Registrace znamych zprav
        static Message() {
            KnownMessages = new Dictionary<Msg, string>( 8 );
            KnownMessages.Add( Msg.HELLO, "HELLO NPRG038CHAT" );
            KnownMessages.Add( Msg.ACK, "ACK" );
            KnownMessages.Add( Msg.ERROR, "ERROR" );
            KnownMessages.Add( Msg.MSG, "MSG" );
            KnownMessages.Add( Msg.OLLEH, "OLLEH NPRG038CHAT" );
            KnownMessages.Add( Msg.PING, "PING" );
            KnownMessages.Add( Msg.PONG, "PONG" );
        }

        /// <summary>
        /// Metoda pro registraci dalsich custom zprav
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="strRepresentation"></param>
        /// <returns></returns>
        public static bool RegisterNewMessage(Msg msg, string strRepresentation ) {
            if( KnownMessages.ContainsKey( msg ) )
                return false;
            else
                KnownMessages.Add( msg, strRepresentation );

            return true;
        }

        public Msg MsgType { get; set; } //Typ zpravy
        public string Body { get; set; } //Cela string reprezentace zpravy
        public DateTime TimeStamp { get; set; } //Cas vytvoreni zpravy

        /// <summary>
        /// Konstruktor zpravy z typu, casu, verze, zpravy a jmena
        /// </summary>
        /// <param name="msgType">Typ</param>
        /// <param name="timeStamp">Cas</param>
        /// <param name="version">Verze</param>
        /// <param name="message">Telo zpravy</param>
        /// <param name="username">Jmeno</param>
        public Message(Msg msgType, DateTime timeStamp, int version = 1, string message="", string username = "" ) {
            TimeStamp = timeStamp;
            if( version == 0 && (msgType == Msg.PING || msgType == Msg.PONG) )
                MsgType = Msg.ERROR;
            else
                MsgType = msgType;
            Body = KnownMessages[ MsgType ];

            switch( MsgType ) {
                case Msg.HELLO:
                    switch( version ) {
                        case 1:
                            Body += " 1.0 1.1"; break;
                        default:
                            Body += " 1.0"; break;
                    }
                    break;
                case Msg.OLLEH:
                    switch( version ) {
                        case 1:
                            Body += " 1.1"; break;
                        default:
                            Body += " 1.0"; break;
                    }
                    break;
                case Msg.MSG:
                    Body += " " + username + " " + message; break;
                case Msg.ERROR:
                    Body += " " + username + " " + message; break;
            }
        }

        /// <summary>
        /// Naparsuje string zpravy a vytvori z nej Message
        /// </summary>
        /// <param name="msg">Vstupni string zpravy</param>
        /// <param name="version">Cilena verze</param>
        /// <returns>Vraci Message reprezentaci vstupniho stringu zpravy</returns>
        public static Message ParseMsg(string msg, int version = 1 ) {
            var splits = msg.Trim().Split( ' ' );
            Msg msgType = MsgConvertor.StringToMsg( splits[ 0 ] );
            switch( msgType ) {
                case Msg.HELLO:
                    if(splits.Length >= 3 && splits[ 1 ] == "NPRG038CHAT" )
                        return new Message( msgType, DateTime.Now, version );
                    else
                        return new Message( Msg.ERROR, DateTime.Now, version );
                case Msg.OLLEH:
                    if( splits.Length == 3 && splits[ 1 ] == "NPRG038CHAT" )
                        return new Message( msgType, DateTime.Now, splits[2] == "1.0" ? 0 : 1 );
                    else
                        return new Message( Msg.ERROR, DateTime.Now, version );
                case Msg.ERROR:
                    if( splits.Length >= 2 )
                        return new Message( Msg.ERROR, DateTime.Now, version, string.Join( " ", splits ));
                    else
                        return new Message( Msg.ERROR, DateTime.Now, version );
                case Msg.MSG:
                    if( splits.Length < 3 )
                        return new Message( Msg.ERROR, DateTime.Now, version );
                    else
                        return new Message( msgType, DateTime.Now, version, string.Join( " ", splits, 2, splits.Length - 2 ), splits[ 1 ] );
                default:
                    return new Message( msgType, DateTime.Now, version );
            }
        }

        /// <summary>
        /// Naparsuje pole bytu "byte[]" a vytvori z nej Message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgLenght"></param>
        /// <returns></returns>
        public static Message ParseMsgFromBytes( byte[] msg, int msgLength ) {
            //Toto kopirovani se provadi, protoze pole 'msg' muze byt delsi nez je aktualni zprava, ktera je danou delkou 'msgLength'
            byte[] tmp = new byte[ msgLength ];
            Array.Copy( msg, tmp, msgLength );
            return ParseMsg( BinaryStringConverter.BinaryToString( tmp ) );
        }

        /// <summary>
        /// Zkonvertuje aktualni Message do pole bytu 'byte[]'
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() {
            return BinaryStringConverter.StringToBinary( Body );
        }
    }
    
    /// <summary>
    /// Pomocna tridicka na konvertovani stringu do pole bytu a zpet pres kodovani UTF8
    /// </summary>
    static class BinaryStringConverter
    {
        static Func<byte[], char[]> defaultDec = Encoding.UTF8.GetChars;
        static Func<string, byte[]> defaultEnc = Encoding.UTF8.GetBytes;

        public static string BinaryToString( byte[] data, Func<byte[], char[]> encodingFunct = null ) {
            char[] m;
            if( encodingFunct == null ) {
                m = defaultDec( data );
            }
            else {
                m = encodingFunct( data );
            }
            return new string( m );
        }
        public static byte[] StringToBinary( string data, Func<string, byte[]> encodingFunct = null ) {
            if( encodingFunct == null ) {
                return defaultEnc( data );
            }
            else {
                return encodingFunct( data );
            }
        }
    }
}
