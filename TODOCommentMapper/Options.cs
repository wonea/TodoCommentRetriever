using CommandLine;

namespace TODOCommentMapper
{
    public class Options
    {
        /// <summary>
        /// Solution 
        /// </summary>
        [Option('p', Required = true, HelpText = "Path to solution")]
        public string Path { get; set; }
    }
}