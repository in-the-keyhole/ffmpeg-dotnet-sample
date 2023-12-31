﻿namespace App2;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for transcoding an audio file to opus");

        var inputFile = new Option<string?>(name: "-in", description: "The audio file to be processed.");
        rootCommand.AddOption(inputFile);

        var outputFile = new Option<string?>(name: "-out", description: "The expected output file after transcoding to opus.");
        rootCommand.AddOption(outputFile);

        // write to file from ffmpeg
        //    rootCommand.SetHandler((input, output) =>
        //         {
        //             transcodeToOpus(input!, output!);
        //         },
        //         inputFile,
        //         outputFile
        //     );

        // write to byte[] from ffmpeg, then to file
        rootCommand.SetHandler((input, output) =>
            {
                var inputStream = new FileStream(input!, FileMode.Open, FileAccess.Read);

                // Put the file into a buffer for processing.
                // This is just for the example - not recommended.
                byte[] inputBuffer = new byte[(int)inputStream.Length];
                inputStream.Read(inputBuffer);
                inputStream.Close();

                var outputStream = transcodeToOpusBytes(inputBuffer);

                File.WriteAllBytes(output!, outputStream.ToArray());
                Console.WriteLine($"Successfully wrote to file: {output!}");
            },
            inputFile,
            outputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void transcodeToOpus(string input, string output)
    {
        var inputStream = new FileStream(input, FileMode.Open, FileAccess.Read);
        var inputPipe = new StreamPipeSource(inputStream);
        FFMpegArguments
            .FromPipeInput(inputPipe)
            .OutputToFile(output, true, options => options
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
                .WithCustomArgument("-application voip")
            )
        .ProcessSynchronously();
        inputStream.Close();
        Console.WriteLine($"Input: {input}");
        Console.WriteLine($"Output: {output}");
        return;
    }

    static MemoryStream transcodeToOpusStream(Stream inputStream)
    {
        var source = new StreamPipeSource(inputStream);

        var outputStream = new MemoryStream();
        var sink = new StreamPipeSink(outputStream);

        FFMpegArguments
            .FromPipeInput(source)
            .OutputToPipe(sink, options => options
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
                .WithCustomArgument("-application voip")
                .ForceFormat("webm")
            )
        .ProcessSynchronously();
        return outputStream;
    }

    // In case we want to provide a stream for both input and output
    static void transcodeToOpusStream(Stream inputStream, MemoryStream outputStream)
    {
        var source = new StreamPipeSource(inputStream);
        var sink = new StreamPipeSink(outputStream);

        FFMpegArguments
            .FromPipeInput(source)
            .OutputToPipe(sink, options => options
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
                .WithCustomArgument("-application voip")
                .ForceFormat("webm")
            )
        .ProcessSynchronously();
    }

    // In case we want to focus on byte[] for both input and output
    static byte[] transcodeToOpusBytes(byte[] input)
    {
        Stream inputStream = new MemoryStream(input);
        var source = new StreamPipeSource(inputStream);

        var outputStream = new MemoryStream();
        var sink = new StreamPipeSink(outputStream);

        FFMpegArguments
            .FromPipeInput(source)
            .OutputToPipe(sink, options => options
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
                .WithCustomArgument("-application voip")
                .ForceFormat("webm")
            )
        .ProcessSynchronously();
        return outputStream.ToArray();
    }
}