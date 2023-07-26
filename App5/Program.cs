﻿namespace App5;
using System;
using System.CommandLine;
using FFMpegCore;
using FFMpegCore.Pipes;
using FFMpegCore.Enums;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sample app for testing opus-to-opus files.");

        var inputFiles = new Option<string[]?>(name: "-in", description: "The audio file to be processed.");
        rootCommand.AddOption(inputFiles);

        var outputFile = new Option<string?>(name: "-out", description: "The expected output file.");
        rootCommand.AddOption(outputFile);
        
        // Override option for ffmpeg bin directory
        GlobalFFOptions.Configure(options =>
        {
            options.BinaryFolder = @"C:\software\ffmpeg4_2";
            options.UseCache = true;
            options.TemporaryFilesFolder = @"c:\temp";
            options.WorkingDirectory = @"c:\temp";
        });

        // write to stream from ffmpeg, then to file
        rootCommand.SetHandler((input, output) =>
            {
                var inputStreams = new List<Stream>();
                foreach(String file in input!)
                {
                    Console.WriteLine($"Input : {file}");
                    inputStreams.Add(new FileStream(file, FileMode.Open, FileAccess.Read));
                }
                Console.WriteLine($"Output: {output!}");
                
                var (outputStream, duration) = concatenateStreams(inputStreams.ToArray());
                
                Console.WriteLine($"Total Time is: {duration}");
                Console.WriteLine($"...and in milliseconds: {duration.TotalMilliseconds}");

                File.WriteAllBytes(output!, outputStream.ToArray());

                foreach(Stream s in inputStreams)
                {
                    s.Close();
                }
                outputStream.Close();
            },
            inputFiles,
            outputFile
        );

        return await rootCommand.InvokeAsync(args);
    }
    
    // static (MemoryStream, TimeSpan) concatenateStreams(Stream[] streams)
    // {
    // }
    
    static string BuildConcatFilter(int numberOfStreams)
    {
        var filterString = "";
        for(int i = 0; i<numberOfStreams; i++)
        {
            filterString += $"[{i}:0][{i}:0]";
        }
        filterString += $"concat=n={numberOfStreams}:v=0:a={numberOfStreams} [outa]";
        return filterString;
    }
    
    static (MemoryStream, TimeSpan) concatenateStreams(Stream[] streams)
    {
        var numberOfStreams = streams.Length;
        var complexFilter = BuildConcatFilter(numberOfStreams);
        
        var inputPipes = new StreamPipeSource[numberOfStreams];
        FFMpegArguments args = null!;
        
        for(int i=0; i< streams.Length; i++)
        {
            inputPipes[i] = new StreamPipeSource(streams[i]);
            
            if(i == 0)
            {
                args = FFMpegArguments.FromPipeInput(inputPipes[i], a=>{
                    // a.ForceFormat("webm");
                    // a.WithCustomArgument("-hide_banner"); // just to reduce output we are notified about
                    // a.WithCustomArgument("-vn");
                });
            }
            else
            {
                args.AddPipeInput(inputPipes[i], a=>{
                    // a.ForceFormat("webm");
                    // a.SelectStream(0);
                    // a.WithCustomArgument("-safe");
                });
            }
        }
        
        var outputStream = new MemoryStream();
        var sink = new StreamPipeSink(outputStream);
        var secondToLastLine = "";
        var lastLine = "";
        
        args.OutputToPipe(sink, options => options
                // .SelectStreams(new List<int>{0}, 0, Channel.Audio)
                // .SelectStreams(new List<int>{0}, 1, Channel.Audio)
                // .WithAudioCodec(FFMpeg.GetCodec("libopus"))
                // .WithVariableBitrate(0)
                // .WithAudioBitrate(16)
                // .WithCustomArgument("-application voip")
                // .WithFastStart()
                // .WithCustomArgument(@"-filter_complex ""[0:a:0][1:a:0]concat=n=2:v=0:a=2 [outa]""")
                .WithCustomArgument($@"-filter_complex ""{complexFilter}""")
                .WithCustomArgument(@"-map [outa]")
                // .WithCustomArgument(@"-map '[outv]'")
                // .OverwriteExisting()
                // .WithDuration(TimeSpan.FromSeconds(4.5))
                // .ForceFormat("webm")
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
                // .WithFastStart()
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
