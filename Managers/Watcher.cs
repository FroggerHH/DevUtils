#nullable enable
using BepInEx;
using System.IO;

namespace Managers;

// A simplified wrapper for FileSystemWatcher, eats duplicate fsw events
public class Watcher
{
    public const long consumeThreshold = 100000; // 10ms

    public event Action<object, FileSystemEventArgs>? FileChanged;

    public bool EnableRaisingEvents
    {
        get => fileSystemWatcher.EnableRaisingEvents;
        set => fileSystemWatcher.EnableRaisingEvents = value;
    }

    private FileSystemWatcher fileSystemWatcher;
    private DateTime lastRead = DateTime.MinValue;
    private WatcherChangeTypes lastChange = WatcherChangeTypes.All;

    public Watcher(string path, string filter)
    {
        if (path == null) { throw new ArgumentNullException("path"); }

        if (filter == null) { throw new ArgumentNullException("filter"); }

        Debug($"Watcher created for {path}, {filter}");

        fileSystemWatcher = new FileSystemWatcher(path, filter);
        fileSystemWatcher.Changed += OnAnyFilesystemEvent;
        fileSystemWatcher.Created += OnAnyFilesystemEvent;
        fileSystemWatcher.Deleted += OnAnyFilesystemEvent;
        fileSystemWatcher.Renamed += OnAnyFilesystemEvent;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private void OnAnyFilesystemEvent(object sender, FileSystemEventArgs args)
    {
        var lastWriteTime = File.GetLastWriteTime(args.FullPath);

        if (lastWriteTime.Ticks - lastRead.Ticks > consumeThreshold || lastChange != args.ChangeType)
        {
            lastRead = lastWriteTime;
            lastChange = args.ChangeType;

            Debug($"OnAnyFilesystemEvent triggered: {args.Name}, {args.ChangeType}");
            FileChanged?.Invoke(sender, args);
        } else Debug($"Consuming duplicate FileSystemEvent: {args.Name}, {args.ChangeType}");
    }
}