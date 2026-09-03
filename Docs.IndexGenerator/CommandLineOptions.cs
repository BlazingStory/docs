namespace BlazingStory.Docs.IndexGenerator;

internal class CommandLineOptions
{
    /// <summary>
    /// The content folder path ("Docs/Docs") containing "versions.json" and the version folders.<br/>
    /// Default: auto-detected by walking up from the current directory.
    /// </summary>
    public string? Docs { get; set; }

    /// <summary>
    /// The folder path where the ONNX model and vocabulary files are cached.<br/>
    /// Default: ".model-cache" under the project folder.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Rebuilds the search index files even if they are already up to date.<br/>
    /// Default: false.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Prints the chunks of every document instead of generating the index files, for troubleshooting
    /// how a document was split. No index file is written when this option is specified.<br/>
    /// Default: false.
    /// </summary>
    public bool Dump { get; set; }

    /// <summary>
    /// Prints the embedding vector of the given text instead of generating the index files, to check
    /// that this program and the browser side compute the same values.<br/>
    /// Default: null.
    /// </summary>
    public string? Embed { get; set; }
}
