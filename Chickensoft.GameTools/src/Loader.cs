namespace Chickensoft.GameTools;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

using ThreadLoadStatus = Godot.ResourceLoader.ThreadLoadStatus;
using CacheMode = Godot.ResourceLoader.CacheMode;

/// <summary>
/// A wrapper around Godot's own <see cref="ResourceLoader" /> which makes it
/// really simple to load multiple resources at once while tracking progress
/// and completion.
/// </summary>
public interface ILoader : IDisposable {
  /// <summary>Event invoked when the loader begins loading resources.</summary>
  event Action? Started;

  /// <summary>Event invoked when the loader's progress changes.</summary>
  event Action<float>? Progress;

  /// <summary>
  /// Event invoked when the loader completes loading resources.
  /// </summary>
  event Action? Completed;

  /// <summary>
  /// Progress of the loader as a percentage from 0 to 1.
  /// </summary>
  float ProgressPercentage { get; }

  /// <summary>
  /// Asynchronous task that completes when the loader completes loading
  /// resources (or encounters an error).
  /// </summary>
  Task Task { get; }

  /// <summary>
  /// True if the loader has completed loading all resources.
  /// </summary>
  bool IsCompleted { get; }

  /// <summary>
  /// Adds a loading job to the loader. Jobs are not run until you call
  /// <see cref="Load" />.
  /// </summary>
  /// <typeparam name="T">Type of resource to load.</typeparam>
  /// <param name="resourcePath">Path to the resource.</param>
  /// <param name="onComplete">Callback invoked with the resource once it has
  /// been loaded. Typically, you want to save the value to a field or property
  /// so you can use it.</param>
  /// <param name="typeHint">Godot resource type hint.</param>
  /// <param name="useSubthreads">True if Godot should use subthreads when
  /// loading complex resources. This is false by default, as subthreads can
  /// experience issues.</param>
  /// <param name="cacheMode">Godot resource loader cache mode.</param>
  void AddJob<T>(
    string resourcePath,
    Action<T> onComplete,
    string typeHint = "",
    bool useSubthreads = false,
    CacheMode cacheMode = CacheMode.Reuse
  ) where T : Resource;

  /// <summary>
  /// Begin loading all the resources from the jobs added via
  /// <see cref="AddJob" />. Be sure to call <see cref="Update" />
  /// every frame to track progress and ensure the events are invoked.
  /// </summary>
  void Load();

  /// <summary>
  /// Updates the loader. Call this every frame to track progress and ensure
  /// the events are invoked.
  /// </summary>
  void Update();
}

/// <summary><inheritdoc cref="ILoader" path="/summary"/></summary>
public sealed class Loader : ILoader, IDisposable {
  internal abstract class PendingJob(
    Action<object> internalOnComplete,
    string resourcePath,
    string typeHint,
    bool useSubthreads,
    CacheMode cacheMode
  ) : IEquatable<PendingJob> {
    public Action<object> InternalOnComplete { get; } = internalOnComplete;
    public string ResourcePath { get; } = resourcePath;
    public string TypeHint { get; } = typeHint;
    public bool UseSubthreads { get; } = useSubthreads;
    public CacheMode CacheMode { get; } = cacheMode;
    public float Progress { get; set; }

    public bool Equals(PendingJob? other) =>
      other?.ResourcePath == ResourcePath;

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => ResourcePath.GetHashCode();
  }

  internal sealed class PendingJob<T>(
    string resourcePath,
    Action<T> onComplete,
    string typeHint,
    bool useSubthreads,
    CacheMode cacheMode
  ) : PendingJob(
    (obj) => onComplete((T)obj),
    resourcePath,
    typeHint,
    useSubthreads,
    cacheMode
  );

  /// <inheritdoc />
  public event Action? Started;
  /// <inheritdoc />
  public event Action<float>? Progress;
  /// <inheritdoc />
  public event Action? Completed;
  /// <inheritdoc />
  public float ProgressPercentage { get; private set; }
  /// <inheritdoc />
  public Task Task => _tcs.Task;

  /// <inheritdoc />
  public bool IsCompleted => Task.IsCompleted;

  private bool _isRunning;
  private bool _isDisposed;
  private readonly HashSet<PendingJob> _pendingJobs = [];
  // Used for tracking completed jobs during a single update to avoid allocs
  private readonly HashSet<PendingJob> _completedJobs = [];
  private readonly Godot.Collections.Array _progressArray = [0f];
  private TaskCompletionSource<object> _tcs = new();

  private static readonly object _taskValue = new();

  #region Shims for Testing
  internal delegate Error LoadThreadedRequestDelegate(
    string path,
    string typeHint = "",
    bool useSubThreads = false,
    CacheMode cacheMode = CacheMode.Reuse
  );
  internal static LoadThreadedRequestDelegate
    LoadThreadedRequest { get; set; } = ResourceLoader.LoadThreadedRequest;

  internal delegate ThreadLoadStatus LoadThreadedGetStatusDelegate(
      string path, Godot.Collections.Array? progress = null
    );
  internal static LoadThreadedGetStatusDelegate
    LoadThreadedGetStatus { get; set; } =
      LoadThreadedGetStatus = ResourceLoader.LoadThreadedGetStatus;

  internal delegate Resource LoadThreadedGetDelegate(string path);
  internal static LoadThreadedGetDelegate
    LoadThreadedGet { get; set; } = ResourceLoader.LoadThreadedGet;
  #endregion Shims For Testing

  /// <inheritdoc />
  public void AddJob<T>(
    string resourcePath,
    Action<T> onComplete,
    string typeHint = "",
    bool useSubthreads = false,
    CacheMode cacheMode = CacheMode.Reuse
  ) where T : Resource {
    ObjectDisposedException.ThrowIf(
      _isDisposed, "Cannot add jobs once disposed."
    );

    if (_isRunning) {
      throw new InvalidOperationException("Cannot add jobs while running.");
    }

    _pendingJobs.Add(
      new PendingJob<T>(
        resourcePath, onComplete, typeHint, useSubthreads, cacheMode
      )
    );
  }

  /// <inheritdoc />
  public void Load() {
    if (_isRunning || _isDisposed) { return; }

    _isRunning = true;

    _tcs = new();

    foreach (var pendingJob in _pendingJobs) {
      LoadThreadedRequest(
        pendingJob.ResourcePath,
        pendingJob.TypeHint,
        pendingJob.UseSubthreads,
        pendingJob.CacheMode
      );
    }

    InvokeStarted();
    UpdateAndInvokeProgress(0f);
  }

  /// <inheritdoc />
  public void Update() {
    if (_isDisposed || !_isRunning) { return; }

    UpdateProgress();
  }

  /// <inheritdoc />
  public void Dispose() {
    if (_isRunning) {
      throw new InvalidOperationException("Cannot dispose while running.");
    }

    if (_isDisposed) {
      return;
    }

    ProgressPercentage = 0f;
    _pendingJobs.Clear();
    _completedJobs.Clear();
    _progressArray.Dispose();
    GC.SuppressFinalize(this);
    _isDisposed = true;
  }

  private void UpdateProgress() {
    _completedJobs.Clear();

    var hasProgressChanged = false;

    foreach (var pendingJob in _pendingJobs) {
      var status = LoadThreadedGetStatus(
        pendingJob.ResourcePath, _progressArray
      );
      var progress = _progressArray[0].AsSingle();

      if (!Mathf.IsEqualApprox(progress, pendingJob.Progress)) {
        pendingJob.Progress = progress;
        hasProgressChanged = true;
      }

      if (status == ThreadLoadStatus.InvalidResource) {
        var e = new InvalidOperationException(
          $"Failed to load resource '{pendingJob.ResourcePath}' due to an " +
          "invalid resource error."
        );
        _tcs.SetException(e);
        throw e;
      }

      if (status == ThreadLoadStatus.Failed) {
        var e = new InvalidOperationException(
          $"Failed to load resource '{pendingJob.ResourcePath}'."
        );
        _tcs.SetException(e);
        throw e;
      }

      if (status == ThreadLoadStatus.Loaded) {
        _completedJobs.Add(pendingJob);
      }
    }

    if (hasProgressChanged) {
      ProgressChanged();
    }

    foreach (var job in _completedJobs) {
      CompleteJob(job);
    }

    _completedJobs.Clear();
  }

  private void ProgressChanged() {
    var totalProgress = 0f;

    foreach (var pendingJob in _pendingJobs) {
      totalProgress += pendingJob.Progress;
    }

    totalProgress /= _pendingJobs.Count;

    UpdateAndInvokeProgress(totalProgress);
  }

  private void CompleteJob(PendingJob pendingJob) {
    _pendingJobs.Remove(pendingJob);

    var result = LoadThreadedGet(pendingJob.ResourcePath);

    pendingJob.InternalOnComplete(result);

    if (_pendingJobs.Count == 0) {
      // Done with all jobs.
      UpdateAndInvokeProgress(1f);
      _isRunning = false;
      _tcs.SetResult(_taskValue);
      InvokeCompleted();
    }
  }

  internal void InvokeStarted() => Started?.Invoke();
  internal void UpdateAndInvokeProgress(float progress) {
    ProgressPercentage = progress;
    Progress?.Invoke(progress);
  }
  internal void InvokeCompleted() => Completed?.Invoke();
}
