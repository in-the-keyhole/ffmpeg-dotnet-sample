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
        rootCommand.SetHandler((input) =>
            {
                var inputStream = new FileStream(input!, FileMode.Open, FileAccess.Read);
                var inputBuffer = new MemoryStream(new byte[(int)inputStream.Length]);
                inputStream.CopyTo(inputBuffer);
                inputStream.Close();

                LengthOfAudioBuffer(inputBuffer);
            },
            inputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static void LengthOfAudioBuffer(Stream inputStream)
    {
        inputStream.Position = 0;
        // var ps = new FFProbeStream().Duration;
        // new FFProbeAnalysis().Streams.Add(s1);
        var options = new FFOptions();
        options.LogLevel = FFMpegCore.Enums.FFMpegLogLevel.Debug;
        options.TemporaryFilesFolder = @"C:\temp";
        options.WorkingDirectory = @"C:\temp";
        options.UseCache = true;
        
        var outs = new MemoryStream();
        var sink = new StreamPipeSink(outs);

        try
        {
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(sink, options => options
                    .SelectStream(0)
                    .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                    // .WithVariableBitrate(0)
                    .WithAudioBitrate(16)
                    .WithCustomArgument("-application voip")
                    .ForceFormat("webm")
                    .WithFastStart()
                    // .WithCustomArgument("-f null")
                )
                // .NotifyOnOutput(o=>{
                //     Console.WriteLine($"Some output: {o}");
                // })
                .NotifyOnError(o=>{
                    Console.WriteLine($"Some error: {o}");
                })
                // .NotifyOnProgress(progress=>{
                //     Console.WriteLine($"Making progress... {progress}%");
                // }, TimeSpan.FromMilliseconds(5))
            .ProcessSynchronously();
            
            outs.Position = 0;
            // var a = FFProbe.Analyse(inputStream, options);
            // Console.WriteLine($"Duration: {a.Duration.TotalMilliseconds} ms");
            // Console.WriteLine($"Duration: {a.Format.Duration} ms");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        // var options = new FFOptions();
        // FFMpegArguments.FromPipeInput(new StreamPipeSource(s1)).OutputToPipe(new StreamPipeSink(s2)).Configure(c=>{
        // }).ProcessSynchronously(false, options);

        return;
    }
}