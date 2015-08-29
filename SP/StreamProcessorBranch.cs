using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SP
{

	public static class StreamProcessorBranch
	{

		public sealed class BranchProcessor<TGlobalMetaData>
		{

			private StreamProcessor<TGlobalMetaData> sp;
			private Func<SPStream, string> branchCallback;
			private List<Task<StreamProcessor<TGlobalMetaData>>> asyncTasks;
			private List<StreamProcessor<TGlobalMetaData>> syncTasks;

			internal BranchProcessor(StreamProcessor<TGlobalMetaData> sp, Func<SPStream, string> branchCallback)
			{
				this.sp = sp;
				this.branchCallback = branchCallback;
				asyncTasks = new List<Task<StreamProcessor<TGlobalMetaData>>>();
				syncTasks = new List<StreamProcessor<TGlobalMetaData>>();
			}

			public BranchProcessor<TGlobalMetaData> When(string branchName, Func<StreamProcessor<TGlobalMetaData>, StreamProcessor<TGlobalMetaData>> branchFn, bool useAsync = true)
			{
				if (branchName == null)
					throw new ArgumentNullException("branchName", "The branch name must not null.");
				if (branchFn == null)
					branchFn = sp => sp;

				var streams = new List<SPStream>(sp.Streams);
				var processor = new StreamProcessor<TGlobalMetaData>(sp.GlobalMetaData);

				foreach (var stream in streams) {
					if(branchName.Equals(branchCallback(stream), StringComparison.InvariantCultureIgnoreCase)) {
						sp.Streams.Remove(stream);
						processor.Streams.Add(stream);
					}
				}

				if (useAsync)
					asyncTasks.Add(Task.Run(() => branchFn(processor)));
				else
					syncTasks.Add(branchFn.Invoke(processor));

				return this;
			}

			public BranchProcessor<TGlobalMetaData> Otherwise(Func<StreamProcessor<TGlobalMetaData>, StreamProcessor<TGlobalMetaData>> branchFn, bool useAsync = true)
			{
				if (branchFn == null)
					branchFn = sp => sp;

				var processor = new StreamProcessor<TGlobalMetaData>(sp.GlobalMetaData);
				processor.Streams.AddRange(sp.Streams);
				sp.Streams.Clear();

				if (useAsync)
					asyncTasks.Add(Task.Run(() => branchFn(processor)));
				else
					syncTasks.Add(branchFn.Invoke(processor));

				return this;
			}

			public StreamProcessor<TGlobalMetaData> Collect()
			{
				// Collect all synchron tasks and get their streams.
				sp.Streams.AddRange(syncTasks.SelectMany(sp => sp.Streams));
				// Collect all asynchron tasks and get their streams.
				sp.Streams.AddRange(Task.WhenAll(asyncTasks).GetAwaiter().GetResult().SelectMany(sp => sp.Streams));

				return sp;
			}

		}

        public static BranchProcessor<TGlobalMetaData> Branch<TGlobalMetaData>(this StreamProcessor<TGlobalMetaData> sp, Func<SPStream, string> branchCallback)
		{
			return new BranchProcessor<TGlobalMetaData>(sp, branchCallback);
		}

	}

}
