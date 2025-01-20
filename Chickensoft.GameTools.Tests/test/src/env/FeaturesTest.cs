namespace Chickensoft.GameTools.Tests.Environment;

using Chickensoft.GoDotTest;
using Chickensoft.GameTools.Environment;
using Godot;
using Shouldly;
using System.Collections.Generic;
using System;

public class FeaturesTest(Node testScene) : TestClass(testScene) {
  private HashSet<string> _features = default!;

  [SetupAll]
  public void SetupAll() {
#pragma warning disable IDE0200 // Not a useless lambda, doesn't work without it
    Features.HasFeature = (feature) => _features.Contains(feature);
#pragma warning restore IDE0200
  }

  [CleanupAll]
  public void CleanupAll() {
    _features.Clear();
    Features.HasFeature = Features.HasFeatureDefault;
    Features.Reset();
  }

  [Test]
  public void TestOperatingSystem() {
    // win
    _features = [Features.WINDOWS];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Windows);

    // test accessor property
    Should.NotThrow(() => Features.OperatingSystem);

    _features = [Features.WINDOWS_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Windows);

    // mac
    _features = [Features.MACOS];
    Features.GetOperatingSystem().ShouldBe(OSFamily.macOS);

    _features = [Features.MACOS_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.macOS);

    // linux
    _features = [Features.LINUX];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Linux);

    _features = [Features.LINUX_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Linux);

    // freeBSD
    _features = [Features.BSD];
    Features.GetOperatingSystem().ShouldBe(OSFamily.FreeBSD);

    _features = [Features.LINUX_OR_BSD_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.FreeBSD);

    // iOS
    _features = [Features.IOS];
    Features.GetOperatingSystem().ShouldBe(OSFamily.iOS);

    _features = [Features.IOS_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.iOS);

    // android
    _features = [Features.ANDROID];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Android);

    _features = [Features.ANDROID_WEB];
    Features.GetOperatingSystem().ShouldBe(OSFamily.Android);

    // unsupported
    _features = [];
    Should.Throw<PlatformNotSupportedException>(
      () => Features.GetOperatingSystem()
    );
  }

  [Test]
  public void TestPlatform() {
    // web
    _features = [Features.WEB];
    Features.GetPlatform().ShouldBe(Platform.Web);

    // test accessor property
    Features.Platform.ShouldBe(Platform.Web);

    // pc
    _features = [Features.PC];
    Features.GetPlatform().ShouldBe(Platform.Desktop);

    // mobile
    _features = [Features.MOBILE];
    Features.GetPlatform().ShouldBe(Platform.Mobile);

    // console
    _features = [];
    Features.GetPlatform().ShouldBe(Platform.Console);
  }

  [Test]
  public void TestInteractivityMode() {
    // movie
    _features = [Features.MOVIE];
    Features.GetInteractivityMode().ShouldBe(InteractivityMode.MovieMode);

    // test accessor property
    Features.InteractivityMode.ShouldBe(InteractivityMode.MovieMode);

    // realtime
    _features = [];
    Features.GetInteractivityMode().ShouldBe(InteractivityMode.Realtime);
  }

  [Test]
  public void TestBuildType() {
    // debug
    _features = [Features.DEBUG];
    Features.GetBuildType().ShouldBe(BuildType.Debug);

    // test accessor property
    Features.BuildType.ShouldBe(BuildType.Debug);

    // release
    _features = [Features.RELEASE];
    Features.GetBuildType().ShouldBe(BuildType.Release);

    // unsupported
    _features = [];
    Should.Throw<NotSupportedException>(
      () => Features.GetBuildType()
    );
  }

  [Test]
  public void TestToolEnvironment() {
    // editor
    _features = [Features.EDITOR];
    Features.GetToolEnvironment().ShouldBe(ToolEnvironment.Editor);

    // test accessor property
    Features.ToolEnvironment.ShouldBe(ToolEnvironment.Editor);

    // template
    _features = [Features.TEMPLATE];
    Features.GetToolEnvironment().ShouldBe(ToolEnvironment.ExportTemplate);

    // unsupported
    _features = [];
    Should.Throw<NotSupportedException>(
      () => Features.GetToolEnvironment()
    );
  }

  [Test]
  public void TestPrecision() {
    // double
    _features = [Features.DOUBLE];
    Features.GetPrecision().ShouldBe(Precision.Double);

    // test accessor property
    Features.Precision.ShouldBe(Precision.Double);

    // single
    _features = [Features.SINGLE];
    Features.GetPrecision().ShouldBe(Precision.Single);

    // unsupported
    _features = [];
    Should.Throw<NotSupportedException>(
      () => Features.GetPrecision()
    );
  }

  [Test]
  public void TestBitLength() {
    // 32
    _features = [Features.X32];
    Features.GetBitLength().ShouldBe(BitLength.X32);

    // test accessor property
    Features.BitLength.ShouldBe(BitLength.X32);

    // 64
    _features = [Features.X64];
    Features.GetBitLength().ShouldBe(BitLength.X64);

    // unsupported
    _features = [];
    Should.Throw<NotSupportedException>(
      () => Features.GetBitLength()
    );
  }

  [Test]
  public void TestArchitecture() {
    // x86
    _features = [Features.X86];
    Features.GetArchitecture().ShouldBe(Architecture.X86);

    // test accessor property
    Features.Architecture.ShouldBe(Architecture.X86);

    // arm
    _features = [Features.ARM];
    Features.GetArchitecture().ShouldBe(Architecture.Arm);

    // risc-v
    _features = [Features.RISC_V];
    Features.GetArchitecture().ShouldBe(Architecture.RiscV);

    // ppc
    _features = [Features.PPC];
    Features.GetArchitecture().ShouldBe(Architecture.PPC);

    // wasm
    _features = [Features.WASM];
    Features.GetArchitecture().ShouldBe(Architecture.Wasm);

    // unsupported
    _features = [];
    Should.Throw<PlatformNotSupportedException>(
      () => Features.GetArchitecture()
    );
  }

  [Test]
  public void TestTextureCompression() {
    // etc
    _features = [Features.ETC];
    Features.GetTextureCompression().ShouldBe(TextureCompression.Etc);

    // test accessor property
    Features.TextureCompression.ShouldBe(TextureCompression.Etc);

    // etc2
    _features = [Features.ETC2];
    Features.GetTextureCompression().ShouldBe(TextureCompression.Etc2);

    // s3tc
    _features = [Features.S3TC];
    Features.GetTextureCompression().ShouldBe(TextureCompression.S3TC);

    // unsupported
    _features = [];
    Should.Throw<NotSupportedException>(
      () => Features.GetTextureCompression()
    );
  }

  [Test]
  public void PrintFeatureDiagnostics() {
    Features.PrintFeatureDiagnostics();
  }

  [Test]
  public void FakeOperatingSystem() {
    Features.FakeOperatingSystem(OSFamily.Windows);
    Features.OperatingSystem.ShouldBe(OSFamily.Windows);
  }

  [Test]
  public void FakePlatform() {
    Features.FakePlatform(Platform.Web);
    Features.Platform.ShouldBe(Platform.Web);
  }

  [Test]
  public void FakeInteractivityMode() {
    Features.FakeInteractivityMode(InteractivityMode.MovieMode);
    Features.InteractivityMode.ShouldBe(InteractivityMode.MovieMode);
  }

  [Test]
  public void FakeBuildType() {
    Features.FakeBuildType(BuildType.Debug);
    Features.BuildType.ShouldBe(BuildType.Debug);
  }

  [Test]
  public void FakeToolEnvironment() {
    Features.FakeToolEnvironment(ToolEnvironment.Editor);
    Features.ToolEnvironment.ShouldBe(ToolEnvironment.Editor);
  }

  [Test]
  public void FakePrecision() {
    Features.FakePrecision(Precision.Double);
    Features.Precision.ShouldBe(Precision.Double);
  }

  [Test]
  public void FakeBitLength() {
    Features.FakeBitLength(BitLength.X32);
    Features.BitLength.ShouldBe(BitLength.X32);
  }

  [Test]
  public void FakeArchitecture() {
    Features.FakeArchitecture(Architecture.X86);
    Features.Architecture.ShouldBe(Architecture.X86);
  }

  [Test]
  public void FakeTextureCompression() {
    Features.FakeTextureCompression(TextureCompression.Etc);
    Features.TextureCompression.ShouldBe(TextureCompression.Etc);
  }
}
