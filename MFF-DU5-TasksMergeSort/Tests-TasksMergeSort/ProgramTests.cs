using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFF_DU5_TasksMergeSort;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests_TasksMergeSort
{
	[TestClass]
	public class ProgramTests
	{
		public void Assert_Stream_File_Are_Equals(TextWriter writer, string expectedFile)
		{
			string tempFileName = System.IO.Path.GetTempFileName();
			File.WriteAllBytes( tempFileName, Encoding.UTF8.GetBytes( writer.ToString() ) );

			BinaryReader expected = new BinaryReader( File.OpenRead( expectedFile ) );
			BinaryReader actual = new BinaryReader( File.OpenRead( tempFileName ) );

			//Assert.AreEqual(expected.BaseStream.Length, actual.BaseStream.Length);
			while( expected.BaseStream.Length == expected.BaseStream.Position || actual.BaseStream.Length == actual.BaseStream.Position )
			{
				Assert.AreEqual( expected.ReadByte(), actual.ReadByte() );
			}
			expected.Close();
			actual.Close();

			File.Delete( tempFileName );
		}

		[TestMethod]
		public void Program_Run()
		{
			StringWriter writer = new StringWriter();
			Program.Run( new string[] { }, new StreamReader( "input.txt" ), writer );
			Assert_Stream_File_Are_Equals( writer, "expected_output.txt" );
		}

		[TestMethod]
		public void EnsureNoAsyncVoidTests()
		{
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( Program ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( ParallelMergeSort ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( ProgramTests ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( MergeWrite ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( Merge ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( AddBlock ).Assembly );
			AssertExtensions.AssertNoAsyncVoidMethods( typeof( SortBlockAndEnqueue ).Assembly );
		}
	}
}
