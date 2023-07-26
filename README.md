# ffmpeg-dotnet-sample

This project exists to work with the FFMpegCore library for analyzing and transcoding audio files. The nested C# projects test use cases for passing data into and getting data out of the FFMpegCore, and are described individually below.

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


## App 4 - Transcode and Analyze memory stream together.

This is basically App2 and App3 combined. Many lines are commented, showing testing of options to tweak results. Duration is grabbed from the stderr output that is generated during transcoding operation.

`cd App4`

`dotnet run -in ../sample_data/test.mp3 -out ../generated_data/test-mp3.webm`

## App 5 - Test opus-to-opus, using memory streams

Opus-to-opus file transcoding was hanging, so this is to test out possible solutions to that problem.

`cd App5`

`dotnet run -in ../generated_data/test-wav.webm -out -in ../generated_data/test-mp3.webm -out ../generated_data/test-concat.webm`


