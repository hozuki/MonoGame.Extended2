# MonoGame.Extended2

A collection of extensions for MonoGame; a complement of [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended).

## Projects

- [Video Playback](Sources/MonoGame.Extended.VideoPlayback): A cross-platform `Video` and `VideoPlayer` implementation for MonoGame (DesktopGL and WindowsDX) using FFmpeg.
- [Text](Sources/MonoGame.Extended.Text): Text rendering with dynamic sprite fonts, using FreeType.
- [Drawing](Sources/MonoGame.Extended.Drawing): [WIP] A Direct2D-like drawing API for MonoGame. Similar to [LilyPath](https://github.com/jaquadro/LilyPath), but LilyPath exposes `System.Drawing`-like API and it does not support gradient brushes.
- [Overlay](Sources/MonoGame.Extended.Overlay): Exposes simple `System.Drawing` APIs for MonoGame, including drawing, filling, and text support, based on [SkiaSharp](https://github.com/mono/SkiaSharp).
- [WinForms](Sources/MonoGame.Extended.WinForms): Integrates MonoGame into WinForms. Heavily inspired by Justin Aquadro's [MonoGame-WinFormsControls](https://github.com/jaquadro/MonoGame-WinFormsControls), with architectural changes (e.g. decoupling). Since MonoGame has switched to SDL when targeting DesktopGL and setting SDL window's parent is not yet tried, this solution currently only works with WindowsDX target.

All projects have corresponding demos. Feel free to check them out.

## Contributing

If you catch a bug feel free to [open an issue](https://github.com/hozuki/MonoGame.Extended2/issues). Tests and extra features are also very appreciated, just open a pull request.

## License

BSD 3-Clause License
