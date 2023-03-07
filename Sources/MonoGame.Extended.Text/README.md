# MonoGame.Extended.Text

Text rendering with dynamic sprite fonts, using FreeType.

**Screenshots:**

<img src="https://raw.githubusercontent.com/hozuki/OpenMLTD.Projector/master/media/Text/screenshots/screenshot1.png" width="320" height="180" />

## Overview

MonoGame.Extended.Text mainly exposes `MonoGame.Extended.Text.DynamicSpriteFont` class.
With the extension methods, you can use a syntax similar to `spriteBatch.DrawString(...)` methods, to any string content using custom fonts.

## Usage

**Requirements**:

- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- MonoGame (≥ 3.8)
- libfreetype

`Game1.cs`:

```csharp
FontManager fontManager;
Font font;
DynamicSpriteFont spriteFont;

protected override void LoadContent() {
    fontManager = new FontManager();
    const float fontSize = 20;
    font = fontManager.LoadFont("some_font.ttf", fontSize);
    spriteFont = new DynamicSpriteFont(GraphicsDevice, font);
}

protected override void UnloadContent() {
    spriteFont.Dispose();
    fontManager.Dispose();
}

protected override void Draw(GameTime gameTime) {
    spriteBatch.Begin();

    var location = new Vector2(100, 100);
    spriteBatch.DrawString(spriteFont, "Hello world! 世界你好！", location, Color.White);

    spriteBatch.End();

    base.Draw(gameTime);
}
```

## Building

The code is written in C# 7.0, therefore you need a C# 7.0-compatible compiler. If you are using Visual Studio, you should use Visual Studio 2022.

The code is supplied with detailed comments and XML documentation. Hope these help to understand the logic behind the code.
