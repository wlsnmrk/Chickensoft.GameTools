namespace Chickensoft.GameTools.Environment;

using System.Diagnostics.CodeAnalysis;

/// <summary>Operating system family.</summary>
public enum OSFamily
{
  /// <summary>Windows operating system family.</summary>
  Windows,
  /// <summary>Linux operating system family.</summary>
  Linux,
  /// <summary>macOS operating system family.</summary>
  macOS,
  /// <summary>iOS operating system family.</summary>
  iOS,
  /// <summary>Android operating system family.</summary>
  Android,
  /// <summary>FreeBSD operating system family.</summary>
  FreeBSD
}

/// <summary>Application platform.</summary>
public enum Platform
{
  /// <summary>Web platform.</summary>
  Web,
  /// <summary>Desktop platform.</summary>
  Desktop,
  /// <summary>Mobile platform.</summary>
  Mobile,
  /// <summary>Console platform.</summary>
  Console
}

/// <summary>Interactivity mode.</summary>
public enum InteractivityMode
{
  /// <summary>Realtime (game) mode.</summary>
  Realtime,
  /// <summary>Non-realtime (MovieMode Maker) mode.</summary>
  MovieMode
}

/// <summary>Build type.</summary>
public enum BuildType
{
  /// <summary>Debug build.</summary>
  Debug,
  /// <summary>Release build.</summary>
  Release,
}

/// <summary>Godot tool environment.</summary>
public enum ToolEnvironment
{
  /// <summary>Editor (development) environment.</summary>
  Editor,
  /// <summary>Exported application.</summary>
  ExportTemplate
}

/// <summary>Floating point precision.</summary>
[
  SuppressMessage(
    "Naming",
    "CA1720",
    Justification = "Represents the named types"
  )
]
public enum Precision
{
  /// <summary>64-bit precision.</summary>
  Double,
  /// <summary>32-bit precision.</summary>
  Single
}

/// <summary>Hardware architecture bit length.</summary>
public enum BitLength
{
  /// <summary>32-bit architecture.</summary>
  X32,
  /// <summary>64-bit architecture.</summary>
  X64
}

/// <summary>Hardware architecture.</summary>
public enum Architecture
{
  /// <summary>x86 hardware architecture.</summary>
  X86,
  /// <summary>ARM hardware architecture.</summary>
  Arm,
  /// <summary>RISCV hardware architecture.</summary>
  RiscV,
  /// <summary>Power PC hardware architecture.</summary>
  PPC,
  /// <summary>Web assembly hardware architecture.</summary>
  Wasm,
}

/// <summary>Texture compression mode.</summary>
public enum TextureCompression
{
  /// <summary>Ericsson texture compression (lossy).</summary>
  Etc,
  /// <summary>Ericsson texture compression (lossy), version 2.</summary>
  Etc2,
  /// <summary>S3 texture compression (lossy).</summary>
  S3TC,
}
