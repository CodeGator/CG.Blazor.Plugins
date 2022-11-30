
namespace CG.Blazor.Plugins;

/// <summary>
/// This class represents an Blazor plugin related exception.
/// </summary>
[Serializable]
public class BlazorPluginException : Exception
{
    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorPluginException"/>
    /// class.
    /// </summary>
    public BlazorPluginException()
    {

    }

    // *******************************************************************

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorPluginException"/>
    /// class.
    /// </summary>
    /// <param name="message">The message to use for the exception.</param>
    /// <param name="innerException">An optional inner exception reference.</param>
    public BlazorPluginException(
        string message,
        Exception innerException
        ) : base(message, innerException)
    {

    }

    // *******************************************************************

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorPluginException"/>
    /// class.
    /// </summary>
    /// <param name="message">The message to use for the exception.</param>
    public BlazorPluginException(
        string message
        ) : base(message)
    {

    }

    // *******************************************************************

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorPluginException"/>
    /// class.
    /// </summary>
    /// <param name="info">The serialization info to use for the exception.</param>
    /// <param name="context">The streaming context to use for the exception.</param>
    public BlazorPluginException(
        SerializationInfo info,
        StreamingContext context
        ) : base(info, context)
    {

    }

    #endregion
}
