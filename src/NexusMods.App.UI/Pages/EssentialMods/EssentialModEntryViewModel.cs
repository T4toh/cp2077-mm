using System.Reactive;
using NexusMods.Abstractions.Library;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Networking.NexusWebApi;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Loadouts;
using NexusMods.Sdk.NexusModsApi;
using NexusMods.UI.Sdk;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using R3;
using System.Reactive.Linq;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Paths;
using NexusMods.Sdk.Jobs;
using NexusMods.Abstractions.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Threading.Tasks;
using NexusMods.Abstractions.NexusModsLibrary;

namespace NexusMods.App.UI.Pages.EssentialMods;

public class EssentialModEntryViewModel : AViewModel<IEssentialModEntryViewModel>, IEssentialModEntryViewModel
{
    public string Name { get; }
    public string Description { get; }
    public ModId ModId { get; }

    [Reactive] public EssentialModStatus Status { get; private set; }

    public ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> InstallCommand { get; }

    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly NexusModsLibrary _nexusModsLibrary;
    private readonly IGraphQlClient _graphQlClient;
    private readonly ILibraryService _libraryService;
    private readonly ILoadoutManager _loadoutManager;
    private readonly LoadoutId _loadoutId;
    private readonly NexusModsGameId _nexusModsGameId;
    private readonly TemporaryFileManager _temporaryFileManager;

    public EssentialModEntryViewModel(
        IServiceProvider serviceProvider,
        LoadoutId loadoutId,
        NexusModsGameId nexusModsGameId,
        string name,
        ModId modId,
        string description)
    {
        _serviceProvider = serviceProvider;
        _connection = serviceProvider.GetRequiredService<IConnection>();
        _nexusModsLibrary = serviceProvider.GetRequiredService<NexusModsLibrary>();
        _graphQlClient = serviceProvider.GetRequiredService<IGraphQlClient>();
        _libraryService = serviceProvider.GetRequiredService<ILibraryService>();
        _loadoutManager = serviceProvider.GetRequiredService<ILoadoutManager>();
        _temporaryFileManager = serviceProvider.GetRequiredService<TemporaryFileManager>();
        _loadoutId = loadoutId;
        _nexusModsGameId = nexusModsGameId;

        Name = name;
        ModId = modId;
        Description = description;

        UpdateStatus();

        InstallCommand = ReactiveUI.ReactiveCommand.CreateFromTask(async () =>
        {
            if (Status == EssentialModStatus.Installed) return;

            if (Status == EssentialModStatus.NotDownloaded)
            {
                Status = EssentialModStatus.Downloading;
                try
                {
                    await DownloadAndInstall();
                }
                catch (Exception)
                {
                    Status = EssentialModStatus.NotDownloaded;
                    throw;
                }
            }
            else if (Status == EssentialModStatus.InLibrary)
            {
                Status = EssentialModStatus.Installing;
                try
                {
                    await InstallFromLibrary();
                }
                catch (Exception)
                {
                    Status = EssentialModStatus.InLibrary;
                    throw;
                }
            }

            UpdateStatus();
        });
    }

    private void UpdateStatus()
    {
        var db = _connection.Db;
        
        // Check if installed in loadout
        var isInstalled = LoadoutItem.FindByLoadout(db, _loadoutId)
            .OfTypeLoadoutItemGroup()
            .Any(g => 
            {
                if (!LibraryLinkedLoadoutItem.TryGet(db, g.Id, out var linked)) return false;
                
                if (!NexusModsLibraryItem.TryGet(db, linked.Value.LibraryItemId.Value, out var nItem)) return false;
                return nItem.Value.ModPageMetadata.Uid.ModId == ModId;
            });

        if (isInstalled)
        {
            Status = EssentialModStatus.Installed;
            return;
        }

        // Check if in library
        var isInLibrary = NexusModsLibraryItem.All(db)
            .Any(x => x.ModPageMetadata.Uid.ModId == ModId);

        Status = isInLibrary ? EssentialModStatus.InLibrary : EssentialModStatus.NotDownloaded;
    }

    private async Task DownloadAndInstall()
    {
        // 1. Get mod files
        var filesResult = await _graphQlClient.QueryModFiles(ModId, _nexusModsGameId);
        var files = filesResult.AssertHasData();
        
        // 2. Pick the latest file (heuristic: highest date)
        var mainFile = files.OrderByDescending(f => f.Date).First();
        
        // 3. Download
        await using var tempPath = _temporaryFileManager.CreateFile();
        var modPage = await _nexusModsLibrary.GetOrAddModPage(ModId, _nexusModsGameId);
        
        // Try to parse Uid as long/uint
        var fileIdValue = uint.Parse(mainFile.Uid.Split(':').Last());
        var fileMetadata = await _nexusModsLibrary.GetOrAddFile(FileId.From(fileIdValue), modPage);
        
        var job = await _nexusModsLibrary.CreateDownloadJob(tempPath, fileMetadata);
        var libraryFile = await _libraryService.AddDownload(job);
        
        // 4. Install
        await _loadoutManager.InstallItem(libraryFile.AsLibraryItem(), _loadoutId);
    }

    private async Task InstallFromLibrary()
    {
        var db = _connection.Db;
        var nexusItem = NexusModsLibraryItem.All(db)
            .First(x => x.ModPageMetadata.Uid.ModId == ModId);
            
        await _loadoutManager.InstallItem(nexusItem.AsLibraryItem(), _loadoutId);
    }
}
