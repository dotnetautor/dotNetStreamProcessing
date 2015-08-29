using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SP.Test.Files
{

	[TestClass]
	[DeploymentItem("TestProject")]
	public class StreamProcessorFiles_Src_Tests
	{

		#region Additional class to hold metadata for Tests with generic methods

		private class FileMetadata
		{
			public string AdditionalInfo1;
			public int AdditionalInfo2;
		}

		#endregion

		#region Test fields and properties

		/// <summary>The stream processor.</summary>
		private StreamProcessor<dynamic> sp;

		/// <summary>Gets or sets the test context.</summary>
		/// <value>The test context.</value>
		public TestContext TestContext { get; set; }

		#endregion

		#region Test initializing method

		/// <summary>Setups the test method environment.</summary>
		[TestInitialize()]
		public void SetupTestMethodEnvironment()
		{
			sp = new StreamProcessor<dynamic>();
		}

		#endregion

		#region Test procedures

		/// <summary>Tests  Src  urces the name of the test with simple file.</summary>
		[TestMethod]
		public void SrcTestWithSimpleFileName()
		{
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files before the test procedure has been started.");
			Assert.AreEqual(0, sp.Streams.Count, "The list of files already contains files before the test procedure has been started.");

			var fluentStreamProcessorInstance = sp.Src("/index.html");

			Assert.IsNotNull(fluentStreamProcessorInstance);
			Assert.AreSame(sp, fluentStreamProcessorInstance, "The return value of Src is not the same stream processor instance as the original stream processor instance.");
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files after the test procedure is completed.");
			Assert.AreEqual(1, sp.Streams.Count, "The list of files does not contain the expected amount of files.");
			Assert.AreEqual("index.html", sp.Streams[0].SourceFileName, "This is not the expected file name.");
		}

		[TestMethod]
		public void SrcTestWithSingleStarFileName()
		{
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files before the test procedure has been started.");
			Assert.AreEqual(0, sp.Streams.Count, "The list of files already contains files before the test procedure has been started.");

			var fluentStreamProcessorInstance = sp.Src("*.html");

			Assert.IsNotNull(fluentStreamProcessorInstance);
			Assert.AreSame(sp, fluentStreamProcessorInstance, "The return value of Src is not the same stream processor instance as the original stream processor instance.");
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files after the test procedure is completed.");
			Assert.AreEqual(1, sp.Streams.Count, "The list of files does not contain the expected amount of files.");
			Assert.AreEqual("index.html", sp.Streams[0].SourceFileName, "This is not the expected file name.");
		}

		[TestMethod]
		public void SrcTestWithGenericMetadataType()
		{
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files before the test procedure has been started.");
			Assert.AreEqual(0, sp.Streams.Count, "The list of files already contains files before the test procedure has been started.");

			var fluentStreamProcessorInstance = sp.Src(filename => new FileMetadata { AdditionalInfo1 = filename, AdditionalInfo2 = 42 }, "index.html");

			Assert.IsNotNull(fluentStreamProcessorInstance);
			Assert.AreSame(sp, fluentStreamProcessorInstance, "The return value of Src is not the same stream processor instance as the original stream processor instance.");
			Assert.IsNotNull(sp.Streams, "There is no instance of the list of files after the test procedure is completed.");
			Assert.AreEqual(1, sp.Streams.Count, "The list of files does not contain the expected amount of files.");
			var file = sp.Streams[0];
			Assert.IsInstanceOfType(file, typeof(SPStream<FileMetadata>));
		}

		#endregion

	}

}
