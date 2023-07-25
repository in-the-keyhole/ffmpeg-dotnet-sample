namespace App4;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for transcoding and determining the length of an audio file in milliseconds");

        var inputFile = new Option<string?>(name: "-in", description: "The audio file to be processed.");
        rootCommand.AddOption(inputFile);

        var outputFile = new Option<string?>(name: "-out", description: "The expected output file after transcoding to opus.");
        rootCommand.AddOption(outputFile);

        // write to stream from ffmpeg, then to file
        rootCommand.SetHandler((input, output) =>
            {
                var inputStream = new FileStream(input!, FileMode.Open, FileAccess.Read);

                var outputStream = transcodeToOpusStream(inputStream);


                // write to outputFile
                File.WriteAllBytes(output!, outputStream.ToArray());
                // var outputFileStream = new FileStream(output!, FileMode.Create, FileAccess.ReadWrite);
                // outputStream.CopyTo(outputFileStream);
                // outputFileStream.Flush();
                // outputFileStream.Position = 0;

                // Now that we have an output file, check the duration of the output file

                // Attempts to use output did not work for me -- using input file for analysis.
                // Console.WriteLine($"Attempting to analyze {input}");
                Console.WriteLine($"Attempting to analyze from stream");
                outputStream.Position = 0;
                // outputStream.Seek(0, SeekOrigin.Begin);
                // var x = outputStream;
                // x.Position = 0;
                //new StreamPipeSource(outputStream).Source
                // var analysis = FFProbe.Analyse(outputStream);

                // Console.WriteLine(analysis.Duration);
                // Console.WriteLine(analysis.Duration.TotalMilliseconds);

                inputStream.Close();
                outputStream.Close();
            },
            inputFile,
            outputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static MemoryStream transcodeToOpusStream(Stream inputStream)
    {
        var source = new StreamPipeSource(inputStream);

        var outputStream = new MemoryStream();
        var sink = new StreamPipeSink(outputStream);

        FFMpegArguments
            .FromPipeInput(source)
            .OutputToPipe(sink, options => options
                .WithCustomArgument("-y")
                .WithCustomArgument("-vn")
                .WithCustomArgument("-map 0:a")
                .WithCustomArgument("-c:a libopus")
                // .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithAudioBitrate(16)
                // .WithVariableBitrate(0)
                .WithCustomArgument("-vbr off")
                .WithCustomArgument("-application voip")
                .WithCustomArgument("-recast_media")
                // .WithCustomArgument("-")
                // .SelectStream(0)
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
}
