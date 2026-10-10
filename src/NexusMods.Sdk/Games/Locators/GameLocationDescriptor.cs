using System.Collections.Immutable;
using DynamicData.Kernel;
using NexusMods.Paths;

namespace NexusMods.Sdk.Games;

public class GameLocationDescriptor
{
    public LocationId LocationId { get; }
    public AbsolutePath Path { get; }
    public ImmutableArray<LocationId> NestedLocations { get; }
    public Optional<LocationId> TopLevelParent { get; }

    /// <summary>
    /// Files the app manages in this location, relative to <see cref="Path"/>. Null means the whole location is
    /// managed (the game folder). A location with a whitelist is never enumerated and nothing outside the list
    /// is read, written, backed up or deleted.
    /// </summary>
    public ImmutableHashSet<RelativePath>? ManagedFiles { get; }

    public bool IsTopLevel => !TopLevelParent.HasValue;

    public GameLocationDescriptor(LocationId locatorId, AbsolutePath path, ImmutableArray<LocationId> nestedLocations, Optional<LocationId> topLevelParent, ImmutableHashSet<RelativePath>? managedFiles = null)
    {
        LocationId = locatorId;
        Path = path;
        NestedLocations = nestedLocations;
        TopLevelParent = topLevelParent;
        ManagedFiles = managedFiles;
    }
}
