namespace App2;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Enums;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for determining the length of an audio file in milliseconds");

        var inputFile = new Option<string?>(name: "-in", description: "The audio file to be processed.");
        rootCommand.AddOption(inputFile);

        var outputFile = new Option<string?>(name: "-out", description: "The expected output file after transcoding to opus.");
        rootCommand.AddOption(outputFile);

        rootCommand.SetHandler((input, output) =>
            {
                transcodeToOpus(input!, output!);
            },
            inputFile,
            outputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void transcodeToOpus(string input, string output)
    {
        FFMpegArguments
            .FromFileInput(input, true)
            .OutputToFile(output, true, options => options
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
            )
        .ProcessSynchronously();
        Console.WriteLine($"Input: {input}");
        Console.WriteLine($"Output: {output}");
        return;
    }
}