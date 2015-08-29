using System;
using System.Collections.Generic;

namespace SP
{

	public sealed class StreamProcessor<TGlobalMetaData>
	{

		#region public static properties

		public static string RootPath { get; private set; }

		#endregion

		#region public properties

		public TGlobalMetaData GlobalMetaData { get; set; }
		public List<SPStream> Streams { get; private set; }

		#endregion

		#region ctor

		static StreamProcessor()
		{
			RootPath = Environment.CurrentDirectory;
		}

		public StreamProcessor()
		{
			Streams = new List<SPStream>();
        }

		public StreamProcessor(TGlobalMetaData globalMetaData) : this()
		{
			GlobalMetaData = globalMetaData;
		}

		#endregion

	}

}
