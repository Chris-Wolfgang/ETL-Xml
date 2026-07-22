using System;
using System.Collections.Generic;
using System.IO;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Xml;

/// <summary>
/// Class-named XML source factories for the fluent <see cref="EtlPipeline"/> chain. Each factory
/// constructs the matching extractor and seeds the pipeline, enabling
/// <c>EtlPipeline.Create().XmlSingleStreamExtractor&lt;Invoice&gt;("invoices.xml")</c>.
/// </summary>
/// <remarks>
/// Path-based factories own the file stream they open and close it when extraction finishes,
/// whether it completes or throws. Stream-based factories do not close the stream — the caller
/// owns its lifetime (respecting <see cref="XmlSingleStreamExtractorOptions.LeaveOpen"/>).
/// </remarks>
public static class EtlPipelineXmlSourceExtensions
{
    /// <summary>
    /// Starts a pipeline that reads records from a single-root XML file. The factory owns the file
    /// stream and closes it when extraction finishes.
    /// </summary>
    /// <typeparam name="T">The record type to deserialize.</typeparam>
    /// <param name="pipeline">The pipeline seed from <see cref="EtlPipeline.Create"/>.</param>
    /// <param name="path">The path of the XML file to read.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> XmlSingleStreamExtractor<T>
    (
        this EtlPipeline pipeline,
        string path
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

        var stream = File.OpenRead(path);

        // The factory opened the stream, so it owns it: LeaveOpen = false makes the extractor close
        // the stream (via XmlReader.CloseInput) when the reader is disposed — on success and failure.
        var extractor = new XmlSingleStreamExtractor<T>
        (
            stream,
            new XmlSingleStreamExtractorOptions { LeaveOpen = false }
        );

        return pipeline.From(extractor);
    }


    /// <summary>
    /// Starts a pipeline that reads records from a single-root XML stream. The caller owns the stream.
    /// </summary>
    /// <typeparam name="T">The record type to deserialize.</typeparam>
    /// <param name="pipeline">The pipeline seed from <see cref="EtlPipeline.Create"/>.</param>
    /// <param name="stream">The stream to read the XML document from.</param>
    /// <param name="options">Optional extractor options. When <see langword="null"/>, defaults are used.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> XmlSingleStreamExtractor<T>
    (
        this EtlPipeline pipeline,
        Stream stream,
        XmlSingleStreamExtractorOptions? options = null
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

        return pipeline.From(new XmlSingleStreamExtractor<T>(stream, options));
    }


    /// <summary>
    /// Starts a pipeline that reads one record per stream from a sequence of single-document XML
    /// streams. Each stream is closed by the extractor after its record is read.
    /// </summary>
    /// <typeparam name="T">The record type to deserialize.</typeparam>
    /// <param name="pipeline">The pipeline seed from <see cref="EtlPipeline.Create"/>.</param>
    /// <param name="streams">The streams to read, one XML document each.</param>
    /// <returns>An <see cref="IEtlPipeline{T}"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="streams"/> is <see langword="null"/>.</exception>
    public static IEtlPipeline<T> XmlMultiStreamExtractor<T>
    (
        this EtlPipeline pipeline,
        IEnumerable<Stream> streams
    )
        where T : notnull, new()
    {
        if (pipeline is null)
        {
            throw new ArgumentNullException(nameof(pipeline));
        }

        if (streams is null)
        {
            throw new ArgumentNullException(nameof(streams));
        }

        return pipeline.From(new XmlMultiStreamExtractor<T>(streams));
    }
}
