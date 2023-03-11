# MonoGame.Extended.VideoPlayback

A cross-platform `Video` and `VideoPlayer` implementation for MonoGame (DesktopGL and WindowsDX) using FFmpeg.

**Screenshots:**

| Ubuntu | Windows |
|--------|------------|
| <img src="https://raw.githubusercontent.com/hozuki/OpenMLTD.Projector/master/media/VideoPlayback/screenshots/screenshot1.png" width="320" height="180" /> | <img src="https://raw.githubusercontent.com/hozuki/OpenMLTD.Projector/master/media/VideoPlayback/screenshots/screenshot2.png" width="320" height="180" /> |

## Overview

MonoGame.Extended.VideoPlayback mainly exposes two classes:

- `MonoGame.Extended.Framework.Media.Video`
- `MonoGame.Extended.Framework.Media.VideoPlayer`

These are the video-related classes for MonoGame/XNA. MonoGame runtimes targeting Windows (DirectX), macOS, iOS and Android all have corresponding
classes, but DesktopGL does not. So you can't play videos in MonoGame if you use DesktopGL target, unless you use some other supporting libraries. There is [one](https://github.com/brundows/XnaFFmpegDecoder), but it is rather incomplete.

Now with this library, you are able to play video using the same API as on other platforms in your game. For WindowsDX users, you can explicitly use the classes provided in this project, instead of the native ones (rely on Windows native codecs):

```csharp
using Video = MonoGame.Extended.Framework.Media.Video;
using VideoPlayer = MonoGame.Extended.Framework.Media.VideoPlayer;
```

The techniques, such as video-audio synchronization and independent thread rendering, can also be applied to elsewhere. You can consider this as a demonstration of building a video player upon FFmpeg under the context of .NET technologies.

## Usage

**Requirements**:

- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- MonoGame (≥ 3.8)
- FFmpeg binaries (5.1, though earlier versions may work with corresponding FFmpeg.AutoGen)

Preparation:

With the new FFmpeg bindings [Sdcb.FFmpeg](https://nuget.org/packages/Sdcb.FFmpeg), you do not have to worry too much about binaries and cross-platform compatibility.
In your project, reference [Sdcb.FFmpeg.runtime.windows-x64](https://nuget.org/packages/Sdcb.FFmpeg.runtime.windows-x64) package for Windows binaries. On other platforms
it will try to use the native libraries installed. For example:

```bash
# Ubuntu 22.04
sudo apt install ffmpeg
# libavcodec58, libavdevice58, ...

# macOS
brew install ffmpeg
```

Game code:

`Game1.cs`:

```csharp
VideoPlayer videoPlayer;
Video video;

protected override void LoadContent() {
    // The creation of VideoPlayer is a little different from standard implementations,
    // because Game.Instance is internal so we can't get the graphics device of the running game
    // within a class other than Game, without using reflection.

    // You can, however, use the no parameters version of the constructor like the comment below.
    // You will receive a compiler warning about why not doing that.

    // videoPlayer = new VideoPlayer();

    videoPlayer = new VideoPlayer(GraphicsDevice);
    video = VideoHelper.LoadFromFile("some_video.mp4");

    videoPlayer.Play(video);
}

protected override void UnloadContent() {
    videoPlayer.Stop();
    video.Dispose();
    videoPlayer.Dispose();
}

protected override void Draw(GameTime gameTime) {
    // Frame and audio synchronization is automatic.
    var texture = videoPlayer.GetTexture();

    if (texture != null) {
        spriteBatch.Begin();

        var destRect = new Rectangle(0, 0, WindowWidth, WindowHeight);
        spriteBatch.Draw(texture, destRect, Color.White);

        spriteBatch.End();
    }

    // Do NOT dispose the obtained texture. It is a image cache so it is not recreated every call.

    base.Draw(gameTime);
}
```

In this repository there is also a visual test application. It is a simple video player with simple pausing and restarting control. Check out how it works.

The test video is downloaded from [sample-videos.com](http://www.sample-videos.com/), a clip of [Big Buck Bunny](https://peach.blender.org/) made by Blender Foundation. ((c) copyright 2008, Blender Foundation / www.bigbuckbunny.org)

## Known Limitation(s)

- Running on different platforms. [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen) can target .NET Framework (≥ 4.5) or .NET Standard (≥ 2.0). However, when targeting .NET Framework, it [forces loading FFmpeg native libraries by calling `LoadLibrary`](https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/9e1dbffb70843eed62c0be5074da1e024da44622/FFmpeg.AutoGen/Native/LibraryLoader.cs). This makes it impossible to run on vanilla Mono, though it does provide a library mapping file. If you want to run the example on other platforms, please use [Wine](https://www.winehq.org/download) (with [wine-mono](https://wiki.winehq.org/Mono) installed) to launch. The code in MonoGame.Extended.VideoPlayback is compatible with both .NET Framework 4.5 and .NET Core 2.0, but I cannot publish a .NET Core application... :disappointed:

## Building

The library is written in pure C# so building it is a piece of cake.

- To build MonoGame.Extended.VideoPlayback you just need to restore the dependencies, and then compile the project, via Visual Studio or MSBuild CLI.

The code is written in C# 7.0, therefore you need a C# 7.0-compatible compiler. If you are using Visual Studio, you should use Visual Studio 2022.

The code is supplied with detailed comments and XML documentation. Hope these help to understand the logic behind the code.
