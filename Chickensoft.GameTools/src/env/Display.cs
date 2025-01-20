namespace Chickensoft.GameTools.Environment;

using Chickensoft.Platform;
using Godot;

/// <summary>
/// Information needed for a <see cref="Window"/> to properly support high DPI.
/// This is dependent on the theme scale, system scale, and other factors that
/// are examined during the process of analyzing the environment.
/// </summary>
/// <param name="AutoDisplayScale">The display scale factor as guessed from
/// the system DPI and screen size. In general, prefer
/// <paramref name="SystemScale" /></param>.
/// <param name="SystemScale">Actual system scale on Windows and macOS. This is
/// computed via Win32 DPI API's on Windows and CoreGraphics on macOS.
/// </param>
/// <param name="ContentScaleFactor">The new content scale factor needed to
/// scale the theme correctly based on the system scale.</param>
/// <param name="WindowSize">New window size.</param>
/// <param name="WindowPosition">New window position.</param>
public readonly record struct WindowDpiScaleInfo(
  float AutoDisplayScale,
  float SystemScale,
  float ContentScaleFactor,
  Vector2I WindowSize,
  Vector2I WindowPosition
);

/// <summary>
/// A collection of methods for working with displays and high DPI scaling.
/// </summary>
public static class Display {
  internal delegate float ScreenGetScaleDelegate(int screen);
  internal delegate Vector2I ScreenGetSizeDelegate(int screen);
  internal delegate int ScreenGetDpiDelegate(int screen);

  internal delegate float GetDisplayScaleFactorDelegate(Window window);

  internal static ScreenGetScaleDelegate ScreenGetScaleDefault { get; } =
    DisplayServer.ScreenGetScale;
  internal static ScreenGetScaleDelegate ScreenGetScale { get; set; } =
    ScreenGetScaleDefault;
  internal static ScreenGetSizeDelegate ScreenGetSizeDefault { get; } =
    DisplayServer.ScreenGetSize;
  internal static ScreenGetSizeDelegate ScreenGetSize { get; set; } =
    ScreenGetSizeDefault;
  internal static ScreenGetDpiDelegate ScreenGetDpiDefault { get; } =
    DisplayServer.ScreenGetDpi;
  internal static ScreenGetDpiDelegate ScreenGetDpi { get; set; } =
    ScreenGetDpiDefault;

  internal static GetDisplayScaleFactorDelegate
    GetDisplayScaleFactorDefault { get; } =
      Displays.Singleton.GetDisplayScaleFactor;
  internal static GetDisplayScaleFactorDelegate GetDisplayScaleFactor { get; set; } =
    GetDisplayScaleFactorDefault;

  /// <summary>
  /// Describes the screen index in a human-readable way.
  /// </summary>
  /// <param name="screen">Screen index</param>
  /// <returns>Screen description.</returns>
  public static string Describe(long screen) => screen switch {
    DisplayServer.ScreenOfMainWindow => "[Screen of Main Window]",
    DisplayServer.ScreenPrimary => "[Primary Screen]",
    DisplayServer.ScreenWithKeyboardFocus => "[Screen with Keyboard Focus]",
    DisplayServer.ScreenWithMouseFocus => "[Screen with Mouse Focus]",
    _ => $"[Screen {screen}]"
  };

  /// <summary>
  /// Describes the screen index in a human-readable way.
  /// </summary>
  /// <param name="screen">Screen index</param>
  /// <returns>Screen description.</returns>
  public static string Describe(int screen) =>
    Describe((long)screen);

  /// <summary>
  /// <para>
  /// Make a window support a high DPI display by updating its scale factor,
  /// size, and position based on the system scale and desired theme scale.
  /// </para>
  /// <para>
  /// If you are using the Chickensoft Platform GDExtension, this method will
  /// use it to determine the exact system scale factor for the monitor that
  /// the window is currently on. Otherwise, a best guess will be made, similar
  /// to how the Godot editor guesses which scale factor to use.
  /// </para>.
  /// </summary>
  /// <param name="window">The window to modify.</param>
  /// <param name="referenceSize">The size the window should be on a display
  /// with a scale factor of 1.0. If not supplied, this value is determined
  /// by reading the window width and height override from the project settings.
  /// Be sure to supply this value in your Godot project settings, as it is not
  /// set by default.
  /// </param>
  /// <param name="themeScale">The scale of the theme. Typical values are 1, 2,
  /// 3, and 4. This is equivalent to the @1x, @2x, @3x, and @4x asset sizes
  /// typically encountered in mobile app development. In general, design your
  /// theme at 3 or 4 times the size it would be on an HD screen and pass
  /// that scale factor in. The content scale factor will be adjusted,
  /// downscaling the theme to look correctly without pixelation on high DPI
  /// displays.
  /// </param>
  public static WindowDpiScaleInfo GetWindowDpiScaleInfo(
    this Window window,
    float themeScale,
    Vector2I? referenceSize = default
  ) {
    referenceSize ??= new Vector2I(
      ProjectSettings.GetSetting(
        "display/window/size/window_width_override"
      ).AsInt32(),
      ProjectSettings.GetSetting(
        "display/window/size/window_height_override"
      ).AsInt32()
    );

    var screen = window.CurrentScreen;
    var autoDisplayScale = GetAutoDisplayScale(screen);
    var systemScale = autoDisplayScale;
    if (Features.OperatingSystem != OSFamily.Linux) {
      // We can get a more granular system scale on Windows and macOS
      // using the Chickensoft.Platform package which invokes native api's
      // on those platforms.
      systemScale = GetDisplayScaleFactor(window);
    }

    // The content scale factor is the amount needed to shrink or grow the
    // theme based on the scale it was designed for. Naturally, this is a ratio
    // of the system scale and the theme's scale.
    var contentScaleFactor = systemScale / themeScale;

    var size = new Vector2I(
      (int)Mathf.Round(referenceSize.Value.X * systemScale),
      (int)Mathf.Round(referenceSize.Value.Y * systemScale)
    );

    // Logical, scaled screen size
    var screenSize = ScreenGetSize(screen);

    return new WindowDpiScaleInfo(
      autoDisplayScale,
      systemScale,
      contentScaleFactor,
      size,
      (screenSize - size) / 2
    );
  }

  /// <summary>
  /// <para>
  /// Guess the display scale the user might want the application to use based
  /// on a few heuristics involving the maximum known display scale,
  /// screen size, and screen DPI.
  /// </para>
  /// <para>
  /// This closely resembles the Godot Engine's
  /// <c>EditorSettings::get_auto_display_scale()</c> method. For the best
  /// experience, provide a setting in your application that allows the user to
  /// override the display scale.
  /// </para>
  /// </summary>
  /// <param name="screen">The screen to estimate the scale for. The default is
  /// the current screen (-1).</param>
  /// <returns>The best-guess scale factor, typically in the range of 1.0 to
  /// 2.0.</returns>
  public static float GetAutoDisplayScale(int screen = -1) {
    // Ported over from Godot's EditorSettings::get_auto_display_scale() method.
    // See editor/editor_settings.cpp in the Godot source code.
    // This uses various heuristics to "guess" the display scale the user might
    // want the application to use automatically.

    if (Features.OperatingSystem is OSFamily.macOS or OSFamily.Android) {
      return ScreenGetScale(screen);
    }

    var screenSize = ScreenGetSize(screen);

    if (screenSize == Vector2I.Zero) {
      // Invalid screen size, skip.
      return 1.0f;
    }

    // Use the smallest dimension to use a correct display scale on portrait
    // displays.
    var smallestDimension = Mathf.Min(screenSize.X, screenSize.Y);

    if (
      ScreenGetDpi(screen) >= 192 &&
      smallestDimension >= 1400
    ) {
      // hiDPI display.
      return 2.0f;
    }
    else if (smallestDimension >= 1700) {
      // Likely a hiDPI display, but we aren't certain due to the returned DPI.
      // Use an intermediate scale to handle this situation.
      return 1.5f;
    }

    return 1f;
  }
}
