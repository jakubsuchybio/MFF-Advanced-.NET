using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFF_DU5_TasksMergeSort;

namespace Tests_TasksMergeSort
{
    [TestClass]
    public class Merge
    {
        [TestMethod]
        public void Merge_Empty_Empty() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { }, new double[] { } );
            double[] expected = new double[] { };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_Empty_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { }, new double[] { 1d } );
            double[] expected = new double[] { 1d };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_One_Empty() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { 1d }, new double[] { } );
            double[] expected = new double[] { 1d };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_One_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { 1d }, new double[] { 1d } );
            double[] expected = new double[] { 1d, 1d };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_Many_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { 1d, 2d, 3d, 4d }, new double[] { 1d } );
            double[] expected = new double[] { 1d, 1d, 2d, 3d, 4d };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_One_Many() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { 1d }, new double[] { 1d, 2d, 3d, 4d } );
            double[] expected = new double[] { 1d, 1d, 2d, 3d, 4d };
            CollectionAssert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Merge_Many_Many() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            double[] actual = p.Merge( new double[] { 1d, 2d, 3d, 4d }, new double[] { 1d, 2d, 3d, 4d } );
            double[] expected = new double[] { 1d, 1d, 2d, 2d, 3d, 3d, 4d, 4d };
            CollectionAssert.AreEqual( expected, actual );
        }
    }
}
