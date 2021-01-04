using CommandLine;

namespace TODOCommentMapper
{
    public class Options
    {
        /// <summary>
        /// Path to solution 
        /// </summary>
        [Option('p', Required = true, HelpText = "Path to solution")]
        public string SolutionFilePath { get; set; }
        
        /// <summary>
        /// Output for report CSV 
        /// </summary>
        [Option('o', Required = true, HelpText = "Path to solution")]
        public string ReportFilePath { get; set; }
    }
}