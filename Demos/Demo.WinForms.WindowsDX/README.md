# Notes for Testing

Due to the removal of [MonoGame.Framework.Portable](https://www.nuget.org/packages/MonoGame.Framework.Portable) in MonoGame v3.8, there isn't a good way to build core projects with a single "facade" library and run with the real ones.
The core libraries have to reference [MonoGame.Framework.DesktopGL](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL) and it is not compatible with [MonoGame.Framework.WindowsDX](https://www.nuget.org/packages/MonoGame.Framework.WindowsDX).
Therefore to run this demo you have to fix the project package references yourself.

