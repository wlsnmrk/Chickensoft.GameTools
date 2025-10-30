namespace Chickensoft.GameTools.Tests.Displays;

using Chickensoft.GameTools.Displays;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class WindowSizeInfoTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public void ToStringDescription()
  {
    var sizeInfo = new WindowSizeInfo(
      Size: new Vector2I(1, 2),
      MinSize: new Vector2I(3, 4),
      MaxSize: new Vector2I(5, 6)
    );

    sizeInfo.ToString().ShouldBe("""
    Size: (1, 2),
    MinSize: (3, 4)
    MaxSize: (5, 6)
    """, StringCompareShould.IgnoreLineEndings);
  }
}
