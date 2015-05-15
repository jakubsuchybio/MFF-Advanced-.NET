using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using MFF_DU5_TasksMergeSort;
using System.Threading.Tasks;
using System.Threading;

namespace Tests_TasksMergeSort
{
    [TestClass]
    public class AddBlock
    {
        [TestMethod]
        public void AddBlock_One_NoWrite() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            Task.WaitAll( p.tasks.ToArray() );
            double[] expected = new double[] { 1d, 3d, 4d, 5d };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }

        [TestMethod]
        public void AddBlock_Two_NoWrite() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            Task.WaitAll( p.tasks.ToArray() );
            double[] expected = new double[] { 1d, 1d, 3d, 3d, 4d, 4d, 5d, 5d };
            double[] actual;
            if( p.SortedBlocks.TryDequeue( out actual ) )
                CollectionAssert.AreEqual( expected, actual );
            else
                Assert.Fail( "Could not dequeue" );
        }

        [TestMethod]
        public void AddBlock_One_Write() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d }, true );
            string expected = "1" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "5" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void AddBlock_Two_Write() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d }, true );
            string expected = "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "5" + Environment.NewLine +
                                "5" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void AddBlock_Three_Write() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d } );
            p.AddBlock( new double[] { 1d, 5d, 3d, 4d }, true );
            string expected = "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "5" + Environment.NewLine +
                                "5" + Environment.NewLine +
                                "5" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void AddBlock_Five_Write() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.AddBlock( new double[] { 4d, 3d, 2d, 1d } );
            p.AddBlock( new double[] { 8d, 7d, 6d, 5d } );
            p.AddBlock( new double[] { 12d, 11d, 10d, 9d } );
            p.AddBlock( new double[] { 16d, 15d, 14d, 13d } );
            p.AddBlock( new double[] { 20d, 19d, 18d, 17d }, true );
            string expected = "1" + Environment.NewLine +
                                "2" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "5" + Environment.NewLine +
                                "6" + Environment.NewLine +
                                "7" + Environment.NewLine +
                                "8" + Environment.NewLine +
                                "9" + Environment.NewLine +
                                "10" + Environment.NewLine +
                                "11" + Environment.NewLine +
                                "12" + Environment.NewLine +
                                "13" + Environment.NewLine +
                                "14" + Environment.NewLine +
                                "15" + Environment.NewLine +
                                "16" + Environment.NewLine +
                                "17" + Environment.NewLine +
                                "18" + Environment.NewLine +
                                "19" + Environment.NewLine +
                                "20" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }
    }
}
