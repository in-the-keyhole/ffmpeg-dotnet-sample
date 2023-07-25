namespace App3;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var inputFile = new Option<string?>(name: "-in", description: "The audio file to be processed.");
        var rootCommand = new RootCommand("Sample app for determining the length of an audio file in milliseconds");
        rootCommand.AddOption(inputFile);

        // Override option for ffmpeg bin directory
        // GlobalFFOptions.Configure(options => options.BinaryFolder = @"C:\software\ffmpeg4_2");

        // write to stream from ffmpeg, then to file
        rootCommand.SetHandler( (input) =>
            {
                var inputStream = new FileStream(input!, FileMode.Open, FileAccess.Read);
                byte[] inputBuffer = new byte[(int)inputStream.Length];
                inputStream.Close();

                LengthOfAudioBuffer(inputBuffer);
            },
            inputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void LengthOfAudioBuffer(byte[] audioBytesIn)
    {
        var s1 = new MemoryStream(audioBytesIn);

        var a = FFProbe.Analyse(s1);
        Console.WriteLine(a.Duration.TotalMilliseconds);
        return;
    }
}