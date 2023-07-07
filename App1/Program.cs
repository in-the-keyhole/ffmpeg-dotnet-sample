namespace App1;
using System;
using System.CommandLine;
using FFMpegCore;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var inputFile = new Option<string?>(name: "-in", description: "The audio file to be processed.");
        var rootCommand = new RootCommand("Sample app for determining the length of an audio file in milliseconds");
        rootCommand.AddOption(inputFile);

        rootCommand.SetHandler((file) =>
            {
                LengthOfAudioFile(file!);
            },
            inputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void LengthOfAudioFile(string file)
    {
        var a = FFProbe.Analyse(file);
        Console.WriteLine(a.Duration.TotalMilliseconds);
        return;
    }
}
