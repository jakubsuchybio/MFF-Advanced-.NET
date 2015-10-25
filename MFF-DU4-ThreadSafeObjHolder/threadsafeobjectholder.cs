using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

class ThreadSafeObjectHolder {
    private object internalObj;
    private List<object> otherObjs;

    public ThreadSafeObjectHolder() {
        // Volatile inicializace (pro internalObj je to mozna az moc velke kladivo na mravence)
        Interlocked.Exchange( ref internalObj, null );
        Interlocked.Exchange( ref otherObjs, new List<object>() );
    }

    public void AddObject(object obj) {
        // Zkusim, zda je internalObj == null a kdyz jo, tak rovnou priradim obj,
        // jinak provedu vnitrek ifu a pridam obj do otherObjs
        if( Interlocked.CompareExchange( ref internalObj, obj, null ) != null ) {
            while( true ) {
                // Volatile cteni
                var original = Interlocked.CompareExchange( ref otherObjs, null, null );
                // Vytvoreni kopie a pridani objektu
                var copy = new List<object>(original.ToArray());
                copy.Add( obj );
                // Pokus o zamenu kolekce s kopii, pokud se kolekce nezmenila od originalu
                var result = Interlocked.CompareExchange( ref otherObjs, copy, original );
                // Kdyz se original nezmenil vuci resultu pred predchozim prikazem, 
                // tzn zadne jine vlanko nepristoupilo k resultu(neboli otherObjs) 
                // a zapis nove copie probehl v poradku, tak ukoncime cyklus
                if( result == original )
                    break;
            }
        }
    }

    public object GetFirstObject() {
        // Volatile cteni
        var result = Interlocked.CompareExchange( ref internalObj, null, null );
        return result;
    }
}

class Program {
    static void Main(string[] args) {

    }
}