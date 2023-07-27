# ffmpeg-dotnet-sample

This project exists to work with the FFMpegCore .NET library for analyzing and transcoding audio files. The nested C# projects test use cases for passing data into and getting data out of the FFMpegCore, and are described individually below.

## App1 - Calculate duration

`cd App1`

`dotnet run -in ../sample_data/test.wav` -- expect: 2643

`dotnet run -in ../sample_data/test.mp3` -- expect: 2690

## App2 - Transcode test audio to opus

`cd App2`


### Test Audio

`dotnet run -in ../sample_data/test.wav -out ../generated_data/test-wav.webm`

`dotnet run -in ../sample_data/test.mp3 -out ../generated_data/test-mp3.webm`

### Reference Audio

`dotnet run -in ../sample_data/B01___01_Matthew_____ENGCAVN2DA.mp3  -out ../generated_data/MATT1.webm`

Expected size: `452,473 bytes`<br />
Expected duration: `195000 ms`

## App3 - Running `Analyse()` with a stream

This is a stripped down version of App1 for testing FFProbe with a MemoryStream. The input file's data is put into a stream prior to being sent through the FFMpegCore library.

Unfortunately this does not seem to work as we want it to due to the encoding used: https://trac.ffmpeg.org/wiki/FFprobeTips#Streamduration

`cd App3`

`dotnet run -in ../sample_data/test.mp3`


## App 4 - Transcode and Determine Duration.

This is basically App2 and App3 combined. Many lines are commented, showing testing of options to tweak results. Duration is grabbed from the stderr output that is generated during transcoding operation.

`cd App4`

`dotnet run -in ../sample_data/test.mp3 -out ../generated_data/test-mp3.webm`

## App 5 - Test opus file concatenation, using memory streams

This builds on App4 and adds additional piped input for appending audio streams.

`cd App5`

`dotnet run -in ../generated_data/test-wav.webm -in ../generated_data/test-mp3.webm -out ../generated_data/test-concat.webm`


## Lessons Learned / Things to consider

- FFProbe may not be able to determine the duration of certain audio formats. (See link below for more details). Due to the duration being output by FFmpeg, we were able to use the information from there during processing of audio.
- When processing streams, the `-f webm` parameter may be necessary to force the audio format. Without this, you may encounter errors notifying you of a broken pipe.
- If you find that no data is being output, it may be that the position of your input stream(s) need to be reset prior to processing.
- FFmpeg places information on `stderr`. When using FFMpegCore, the `NotifyOnError` function can be used to parse this information. The callback you provide is called for each line sent to stderr.

## Links

- [FFmpeg Documentation Home](https://ffmpeg.org/documentation.html)
- [FFmpeg Concat Filter Documentation](https://ffmpeg.org/ffmpeg-filters.html#concat)
- [FFmpeg Bug Tracker and Wiki](https://trac.ffmpeg.org/wiki)
- [FFProbe and Duration](https://trac.ffmpeg.org/wiki/FFprobeTips#Streamduration)
- [FFMpegCore on Github](https://github.com/rosenbjerg/FFMpegCore)

