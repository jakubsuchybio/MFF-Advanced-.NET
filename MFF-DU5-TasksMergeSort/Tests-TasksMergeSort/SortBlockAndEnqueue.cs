using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using MFF_DU5_TasksMergeSort;

namespace Tests_TasksMergeSort
{
    [TestClass]
    public class SortBlockAndEnqueue
    {
        [TestMethod]
        public void SortBlockAndEnqueue_Many_UnSorted() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.SortBlockAndEnqueue( new double[] { 1d, 5d, 3d, 4d } );
            double[] expected = new double[] { 1d, 3d, 4d, 5d };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }

        [TestMethod]
        public void SortBlockAndEnqueue_Many_Sorted() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.SortBlockAndEnqueue( new double[] { 1d, 2d, 3d, 4d } );
            double[] expected = new double[] { 1d, 2d, 3d, 4d };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }

        [TestMethod]
        public void SortBlockAndEnqueue_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.SortBlockAndEnqueue( new double[] { 1d } );
            double[] expected = new double[] { 1d };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }

        [TestMethod]
        public void SortBlockAndEnqueue_Empty() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.SortBlockAndEnqueue( new double[] { } );
            double[] expected = new double[] { };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }
    }
}
