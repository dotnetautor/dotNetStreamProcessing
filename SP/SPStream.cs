using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP
{

	[DebuggerDisplay("SPStream: {SourceFileName} ({Buffer.Length} Bytes)")]
	public class SPStream
	{

		#region public properties

		public string SourceFileName { get; private set; }
		public string DestFileName { get; set; }
		public byte[] Buffer { get; set; }

		#endregion

		#region ctor

		protected SPStream(string sourceFileName, byte[] buffer)
		{
			SourceFileName = sourceFileName;
			Buffer = buffer;
		}

		#endregion

		#region factory methods

		public static SPStream CreateSPStream(string fileName, byte[] buffer)
		{
			return new SPStream(fileName, buffer);
		}

		public static async Task<SPStream> CreateSPStreamAsync<TGlobalMetaData>(StreamProcessor<TGlobalMetaData> sp, string fileName)
		{
			using(var fileStream = new FileStream(Path.Combine(StreamProcessor<TGlobalMetaData>.RootPath, fileName), FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true)) {
				var length = fileStream.Length;
				var buffer = new byte[length];
				await fileStream.ReadAsync(buffer, 0, buffer.Length);
				return new SPStream(fileName, buffer);
			}
		}

		#endregion

	}


	public class SPStream<TLocalMetaData>
		: SPStream
	{

		#region public properties

		public TLocalMetaData LocalMetaData { get; set; }

		#endregion

		#region ctor

		protected SPStream(string sourceFileName, byte[] buffer, TLocalMetaData localMetaData)
			: base(sourceFileName, buffer)
		{
			LocalMetaData = localMetaData;
		}

		#endregion

		#region factory methods

		public static SPStream<TLocalMetaData> CreateSPStream(string fileName, byte[] buffer, TLocalMetaData localMetaData)
		{
			return new SPStream<TLocalMetaData>(fileName, buffer, localMetaData);
		}

		public static async Task<SPStream<TLocalMetaData>> CreateSPStreamAsync<TGlobalMetaData>(StreamProcessor<TGlobalMetaData> sp, string fileName, TLocalMetaData localMetaData)
		{
            using (var fileStream = new FileStream(Path.Combine(StreamProcessor<TGlobalMetaData>.RootPath, fileName), FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true)) {
				var length = fileStream.Length;
				var buffer = new byte[length];
				await fileStream.ReadAsync(buffer, 0, buffer.Length);
				return new SPStream<TLocalMetaData>(fileName, buffer, localMetaData);
			}
		}

		#endregion

	}

}
