using NexusMods.Abstractions.Loadouts;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.ElementComparers;

namespace NexusMods.DataModel.SchemaVersions.Migrations;

/// <summary>
/// Fixes items that were incorrectly disabled at the individual file level by CyberpunkDeepCleanTool.
/// The DeepClean tool used to disable every LoadoutItem (including individual files), but only
/// group-level Disabled markers should be used. Files whose parent group is enabled but the file
/// itself has Disabled=True are re-enabled here.
/// </summary>
internal class _0010_FixDeepCleanDisabledItems : ITransactionalMigration
{
    public static (MigrationId Id, string Name) IdAndName { get; } = MigrationId.ParseNameAndId(nameof(_0010_FixDeepCleanDisabledItems));

    public Task Prepare(IDb db) => Task.CompletedTask;

    public void Migrate(ITransaction tx, IDb db)
    {
        foreach (var item in LoadoutItemWithTargetPath.All(db))
        {
            var li = item.AsLoadoutItem();
            if (!li.Contains(LoadoutItem.Disabled)) continue;

            // Only fix items whose parent group is NOT disabled
            if (!li.Contains(LoadoutItem.Parent)) continue;
            var parentId = li.ParentId;
            if (!LoadoutItemGroup.TryGet(db, parentId, out var parentGroup)) continue;
            if (parentGroup.Value.AsLoadoutItem().Contains(LoadoutItem.Disabled)) continue;

            // Parent group is enabled but file is individually disabled → fix it
            tx.Retract(item.Id, LoadoutItem.Disabled, Null.Instance);
        }
    }
}
