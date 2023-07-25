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

                var (outputStream, duration) = transcodeToOpusStream(inputStream);
                
                Console.WriteLine($"Time is: {duration}");
                Console.WriteLine($"...and in milliseconds: {duration.TotalMilliseconds}");

                File.WriteAllBytes(output!, outputStream.ToArray());

                inputStream.Close();
                outputStream.Close();
            },
            inputFile,
            outputFile
        );

        return await rootCommand.InvokeAsync(args);
    }

    static (MemoryStream, TimeSpan) transcodeToOpusStream(Stream inputStream)
    {
        var source = new StreamPipeSource(inputStream);

        var outputStream = new MemoryStream();
        var sink = new StreamPipeSink(outputStream);
        
        var secondToLastLine = "";
        var lastLine = "";
        
        FFMpegArguments
            .FromPipeInput(source)
            .OutputToPipe(sink, options => options
                .WithCustomArgument("-hide_banner") // just to reduce output we are notified about
                .SelectStream(0)
                .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                .WithVariableBitrate(0)
                .WithAudioBitrate(16)
                .WithCustomArgument("-application voip")
                .ForceFormat("webm")
                
                // Test arguments
                // .WithCustomArgument("-y")
                // .WithCustomArgument("-vn")
                // .WithCustomArgument("-map 0:a")
                // .WithCustomArgument("-c:a libopus")
                // .WithCustomArgument("-vbr off")
                // .WithCustomArgument("-recast_media")
            )
            .NotifyOnError(o=>{
                secondToLastLine = lastLine;
                lastLine = o;
            })
        .ProcessSynchronously();
        
        // Time should be on the second-to-last line
        var time = GrabTimeFromOutput(secondToLastLine);
        var duration = TimeSpanFromTimeString(time).Duration();
        
        return (outputStream, duration);
    }
    
    private static string GrabTimeFromOutput(string line)
    {
        var needle = " time=";
        int entry = line.IndexOf(needle);
        var timeString = line.Substring(entry).TrimStart();
        timeString = timeString.Split(' ')[0].Split('=')[1];
        return timeString;
    }
    
    private static TimeSpan TimeSpanFromTimeString(string time)
    {
        var t = TimeOnly.Parse(time);
        return t.ToTimeSpan();
    }
}
