using System;
using System.Diagnostics.CodeAnalysis;
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
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlSingleStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
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

        return ToFile(pipeline, path, options, progress: null);
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
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlSingleStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
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
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlMultiStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
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


    /// <summary>
    /// Terminates the pipeline, writing all records as a single-root XML document to a file, and
    /// reports the loader's own <see cref="XmlReport"/> progress to <paramref name="progress"/> while
    /// the pipeline runs. The factory owns the file stream and closes it after the run.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="path">The path of the XML file to write.</param>
    /// <param name="progress">Receives the loader's <see cref="XmlReport"/> reports.</param>
    /// <param name="options">Optional loader options. When <see langword="null"/>, defaults are used.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/>, <paramref name="path"/> or <paramref name="progress"/> is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlSingleStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
    (
        this IEtlPipeline<T> pipeline,
        string path,
        IProgress<XmlReport> progress,
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

        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return ToFile(pipeline, path, options, progress);
    }


    /// <summary>
    /// Terminates the pipeline, writing all records as a single-root XML document to a stream, and
    /// reports the loader's own <see cref="XmlReport"/> progress to <paramref name="progress"/> while
    /// the pipeline runs. The caller owns the stream.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="stream">The stream to write the XML document to.</param>
    /// <param name="progress">Receives the loader's <see cref="XmlReport"/> reports.</param>
    /// <param name="options">Optional loader options. When <see langword="null"/>, defaults are used.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/>, <paramref name="stream"/> or <paramref name="progress"/> is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlSingleStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
    (
        this IEtlPipeline<T> pipeline,
        Stream stream,
        IProgress<XmlReport> progress,
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

        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return pipeline.To(new XmlSingleStreamLoader<T>(stream, options), progress);
    }


    /// <summary>
    /// Terminates the pipeline, writing one XML document per record to a stream chosen per record,
    /// and reports the loader's own <see cref="XmlReport"/> progress to <paramref name="progress"/>
    /// while the pipeline runs. The loader closes each stream after writing its record.
    /// </summary>
    /// <typeparam name="T">The record type to serialize.</typeparam>
    /// <param name="pipeline">The pipeline to terminate.</param>
    /// <param name="streamFactory">
    /// A factory that receives each record and returns the stream to write it to. The loader disposes
    /// the returned stream after writing.
    /// </param>
    /// <param name="progress">Receives the loader's <see cref="XmlReport"/> reports.</param>
    /// <returns>A runnable <see cref="IEtlPipelineSink"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/>, <paramref name="streamFactory"/> or <paramref name="progress"/> is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    public static IEtlPipelineSink XmlMultiStreamLoader<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
    (
        this IEtlPipeline<T> pipeline,
        Func<T, Stream> streamFactory,
        IProgress<XmlReport> progress
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

        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return pipeline.To(new XmlMultiStreamLoader<T>(streamFactory), progress);
    }


    [RequiresUnreferencedCode(XmlSerialization.UnreferencedCodeMessage)]
    [RequiresDynamicCode(XmlSerialization.DynamicCodeMessage)]
    private static IEtlPipelineSink ToFile<[DynamicallyAccessedMembers(XmlSerialization.RecordMembers)] T>
    (
        IEtlPipeline<T> pipeline,
        string path,
        XmlSingleStreamLoaderOptions? options,
        IProgress<XmlReport>? progress
    )
        where T : notnull, new()
    {
        // The file is opened (created/truncated) on the first write, not here: composing the pipeline
        // has no file-system side effects, and a dry run never touches an existing destination.
        var stream = DeferredFileStream.ForWrite(path);
        try
        {
            var loader = new XmlSingleStreamLoader<T>(stream, options);
            var sink = progress is null ? pipeline.To(loader) : pipeline.To(loader, progress);
            return sink.DisposingOwned(stream);
        }
        catch
        {
            // Nothing owns the stream until DisposingOwned returns, so a constructor that rejects
            // the options (e.g. an invalid RootElementName) must not leak the open file handle.
            stream.Dispose();
            throw;
        }
    }
}
