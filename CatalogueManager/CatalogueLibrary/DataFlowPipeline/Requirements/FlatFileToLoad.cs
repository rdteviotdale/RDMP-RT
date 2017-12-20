using System.IO;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// Wrapper for FileInfo that can be used in the IPipelineRequirement interface to indicate that component expects a FileInfo that is specifically going to have data loaded
    /// out of it.  Having an IPipelineRequirement for a FileInfo on a component could be confusing, we might also want to allow multiple different types of FileInfo.  Having
    /// this wrapper ensures that there is no confusion about what a FlatFileToLoad Initialization Object is for. 
    /// </summary>
    public class FlatFileToLoad
    {
        public FlatFileToLoad(FileInfo file)
        {
            File = file;
        }

        public FileInfo File { get; set; }
    }
}