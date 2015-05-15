using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFF_DU5_TasksMergeSort;

namespace Tests_TasksMergeSort
{
    [TestClass]
    public class MergeWrite
    {
        [TestMethod]
        public void MergeWrite_Empty_Empty() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { }, new double[] { } );
            string expected = "";
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_Empty_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { }, new double[] { 1d } );
            string expected = "1" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_One_Empty() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { 1d }, new double[] { } );
            string expected = "1" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_One_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { 1d }, new double[] { 1d } );
            string expected =   "1" + Environment.NewLine + 
                                "1" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_Many_One() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { 1d, 2d, 3d, 4d }, new double[] { 1d } );
            string expected =   "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "2" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_One_Many() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { 1d }, new double[] { 1d, 2d, 3d, 4d } );
            string expected =   "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "2" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestMethod]
        public void MergeWrite_Many_Many() {
            StringWriter writer = new StringWriter();
            ParallelMergeSort p = new ParallelMergeSort( writer );
            p.MergeWrite( new double[] { 1d, 2d, 3d, 4d }, new double[] { 1d, 2d, 3d, 4d } );
            string expected =   "1" + Environment.NewLine +
                                "1" + Environment.NewLine +
                                "2" + Environment.NewLine +
                                "2" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "3" + Environment.NewLine +
                                "4" + Environment.NewLine +
                                "4" + Environment.NewLine;
            Assert.AreEqual( expected, writer.ToString() );
        }

    }
}
