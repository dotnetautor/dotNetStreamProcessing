using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP
{

	static class Program
	{

		#region private types

		private class FileMetaData
		{
			public string FileType { get; private set; }
			public FileMetaData(string fileType) { FileType = fileType; }
		}

		#endregion

		#region Main

		static void Main(string[] args)
		{
			var sp = new StreamProcessor<dynamic>();

			sp.Src(file => new FileMetaData("css"), "css/app*/*.css")
				.Src(file => new FileMetaData("js"), "js/**/*.js", "!js/Sub*/dieseNicht.js")
				.Branch(file => ((SPStream<FileMetaData>)file).LocalMetaData.FileType)
				.When("css", cssProcessor =>
					cssProcessor

				).When("js", jsProcessor =>
					jsProcessor
					.Concat("app.js", spStreams => new FileMetaData("js"))

				).Otherwise(processor =>
					processor

				).Collect();

			foreach(var stream in sp.Streams) {
				Console.WriteLine(stream.SourceFileName);
			}

		}

		#endregion

	}

}
