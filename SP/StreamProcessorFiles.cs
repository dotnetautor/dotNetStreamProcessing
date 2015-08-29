using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SP
{

	public static class StreamProcessorFiles
	{

		#region public extension methods

		public static StreamProcessor<TGlobalMetaData> Src<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, params string[] searchList)
		{
			var fileNames = PrepareSearchPattern<TGlobalMetaData>(searchList);
			var tasks = fileNames.Select(fileName => SPStream.CreateSPStreamAsync(sp, fileName));
			sp.Streams.AddRange(Task.WhenAll(tasks).GetAwaiter().GetResult());
			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> Src<TGlobalMetaData, TLocalMetaData>(this StreamProcessor<TGlobalMetaData> sp, Func<string, TLocalMetaData> createMetaDataCallback, params string[] searchList)
		{
			if (createMetaDataCallback == null)
				createMetaDataCallback = fileName => default(TLocalMetaData);
			var fileNames = PrepareSearchPattern<TGlobalMetaData>(searchList);
			var tasks = fileNames.Select(fileName => SPStream<TLocalMetaData>.CreateSPStreamAsync(sp, fileName, createMetaDataCallback(fileName)));
			sp.Streams.AddRange(Task.WhenAll(tasks).GetAwaiter().GetResult());
			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> Dest<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, string pattern)
		{
			if (pattern == null)
				throw new ArgumentNullException("pattern", "Pattern is null.");

			var rootPath = StreamProcessor<TGlobalMetaData>.RootPath;

			var lastPartIndex = pattern.LastIndexOfAny(new[] { '\\', '/' });

			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> Concat<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, string newFileName)
		{
			if (newFileName == null)
				throw new ArgumentNullException("newFileName", "The cocatted stream needs a file name.");
			var spStreams = new List<SPStream>(sp.Streams);

			sp.Streams.Clear();
			sp.Streams.Add(SPStream.CreateSPStream(newFileName, spStreams.SelectMany(spStream => spStream.Buffer).ToArray()));

			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> Concat<TGlobalMetaData, TLocalMetaData>(this StreamProcessor<TGlobalMetaData> sp, string newFileName, Func<IEnumerable<SPStream>, TLocalMetaData> createMetaDataCallback)
		{
			if (createMetaDataCallback == null)
				createMetaDataCallback = spStreams => default(TLocalMetaData);
			return Concat(sp, newFileName, createMetaDataCallback(sp.Streams));
		}

		public static StreamProcessor<TGlobalMetaData> Concat<TGlobalMetaData, TLocalMetaData>(this StreamProcessor<TGlobalMetaData> sp, string newFileName, TLocalMetaData localMetaData)
		{
			if (newFileName == null)
				throw new ArgumentNullException("newFileName", "The cocatted stream needs a file name.");
			var spStreams = new List<SPStream>(sp.Streams);

			sp.Streams.Clear();
			sp.Streams.Add(SPStream<TLocalMetaData>.CreateSPStream(newFileName, spStreams.SelectMany(spStream => spStream.Buffer).ToArray(), localMetaData));

			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> DeleteFiles<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, Func<SPStream, bool> predicate = null)
		{
			if (predicate == null)
				predicate = spStream => true;

			// Enumerate it in reverse order to save remove stream from list.
			for (var i = sp.Streams.Count - 1; i >= 0; --i) {
				var spStream = sp.Streams[i];
				if(predicate(spStream)) {
					var fullPath = Path.Combine(StreamProcessor<TGlobalMetaData>.RootPath, spStream.SourceFileName);
					if (File.Exists(fullPath))
						File.Delete(fullPath);
					sp.Streams.RemoveAt(i);
				}
			}

			return sp;
		}

		public static StreamProcessor<TGlobalMetaData> DeleteStreams<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, Func<SPStream, bool> predicate = null)
		{
			if (predicate == null)
				predicate = spStream => true;

			// Enumerate it in reverse order to save remove stream from list.
			for (var i = sp.Streams.Count - 1; i >= 0; --i) {
				var spStream = sp.Streams[i];
				if (predicate(spStream)) {
					sp.Streams.RemoveAt(i);
				}
			}

			return sp;
		}

		#endregion

		#region private static methods

		private static IEnumerable<String> PrepareSearchPattern<TGlobalMetaData>(string[] searchList)
		{
			var includes = new List<string>();
			var excludes = new List<string>();
			foreach (var searchPattern in searchList) {
				if (searchPattern.Length > 0 && searchPattern[0] == '!') {
					// if searchpattern starts with "!" insert files to excludes.
					excludes.AddRange(GetFilesFromSearchpattern<TGlobalMetaData>(searchPattern.Substring(1)));
				} else {
					// insert files to includes.
					includes.AddRange(GetFilesFromSearchpattern<TGlobalMetaData>(searchPattern));
				}
			}
			return includes
				.Distinct(StringComparer.InvariantCultureIgnoreCase)
				.Except(excludes, StringComparer.InvariantCultureIgnoreCase);
		}

		private static IEnumerable<string> GetFilesFromSearchpattern<TGlobalMetaData>(string searchPattern)
		{
			if (searchPattern == null)
				throw new ArgumentNullException("searchPattern", "SearchPattern is null.");

			var rootPath = StreamProcessor<TGlobalMetaData>.RootPath;

			var lastPartIndex = searchPattern.LastIndexOfAny(new[] { '\\', '/' });
			var filePattern = lastPartIndex == -1 ? searchPattern : searchPattern.Substring(lastPartIndex + 1);
			if (filePattern.Length == 0)
				filePattern = "*.*";
			var folderPattern = lastPartIndex == -1 ? "" : searchPattern.Substring(0, lastPartIndex);
			var searchFolders = new List<DirectoryInfo> { new DirectoryInfo(rootPath) };
			var allSubfolders = false;

			if (folderPattern.Length > 0) {
				var folderParts = folderPattern.Split(new[] { '\\', '/' }, StringSplitOptions.None);
				foreach (var folderPart in folderParts) {
					if (folderPart == null || folderPart.Length == 0)
						throw new ArgumentException("Invalid searchpattern.", "searchPattern");

					if (allSubfolders) {
						// There are folder parts after a ** part. At the moment this is not supported.
						throw new ArgumentException("Invalid searchpattern.", "searchPattern");
					} else {
						var starstarIndex = folderPart.IndexOf("**", StringComparison.InvariantCulture);
						if (starstarIndex < 0) {
							// There is no "**" in the folder name.
							var oldSearchFolders = new List<DirectoryInfo>(searchFolders);
							searchFolders.Clear();
							foreach (var searchFolder in oldSearchFolders) {
								searchFolders.AddRange(searchFolder.EnumerateDirectories(folderPart, SearchOption.TopDirectoryOnly));
							}
						} else {
							// There is a "**" in the folder name.
							// ToDo: Enumerate other than "**" - for example "foo**" or "**baz".
							// At the moment the folder name must be "**"
							if (!folderPart.Equals("**", StringComparison.InvariantCulture))
								throw new ArgumentException("Invalid searchpattern.", "searchPattern");
							var oldSearchFolders = new List<DirectoryInfo>(searchFolders);
							foreach (var searchFolder in oldSearchFolders) {
								searchFolders.AddRange(searchFolder.EnumerateDirectories("*.*", SearchOption.AllDirectories));
							}
							allSubfolders = true;
						}
					}
				}
			}

            foreach (var searchFolder in searchFolders) {
				foreach (var fileInfo in searchFolder.EnumerateFiles(filePattern, SearchOption.TopDirectoryOnly)) {
					yield return fileInfo.FullName.Substring(rootPath.Length + 1);
				}
			}
		}

		#endregion

	}

}
