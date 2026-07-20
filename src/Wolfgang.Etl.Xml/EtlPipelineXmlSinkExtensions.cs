using System;
using System.IO;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Class-named XML sink terminators for the fluent <see cref="EtlPipeline"/> chain. Each terminator
/// constructs the matching loader and terminates the pipeline, enabling
/// <c>… .XmlSingleStreamLoader&lt;Person&gt;("people.xml")</c>.
/// </summary>
/// <remarks>
/// Path-based terminators own the file stream they create and close it after the run completes
/// (success or failure). Stream-based terminators do not close the stream — the caller owns it
/// (respecting <see cref="XmlSingleStreamLoaderOptions.LeaveOpen"/>).
/// </remarks>
public static class EtlPipelineXmlSinkExtensions
{
    /// <summary>
    /// Terminates the pipeline, writing all records as a single-root XML document to a file. The
    /// factory owns the file stream and closes it after the run.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="path">The path of the XML file to write.</param>
    /// <param name="options">Optional loader options. When <see langword="null"/>, defaults are used.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
    public static IEtlPipelineSink XmlSingleStreamLoader<T>
    (
        this IEtlPipeline<T> pipeline,
        string path,
        XmlSingleStreamLoaderOptions? options = null
    )
        where T : notnull, new()
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        var stream = File.Create(path);
        var loader = new XmlSingleStreamLoader<T>(stream, options);
        return pipeline.To(loader).DisposingOwned(stream);
    }


    /// <summary>
    /// Terminates the pipeline, writing all records as a single-root XML document to a stream. The
    /// caller owns the stream.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="stream">The stream to write the XML document to.</param>
    /// <param name="options">Optional loader options. When <see langword="null"/>, defaults are used.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
    public static IEtlPipelineSink XmlSingleStreamLoader<T>
    (
        this IEtlPipeline<T> pipeline,
        Stream stream,
        XmlSingleStreamLoaderOptions? options = null
    )
        where T : notnull, new()
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        return pipeline.To(new XmlSingleStreamLoader<T>(stream, options));
    }


    /// <summary>
    /// Terminates the pipeline, writing one XML document per record to a stream chosen per record.
    /// The loader closes each stream after writing its record.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="streamFactory">
    /// A factory that receives each record and returns the stream to write it to. The loader disposes
    /// the returned stream after writing.
    /// </param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="streamFactory"/> is <see langword="null"/>.</exception>
    public static IEtlPipelineSink XmlMultiStreamLoader<T>
    (
        this IEtlPipeline<T> pipeline,
        Func<T, Stream> streamFactory
    )
        where T : notnull, new()
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (streamFactory is null)
        {
            throw new ArgumentNullException(nameof(streamFactory));
        }

        return pipeline.To(new XmlMultiStreamLoader<T>(streamFactory));
    }
}
