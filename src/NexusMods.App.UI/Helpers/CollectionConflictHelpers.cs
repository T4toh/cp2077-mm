using NexusMods.Abstractions.Collections;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.App.UI.Dialog;
using NexusMods.App.UI.Dialog.Enums;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.Windows;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Loadouts;
using NexusMods.UI.Sdk.Dialog;
using NexusMods.UI.Sdk.Dialog.Enums;

namespace NexusMods.App.UI.Helpers;

/// <summary>
/// Helpers for detecting and warning about conflicting collections in a loadout.
/// </summary>
public static class CollectionConflictHelpers
{
    /// <summary>
    /// Returns the names of active (enabled) Nexus collections in the loadout
    /// that belong to a different collection than the one being installed.
    /// </summary>
    public static string[] GetConflictingCollectionNames(
        IDb db,
        LoadoutId loadoutId,
        CollectionMetadata.ReadOnly currentCollection)
    {
        return NexusCollectionLoadoutGroup.All(db)
            .Where(g =>
            {
                var loadoutItem = g.AsCollectionGroup().AsLoadoutItemGroup().AsLoadoutItem();
                return loadoutItem.LoadoutId == loadoutId
                    && !loadoutItem.IsDisabled
                    && g.CollectionId != currentCollection;
            })
            .Select(g => g.AsCollectionGroup().AsLoadoutItemGroup().AsLoadoutItem().Name)
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Shows a warning dialog if other active collections exist in the loadout.
    /// Returns true if install should proceed (no conflicts or user confirmed).
    /// </summary>
    public static async Task<bool> ConfirmInstallWithConflicts(
        IDb db,
        LoadoutId loadoutId,
        CollectionMetadata.ReadOnly currentCollection,
        IWindowManager windowManager)
    {
        var conflicting = GetConflictingCollectionNames(db, loadoutId, currentCollection);
        if (conflicting.Length == 0) return true;

        var collectionList = string.Join("\n", conflicting.Select(n => $"  • {n}"));
        var message = string.Format(Language.CollectionConflict_Warning_Message, collectionList);

        var dialog = DialogFactory.CreateStandardDialog(
            title: Language.CollectionConflict_Warning_Title,
            new StandardDialogParameters { Text = message },
            buttonDefinitions:
            [
                DialogStandardButtons.Cancel,
                new DialogButtonDefinition(
                    Language.CollectionConflict_Warning_InstallAnyway,
                    ButtonDefinitionId.Accept,
                    ButtonAction.Accept,
                    ButtonStyling.Destructive
                ),
            ]
        );

        var result = await windowManager.ShowDialog(dialog, DialogWindowType.Modal);
        return result.ButtonId == ButtonDefinitionId.Accept;
    }
}
