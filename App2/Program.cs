namespace App2;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for determining the length of an audio file in milliseconds");

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

        // write to stream from ffmpeg, then to file
        rootCommand.SetHandler( (input, output) =>
            {
                var inputStream = new FileStream(input!, FileMode.Open, FileAccess.Read);

                var outputStream = transcodeToOpusStream(inputStream);
                
                // write to outputFile
                File.WriteAllBytes(output!, outputStream.ToArray());

                inputStream.Close();
                outputStream.Close();

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
                // Not sure what "-application voip" from the reference is meant to do
                // .WithCustomArgument("-application voip")
            )
        .ProcessSynchronously();
        inputStream.Close();
        Console.WriteLine($"Input: {input}");
        Console.WriteLine($"Output: {output}");
        return;
    }

    static MemoryStream transcodeToOpusStream(FileStream inputStream)
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
                // Not sure what "-application voip" from the reference is meant to do
                // .WithCustomArgument("-application voip")
                .ForceFormat("webm") // Some reason this works
            )
        .ProcessSynchronously();
        return outputStream;
    }

    // In case we want to provide a stream for both input and output
    static void transcodeToOpusStream(FileStream inputStream, MemoryStream outputStream)
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
                // Not sure what "-application voip" from the reference is meant to do
                // .WithCustomArgument("-application voip")
                .ForceFormat("webm") // Some reason this works
            )
        .ProcessSynchronously();
    }
}