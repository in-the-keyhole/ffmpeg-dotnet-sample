namespace App3;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for determining the length of an audio file in milliseconds");

        // write to stream from ffmpeg, then to file
        rootCommand.SetHandler( () =>
            {
                var inputStream = new FileStream("../sample_data/test.mp3", FileMode.Open, FileAccess.Read);
                byte[] inputBuffer = new byte[(int)inputStream.Length];
                inputStream.Close();

                LengthOfAudioBuffer(inputBuffer);
            }
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void LengthOfAudioBuffer(byte[] audioBytesIn)
    {
        var s1 = new MemoryStream(audioBytesIn);
        var options = new FFOptions();
        options.LogLevel = FFMpegCore.Enums.FFMpegLogLevel.Debug;
        // options.TemporaryFilesFolder = "/tmp";
        // options.WorkingDirectory = "/tmp";

        var a = FFProbe.Analyse(s1, options);
        Console.WriteLine(a.Duration.TotalMilliseconds);
        return;
    }
}