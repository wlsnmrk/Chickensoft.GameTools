namespace Chickensoft.GameTools.Environment;

using System;

/// <summary>
/// Godot feature tags.
/// </summary>
public static class Features
{
  // Godot uses feature tags for convenience. Sometimes, it's nice to take the
  // flat tag model and put it back into categories so we can do C# pattern
  // matching in our game code. That's what this file does.
  //
  // For more about feature tags, see:
  // https://docs.godotengine.org/en/stable/tutorials/export/feature_tags.html

  #region PlatformTags
  internal const string WINDOWS = "windows";
  internal const string WINDOWS_WEB = "web_windows";
  internal const string MACOS = "macos";
  internal const string MACOS_WEB = "web_macos";
  internal const string LINUX = "linux";
  internal const string LINUX_WEB = "web_linux";
  internal const string BSD = "bsd";
  /*
    no WEB_BSD as of Godot 4.3
  */
  internal const string IOS = "ios";
  internal const string IOS_WEB = "web_ios";
  internal const string ANDROID = "android";
  internal const string ANDROID_WEB = "web_android";

  internal const string LINUX_OR_BSD_WEB = "web_linuxbsd";
  internal const string WEB = "web";
  internal const string PC = "pc";
  internal const string MOBILE = "mobile";
  #endregion PlatformTags

  #region BuildTypeTags
  internal const string DEBUG = "debug";
  internal const string RELEASE = "release";
  #endregion BuildTypeTags

  #region ToolEnvironmentTags
  internal const string EDITOR = "editor";
  internal const string TEMPLATE = "template";
  #endregion ToolEnvironmentTags

  #region PrecisionTags
  internal const string DOUBLE = "double";
  internal const string SINGLE = "single";
  #endregion PrecisionTags

  #region BitLengthTags
  internal const string X32 = "32";
  internal const string X64 = "64";
  #endregion BitLengthTags

  #region ArchitectureTags
  internal const string X86 = "x86";
  internal const string ARM = "arm";
  internal const string RISC_V = "riscv";
  internal const string PPC = "ppc";
  internal const string WASM = "wasm";
  #endregion ArchitectureTags

  #region TextureCompressionTags
  internal const string ETC = "etc";
  internal const string ETC2 = "etc2";
  internal const string S3TC = "s3tc";
  #endregion TextureCompressionTags

  #region Interactivity Modes
  internal const string MOVIE = "movie";
  #endregion

  private static Lazy<OSFamily> _operatingSystem =
    new(GetOperatingSystem);
  private static Lazy<Platform> _platform =
    new(GetPlatform);
  private static Lazy<InteractivityMode> _interactivityMode =
    new(GetInteractivityMode);
  private static Lazy<BuildType> _buildType = new(GetBuildType);
  private static Lazy<ToolEnvironment> _toolEnvironment =
    new(GetToolEnvironment);
  private static Lazy<Precision> _precision = new(GetPrecision);
  private static Lazy<BitLength> _bitLength = new(GetBitLength);
  private static Lazy<Architecture> _architecture =
    new(GetArchitecture);
  private static Lazy<TextureCompression> _textureCompression =
    new(GetTextureCompression);

  private static OSFamily? _fakeOperatingSystem;
  private static Platform? _fakePlatform;
  private static InteractivityMode? _fakeInteractivityMode;
  private static BuildType? _fakeBuildType;
  private static ToolEnvironment? _fakeToolEnvironment;
  private static Precision? _fakePrecision;
  private static BitLength? _fakeBitLength;
  private static Architecture? _fakeArchitecture;
  private static TextureCompression? _fakeTextureCompression;


  /// <summary>Current operating system.</summary>
  public static OSFamily OperatingSystem =>
    _fakeOperatingSystem ?? _operatingSystem.Value;
  /// <summary>Current platform.</summary>
  public static Platform Platform => _fakePlatform ?? _platform.Value;
  /// <summary>Current interactivity mode.</summary>
  public static InteractivityMode InteractivityMode =>
    _fakeInteractivityMode ?? _interactivityMode.Value;
  /// <summary>Current build type.</summary>
  public static BuildType BuildType => _fakeBuildType ?? _buildType.Value;
  /// <summary>Current tool environment.</summary>
  public static ToolEnvironment ToolEnvironment =>
    _fakeToolEnvironment ?? _toolEnvironment.Value;
  /// <summary>Hardware precision.</summary>
  public static Precision Precision => _fakePrecision ?? _precision.Value;
  /// <summary>Hardware bit length.</summary>
  public static BitLength BitLength => _fakeBitLength ?? _bitLength.Value;
  /// <summary>Hardware architecture.</summary>
  public static Architecture Architecture =>
    _fakeArchitecture ?? _architecture.Value;
  /// <summary>Texture compression mode.</summary>
  public static TextureCompression TextureCompression =>
    _fakeTextureCompression ?? _textureCompression.Value;

  static Features()
  {
    HasFeatureDefault = Godot.OS.HasFeature;
    HasFeature = HasFeatureDefault;
  }

  /// <summary>
  /// Outputs the current application environment features to the Godot console.
  /// </summary>
  public static void PrintFeatureDiagnostics()
  {
    Print("-----------------------------------------------------");
    Print("         Application Environment Features");
    Print("-----------------------------------------------------");
    Print("Operating System: " + OperatingSystem);
    Print("Application Experience: " + Platform);
    Print("Interactivity: " + InteractivityMode);
    Print("Build Type: " + BuildType);
    Print("Tool Environment: " + ToolEnvironment);
    Print("Precision: " + Precision);
    Print("Bit Length: " + BitLength);
    Print("Architecture: " + Architecture);
    Print("Texture Compression: " + TextureCompression);
    Print("-----------------------------------------------------");
  }

  /// <summary>
  /// Resets all fake features and removes the cached values, forcing the
  /// feature values to be recomputed the next time they are requested.
  /// </summary>
  public static void Reset()
  {
    _fakeOperatingSystem = null;
    _fakePlatform = null;
    _fakeInteractivityMode = null;
    _fakeBuildType = null;
    _fakeToolEnvironment = null;
    _fakePrecision = null;
    _fakeBitLength = null;
    _fakeArchitecture = null;
    _fakeTextureCompression = null;

    _operatingSystem = new(GetOperatingSystem);
    _platform = new(GetPlatform);
    _interactivityMode = new(GetInteractivityMode);
    _buildType = new(GetBuildType);
    _toolEnvironment = new(GetToolEnvironment);
    _precision = new(GetPrecision);
    _bitLength = new(GetBitLength);
    _architecture = new(GetArchitecture);
    _textureCompression = new(GetTextureCompression);
  }

  /// <summary>Fakes the operating system for testing purposes.</summary>
  /// <param name="os">Operating system to use.</param>
  public static void FakeOperatingSystem(OSFamily os) =>
    _fakeOperatingSystem = os;

  /// <summary>Fakes the platform for testing purposes.</summary>
  /// <param name="platform">Platform to use.</param>
  public static void FakePlatform(Platform platform) =>
    _fakePlatform = platform;

  /// <summary>Fakes the interactivity mode for testing purposes.</summary>
  /// <param name="mode">Interactivity mode to use.</param>
  public static void FakeInteractivityMode(InteractivityMode mode) =>
    _fakeInteractivityMode = mode;

  /// <summary>Fakes the build type for testing purposes.</summary>
  /// <param name="type">Build type to use.</param>
  public static void FakeBuildType(BuildType type) =>
    _fakeBuildType = type;

  /// <summary>Fakes the tool environment for testing purposes.</summary>
  /// <param name="env">Tool environment to use.</param>
  public static void FakeToolEnvironment(ToolEnvironment env) =>
    _fakeToolEnvironment = env;

  /// <summary>Fakes the precision for testing purposes.</summary>
  /// <param name="precision">Precision to use.</param>
  public static void FakePrecision(Precision precision) =>
    _fakePrecision = precision;

  /// <summary>Fakes the bit length for testing purposes.</summary>
  /// <param name="bitLength">Bit length to use.</param>
  public static void FakeBitLength(BitLength bitLength) =>
    _fakeBitLength = bitLength;

  /// <summary>Fakes the architecture for testing purposes.</summary>
  /// <param name="architecture">Architecture to use.</param>
  public static void FakeArchitecture(Architecture architecture) =>
    _fakeArchitecture = architecture;

  /// <summary>Fakes the texture compression for testing purposes.</summary>
  /// <param name="compression">Texture compression to use.</param>
  public static void FakeTextureCompression(TextureCompression compression) =>
    _fakeTextureCompression = compression;

  // Stubs of actual godot methods so we can swap out for testing.
  internal static Func<string, bool> HasFeatureDefault { get; }
  internal static Func<string, bool> HasFeature { get; set; }

  internal static Action<string> PrintDefault { get; } =
    Godot.GD.Print;
  internal static Action<string> Print { get; set; } =
    PrintDefault;

  internal static OSFamily GetOperatingSystem() => true switch
  {
    _ when
      HasFeature(WINDOWS) ||
      HasFeature(WINDOWS_WEB) => OSFamily.Windows,
    _ when
      HasFeature(MACOS) ||
      HasFeature(MACOS_WEB) => OSFamily.macOS,
    _ when
      HasFeature(LINUX) ||
      HasFeature(LINUX_WEB) => OSFamily.Linux,
    _ when
      HasFeature(BSD) ||
      HasFeature(LINUX_OR_BSD_WEB) => OSFamily.FreeBSD,
    _ when
      HasFeature(IOS) ||
      HasFeature(IOS_WEB) => OSFamily.iOS,
    _ when
      HasFeature(ANDROID) ||
      HasFeature(ANDROID_WEB) => OSFamily.Android,
    _ => throw new PlatformNotSupportedException()
  };

  internal static Platform GetPlatform() => true switch
  {
    _ when HasFeature(WEB) => Platform.Web,
    _ when HasFeature(PC) => Platform.Desktop,
    _ when HasFeature(MOBILE) => Platform.Mobile,
    // if Godot doesn't know where it is, assume the developer is porting
    // to a console. If you're developing for a console and this is *not* the
    // case, please file an issue or open a PR with the intended behavior.
    _ => Platform.Console
  };

  internal static InteractivityMode GetInteractivityMode() => true switch
  {
    _ when HasFeature(MOVIE) => InteractivityMode.MovieMode,
    _ => InteractivityMode.Realtime
  };

  internal static BuildType GetBuildType() => true switch
  {
    _ when HasFeature(DEBUG) => BuildType.Debug,
    _ when HasFeature(RELEASE) => BuildType.Release,
    _ => throw new NotSupportedException()
  };

  internal static ToolEnvironment GetToolEnvironment() => true switch
  {
    _ when HasFeature(EDITOR) => ToolEnvironment.Editor,
    _ when HasFeature(TEMPLATE) => ToolEnvironment.ExportTemplate,
    _ => throw new NotSupportedException()
  };

  internal static Precision GetPrecision() => true switch
  {
    _ when HasFeature(DOUBLE) => Precision.Double,
    _ when HasFeature(SINGLE) => Precision.Single,
    _ => throw new NotSupportedException()
  };

  internal static BitLength GetBitLength() => true switch
  {
    _ when HasFeature(X32) => BitLength.X32,
    _ when HasFeature(X64) => BitLength.X64,
    _ => throw new NotSupportedException()
  };

  internal static Architecture GetArchitecture() => true switch
  {
    _ when HasFeature(X86) => Architecture.X86,
    _ when HasFeature(ARM) => Architecture.Arm,
    _ when HasFeature(RISC_V) => Architecture.RiscV,
    _ when HasFeature(PPC) => Architecture.PPC,
    _ when HasFeature(WASM) => Architecture.Wasm,
    _ => throw new PlatformNotSupportedException()
  };

  internal static TextureCompression GetTextureCompression() => true switch
  {
    _ when HasFeature(ETC) => TextureCompression.Etc,
    _ when HasFeature(ETC2) => TextureCompression.Etc2,
    _ when HasFeature(S3TC) => TextureCompression.S3TC,
    _ => throw new NotSupportedException()
  };
}
