using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MFF_DU5_TasksMergeSort {
    /*
     * Algoritmus:
     * 1. V hl. vlakne ctu po radcich soubor a plnim si docasny list<double> do velikost 128(BlockSize)
     * 2. Po naplneni listu nebo narazeni na konec souboru pridavam list do tridy ParallelMergeSort
     * 3. V pridani listu do ParallelMergeSort si vytvarim Task, ktery zkopiruje, setridi a ulozi list do thread-safe queue
     * 4. Po skonceni tasku ze 3. se vytvori ContinueWith Task, ktery kontroluje zda queue ma >= 2 prvky (setridena pole)
     * 4.1. Pokud ma, tak se vytvori Task, ktery vybere 2 pole a zmerguje je.
     * 4.2. ad 2. Pri pridavani preposilam stav, jestli je blok posledni, pokud ano, tak pred zmergovanim delam kontrolu,
     * zda byl zpracovan posledni blok a zda jsou v queue 2 zaznamy. Pokud ano, tak provedu zaverecny MergeWrite
     */
    class Program {
        private const string ArgumentError = "Argument Error";
        private const string FormatError = "Format Error";
        private const string IOError = "IO Error";
        private const string UnknownError = "Unknown Error";

        private const int BlockSize = 128;

        static void Main( string[] args ) {
            Run( args );
        }

        /// <summary>
        /// Pomocna metoda pro ucely testovani
        /// </summary>
        /// <param name="args"></param>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        public static void Run( string[] args, TextReader reader = null, TextWriter writer = null) {
            try {
                if( args.Length == 0 && ( reader == null || writer == null ) )
                    throw new ArgumentException();
                else if( args.Length != 2 )
                    throw new ArgumentException();
                else {
                    reader = new StreamReader( args[ 0 ] );
                    writer = new StreamWriter( args[ 1 ] );
                }
            }
            catch( ArgumentException ) {
                Console.WriteLine( ArgumentError );
            }
            catch( IOException ) {
                Console.WriteLine( IOError );
            }
            catch( Exception ) {
                Console.WriteLine( UnknownError );
            }

            // Invariant: Arguments OK
            try {
                ProcessFile( reader, writer );
            }
            catch( FormatException ) {
                Console.WriteLine( FormatError );
            }
            catch( Exception ) {
                Console.WriteLine( UnknownError );
            }
        }

        /// <summary>
        /// Po kontrole vsech argumentu nastava zpracovani vstupu
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        private static void ProcessFile( TextReader reader, TextWriter writer ) {
            string line;
            List<double> dataBlock = new List<double>(BlockSize);
            ParallelMergeSort ps = new ParallelMergeSort(writer);

            while( ( line = reader.ReadLine() ) != null ) {
                double value;
                if( !double.TryParse( line, out value ) )
                    throw new FormatException();

                dataBlock.Add( value );

                if( reader.Peek() == -1 ) {
                    ps.AddBlock( dataBlock.ToArray(), true );
                    dataBlock.Clear();
                }
                else if( dataBlock.Count == BlockSize ) {
                    ps.AddBlock( dataBlock.ToArray() );
                    dataBlock.Clear();
                }
            }
            Task.WaitAll();
        }
    }

    class ParallelMergeSort {
        //Fronta setridenych bloku
        ConcurrentQueue<double[]> SortedBlocks;
        //Pro ucely zaverecneho vypisu
        TextWriter writer;

        public ParallelMergeSort(TextWriter writer) {
            SortedBlocks = new ConcurrentQueue<double[]>();
            this.writer = writer;
        }

        /// <summary>
        /// Vytvori task, ktery sesorti a prida vstupni dataBlock do fronty a hned po skonceni
        /// </summary>
        /// <param name="dataBlock">Vstupni dataBlock</param>
        /// <param name="lastBlock">Pri poslednim bloku se pocka na dokonceni predchozich tasku</param>
        public void AddBlock( double[] dataBlock, bool lastBlock = false) {
            if( lastBlock )
                Task.WaitAll();
            var sortBlock = Task.Run( () => SortBlockAndEnqueue( dataBlock ) );
            sortBlock.ContinueWith( ( t ) => CheckToMerge(lastBlock) );
        }

        /// <summary>
        /// Zkopiruje, sesorti a ulozi dataBlock
        /// </summary>
        /// <param name="dataBlock"></param>
        private void SortBlockAndEnqueue( double[] dataBlock ) {
            double[] tmp = new double[ dataBlock.Length ];
            Array.Copy( dataBlock, tmp, dataBlock.Length );
            Array.Sort( tmp );
            SortedBlocks.Enqueue( tmp );
        }

        /// <summary>
        /// Kontroluje, zda jsou aspon 2 pole k mergnuti.
        /// Pokud ano, tak se je snazi z fronty odebrat:
        /// Pri spatnem odebrani jednoho se eventuelni dobre odebrany vlozi zpet
        /// Pri spravnem odebrani obou se vytvori task pro Mergnuti obou poli a hned po jeho skonceni
        /// se za nim vytvori hned dalsi Task pro dalsi CheckToMerge (kdyby naaahodou bylo neco dalsiho k mergovani)
        /// Pri poslednim bloku mame z AddBlock zaruceno dobehnuti vsech tasku a ve fronte jsou 2 pole, ktera se
        /// poslou do MergeWritu k mergnuti a zapsani
        /// </summary>
        /// <param name="lastBlock"></param>
        /// <remarks>INVARIANT: lastBlock == true -> ve fronte jsou posledni 2 setridena pole</remarks>
        private void CheckToMerge( bool lastBlock ) {
            if( SortedBlocks.Count >= 2 ) {
                double[] A,B;
                bool isA = SortedBlocks.TryDequeue( out A );
                bool isB = SortedBlocks.TryDequeue( out B );
                if( isA && isB ) {
                    if( lastBlock && SortedBlocks.IsEmpty ) {
                        MergeWrite( A, B );
                    }
                    else {
                        var mergeBlocks = Task.Run<double[]>( () => { return Merge( A, B ); } );
                        var enqueueResult = mergeBlocks.ContinueWith( ( result ) => SortedBlocks.Enqueue( result.Result ) );
                        enqueueResult.ContinueWith( ( t ) => CheckToMerge( lastBlock ) );
                    }
                }
                else if( isA )
                    SortedBlocks.Enqueue( A );
                else if( isB )
                    SortedBlocks.Enqueue( B );
            }
        }
        
        /// <summary>
        /// Jednoduchy merge 2 poli
        /// </summary>
        /// <param name="A">Prvni vstupni pole</param>
        /// <param name="B">Druhe vstupni pole</param>
        /// <returns>Mergnute vysledne pole</returns>
        private double[] Merge( double[] A, double[] B ) {
            double[] res = new double[ A.Length + B.Length ];
            int resPos = 0;
            int APos = 0, BPos = 0;

            while( resPos != A.Length + B.Length ) {
                if( A[ APos ] <= B[ BPos ] )
                    res[ resPos++ ] = A[ APos++ ];
                else
                    res[ resPos++ ] = B[ BPos++ ];
            }

            return res;
        }

        /// <summary>
        /// Jednoduchy merge 2 poli s primym zapisem do vystupniho souboru
        /// </summary>
        /// <param name="A">Prvni vstupni pole</param>
        /// <param name="B">Druhe vstupni pole</param>
        private void MergeWrite( double[] A, double[] B ) {
            int APos = 0, BPos = 0;
            while( APos != A.Length - 1 && BPos != B.Length - 1) {
                if( A[ APos ] <= B[ BPos ] )
                    writer.WriteLine( A[ APos++ ] );
                else
                    writer.WriteLine( B[ BPos++ ] );
            }
        }
    }
}
