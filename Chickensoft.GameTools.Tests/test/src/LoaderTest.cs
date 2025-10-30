namespace Chickensoft.GameTools.Tests;

using System;
using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class LoaderTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public async Task LoadsResourcesSuccessfully()
  {
    var res1Path = "res://stuff/scene.tscn";
    var res1 = new PackedScene();

    var res2Path = "res://stuff/mesh.tscn";
    var res2 = new Mesh();

    var loader = new Loader();

    PackedScene scene = default!;
    loader.AddJob<PackedScene>(res1Path, (r) => scene = r);

    Mesh mesh = default!;
    loader.AddJob<Mesh>(res2Path, (r) => mesh = r);

    var onStarted = false;
    loader.Started += () => onStarted = true;

    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;


    var progress = -1f;
    loader.Progress += (p) => progress = p;

    loader.Load();

    var isTaskCompleted = false;
    var task = loader.Task;
    var otherTask = task.ContinueWith(
      (_) => isTaskCompleted = true
    );

    progress.ShouldBe(0f);

    onStarted.ShouldBeTrue();

    Loader.LoadThreadedGetStatus = (path, progressArray) =>
    {
      if (path == res1Path)
      {
        progressArray![0] = 0.25f;
      }
      else if (path == res2Path)
      {
        progressArray![0] = 0.5f;
      }
      return ResourceLoader.ThreadLoadStatus.InProgress;
    };

    loader.Update();

    var isCompleted = false;
    loader.Completed += () => isCompleted = true;

    Mathf.IsEqualApprox(progress, (0.5f + 0.25f) / 2f).ShouldBeTrue();
    Mathf.IsEqualApprox(loader.ProgressPercentage, progress).ShouldBeTrue();

    Loader.LoadThreadedGetStatus = (path, progressArray) =>
    {
      if (path == res1Path)
      {
        progressArray![0] = 0.99f;
      }
      else if (path == res2Path)
      {
        progressArray![0] = 0.99f;
      }
      return ResourceLoader.ThreadLoadStatus.Loaded;
    };

    Loader.LoadThreadedGet = (path) =>
    {
      if (path == res1Path)
      {
        return res1;
      }

      return res2;
    };

    isTaskCompleted.ShouldBeFalse();
    loader.IsCompleted.ShouldBeFalse();

    loader.Update();

    Mathf.IsEqualApprox(progress, 1f).ShouldBeTrue();
    Mathf.IsEqualApprox(loader.ProgressPercentage, 1f).ShouldBeTrue();
    isCompleted.ShouldBeTrue();

    // Make sure we got the resources we loaded
    scene.ShouldBeSameAs(res1);
    mesh.ShouldBeSameAs(res2);

    task.IsCompleted.ShouldBeTrue();
    await otherTask;
    isTaskCompleted.ShouldBeTrue();

    loader.IsCompleted.ShouldBeTrue();
  }

  [Test]
  public void EncountersInvalidResourceExceptionWhenLoading()
  {
    var resPath = "res://stuff/scene.tscn";

    var loader = new Loader();

    loader.AddJob<PackedScene>(resPath, (r) => { });

    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;

    Loader.LoadThreadedGetStatus = (path, progressArray) =>
      ResourceLoader.ThreadLoadStatus.InvalidResource;

    loader.Load();
    Should.Throw<InvalidOperationException>(loader.Update);
  }

  [Test]
  public void EncountersFailedExceptionWhenLoading()
  {
    var resPath = "res://stuff/scene.tscn";

    var loader = new Loader();

    loader.AddJob<PackedScene>(resPath, (r) => { });

    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;

    Loader.LoadThreadedGetStatus = (path, progressArray) =>
      ResourceLoader.ThreadLoadStatus.Failed;

    loader.Load();
    Should.Throw<InvalidOperationException>(loader.Update);
  }

  [Test]
  public void InvokeCompletedDoesNothingWithNoSubscriptions()
  {
    var loader = new Loader();

    Should.NotThrow(loader.InvokeCompleted);
  }

  [Test]
  public void DisposeThrowsIfRunning()
  {
    var loader = new Loader();
    loader.AddJob<PackedScene>("res://stuff/scene.tscn", (r) => { });
    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;
    loader.Load();

    Should.Throw<InvalidOperationException>(loader.Dispose);
  }

  [Test]
  public void DisposeDoesNothingIfDisposed()
  {
    var loader = new Loader();

    loader.Dispose();

    Should.NotThrow(loader.Dispose);
  }

  [Test]
  public void AddJobThrowsIfRunning()
  {
    var loader = new Loader();
    loader.AddJob<PackedScene>("res://stuff/scene.tscn", (r) => { });
    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;
    loader.Load();

    Should.Throw<InvalidOperationException>(() =>
      loader.AddJob<PackedScene>("res://stuff/scene.tscn", (r) => { })
    );
  }

  [Test]
  public void LoadDoesNothingIfRunning()
  {
    var loader = new Loader();
    loader.AddJob<PackedScene>("res://stuff/scene.tscn", (r) => { });
    Loader.LoadThreadedRequest =
      (path, typeHint, useSubthreads, cacheMode) => Error.Ok;
    loader.Load();

    Should.NotThrow(loader.Load);
  }

  [Test]
  public void LoadDoesNothingIfDisposed()
  {
    var loader = new Loader();
    loader.Dispose();

    Should.NotThrow(loader.Load);
  }

  [Test]
  public void UpdateDoesNothingIfNotRunning()
  {
    var loader = new Loader();

    Should.NotThrow(loader.Update);
  }

  [Test]
  public void UpdateDoesNothingIfDisposed()
  {
    var loader = new Loader();

    loader.Dispose();

    Should.NotThrow(loader.Update);
  }
}



public class PendingJobTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public void Equality()
  {
    var job = new Loader.PendingJob<PackedScene>(
      "res://stuff/scene.tscn",
      (r) => { },
      "PackedScene",
      true,
      ResourceLoader.CacheMode.Reuse
    );

    job.Equals(null).ShouldBeFalse();
    (job as object).Equals(null).ShouldBeFalse();
  }
}
