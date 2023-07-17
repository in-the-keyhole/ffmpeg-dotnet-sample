# calculate duration 
cd App1

`dotnet run -in ../sample_data/test.wav` -- expect: 2643

`dotnet run -in ../sample_data/test.mp3` -- expect: 2690

# transcode test audio to opus
`cd ../App2`

`dotnet run -in ../sample_data/test.wav -out ../generated_data/test-wav.webm`

`dotnet run -in ../sample_data/test.mp3 -out ../generated_data/test-mp3.webm`

# App3

`cd App3`

`dotnet run`

This is a stripped down version of App1 for testing FFProbe with a MemoryStream

# transcode reference audio to opus (expected size: 452,473 bytes, expected duration: 195000 msec)
`dotnet run -in ../sample_data/B01___01_Matthew_____ENGCAVN2DA.mp3  -out ../generated_data/MATT1.webm`



