using System;
using System.Collections.Generic;
using System.Linq;
using CachingEngine.Locking;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Progress;

namespace CachingEngine.PipelineExecution
{
    public class RoundRobinPipelineExecution : IMultiPipelineEngineExecutionStrategy
    {
        public IEngineLockProvider EngineLockProvider { get; set; }

        public void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken, IDataLoadEventListener listener)
        {
            // Execute one pass through a pipeline before moving to the next. Continue until completion.
            var engineList = engines.ToList();
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Round robin executor has " + engineList.Count + " pipeline(s) to run."));

            var allComplete = false;
            while (!allComplete)
            {
                allComplete = true;
                foreach (var engine in engineList)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (EngineLockProvider.IsLocked(engine))
                        continue;

                    // assigned to temporary variable here to make the logic a bit more explicit
                    EngineLockProvider.Lock(engine);
                    try
                    {
                        var hasMoreData = engine.ExecuteSinglePass(cancellationToken);
                        allComplete = !hasMoreData && allComplete;
                    }
                    finally
                    {
                        EngineLockProvider.Unlock(engine);
                    }
                }
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Round robin executor is finished, all pipelines have run to completion."));
        }
    }
}