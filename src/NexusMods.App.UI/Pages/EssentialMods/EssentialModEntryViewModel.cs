using System.Reactive;
using System.Reactive.Disposables;
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
using NexusMods.Networking.GitHub;
using DynamicData.Kernel;
using Microsoft.Extensions.Logging;
using NexusMods.Sdk.FileStore;

namespace NexusMods.App.UI.Pages.EssentialMods;

public class EssentialModEntryViewModel : AViewModel<IEssentialModEntryViewModel>, IEssentialModEntryViewModel, IActivatableViewModel
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
    private readonly IGitHubApi _gitHubApi;
    private readonly LoadoutId _loadoutId;
    private readonly NexusModsGameId _nexusModsGameId;
    private readonly TemporaryFileManager _temporaryFileManager;
    private readonly string _gitHubOrg;
    private readonly string _gitHubRepo;
    private readonly IMessageBus _messageBus;
    private readonly IFileStore _fileStore;
    private readonly ILogger<EssentialModEntryViewModel> _logger;

    public EssentialModEntryViewModel(
        IServiceProvider serviceProvider,
        LoadoutId loadoutId,
        NexusModsGameId nexusModsGameId,
        string name,
        ModId modId,
        string description,
        string gitHubOrg,
        string gitHubRepo)
    {
        _serviceProvider = serviceProvider;
        _connection = serviceProvider.GetRequiredService<IConnection>();
        _nexusModsLibrary = serviceProvider.GetRequiredService<NexusModsLibrary>();
        _graphQlClient = serviceProvider.GetRequiredService<IGraphQlClient>();
        _libraryService = serviceProvider.GetRequiredService<ILibraryService>();
        _loadoutManager = serviceProvider.GetRequiredService<ILoadoutManager>();
        _gitHubApi = serviceProvider.GetRequiredService<IGitHubApi>();
        _temporaryFileManager = serviceProvider.GetRequiredService<TemporaryFileManager>();
        _messageBus = serviceProvider.GetRequiredService<IMessageBus>();
        _fileStore = serviceProvider.GetRequiredService<IFileStore>();
        _logger = serviceProvider.GetRequiredService<ILogger<EssentialModEntryViewModel>>();
        _loadoutId = loadoutId;
        _nexusModsGameId = nexusModsGameId;
        _gitHubOrg = gitHubOrg;
        _gitHubRepo = gitHubRepo;

        Name = name;
        ModId = modId;
        Description = description;

        this.WhenActivated(d => 
        {
            // Re-query status on activation so state is fresh after navigating away and back
            UpdateStatus();

            LoadoutItem.ObserveAll(_connection)
                .Subscribe(_ => UpdateStatus())
                .DisposeWith(d);

            NexusModsLibraryItem.ObserveAll(_connection)
                .Subscribe(_ => UpdateStatus())
                .DisposeWith(d);
        });

        UpdateStatus();

        InstallCommand = ReactiveUI.ReactiveCommand.CreateFromTask(async () =>
        {
            if (Status == EssentialModStatus.Installed) return;

            // Check if already in loadout but disabled
            var db = _connection.Db;
            var existingDisabledItem = LoadoutItem.FindByLoadout(db, _loadoutId)
                .OfTypeLoadoutItemGroup()
                .FirstOrOptional(g => 
                {
                    if (!g.Contains(LoadoutItem.Disabled)) return false;
                    if (!LibraryLinkedLoadoutItem.TryGet(db, g.Id, out var linked)) return false;
                    if (!NexusModsLibraryItem.TryGet(db, linked.Value.LibraryItemId.Value, out var nItem)) return false;
                    return nItem.Value.ModPageMetadata.Uid.ModId == ModId;
                });

            if (existingDisabledItem.HasValue)
            {
                Status = EssentialModStatus.Installing;
                using var tx = _connection.BeginTransaction();
                tx.Retract(existingDisabledItem.Value.Id, LoadoutItem.Disabled, NexusMods.MnemonicDB.Abstractions.ElementComparers.Null.Instance);
                await tx.Commit();
                UpdateStatus();
                _messageBus.SendMessage(System.Reactive.Unit.Default); // Signal refresh to the rest of the app
                return;
            }

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
        
        // Check if installed in loadout AND not disabled
        var isInstalled = LoadoutItem.FindByLoadout(db, _loadoutId)
            .OfTypeLoadoutItemGroup()
            .Any(g => 
            {
                if (g.Contains(LoadoutItem.Disabled)) return false;
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
        // 1. Get mod files from Nexus to have metadata even if we download from elsewhere
        var filesResult = await _graphQlClient.QueryModFiles(ModId, _nexusModsGameId);
        var files = filesResult.AssertHasData();
        
        // 2. Pick the latest file (heuristic: highest date)
        var mainFile = files.OrderByDescending(f => f.Date).First();
        var fileId = FileUid.FromV2Api(mainFile.Uid).FileId;
        var modPage = await _nexusModsLibrary.GetOrAddModPage(ModId, _nexusModsGameId);
        var fileMetadata = await _nexusModsLibrary.GetOrAddFile(fileId, modPage);

        try
        {
            await DownloadAndInstallFromNexus(fileMetadata);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            await DownloadFromGitHub(fileMetadata);
        }
    }

    private async Task DownloadAndInstallFromNexus(NexusModsFileMetadata.ReadOnly fileMetadata)
    {
        await using var tempPath = _temporaryFileManager.CreateFile();
        var job = await _nexusModsLibrary.CreateDownloadJob(tempPath, fileMetadata);
        var libraryFile = await _libraryService.AddDownload(job);
        
        await _loadoutManager.InstallItem(libraryFile.AsLibraryItem(), _loadoutId);
    }

    private async Task DownloadFromGitHub(NexusModsFileMetadata.ReadOnly fileMetadata)
    {
        var release = await _gitHubApi.FetchLatestRelease(_gitHubOrg, _gitHubRepo);
        if (release is null || release.Assets.Count == 0)
            throw new InvalidOperationException($"No release or assets found for GitHub repository {_gitHubOrg}/{_gitHubRepo}");

        // Heuristic: pick the first asset that is a zip or has no extension
        var asset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) 
                    ?? release.Assets.First();

        await using var tempPath = _temporaryFileManager.CreateFile();
        var uri = new Uri(asset.BrowserDownloadUrl);
        var downloadPage = new Uri($"https://github.com/{_gitHubOrg}/{_gitHubRepo}/releases/tag/{release.TagName}");
        
        var httpJob = NexusMods.Networking.HttpDownloader.HttpDownloadJob.Create(_serviceProvider, uri, downloadPage, tempPath);
        var nexusJob = NexusMods.Networking.NexusWebApi.NexusModsDownloadJob.Create(_serviceProvider, httpJob, fileMetadata);
        var libraryFile = await _libraryService.AddDownload(nexusJob);

        await _loadoutManager.InstallItem(libraryFile.AsLibraryItem(), _loadoutId);
    }

    private async Task InstallFromLibrary()
    {
        var db = _connection.Db;
        var nexusItem = NexusModsLibraryItem.All(db)
            .First(x => x.ModPageMetadata.Uid.ModId == ModId);

        // Verify the file contents are actually backed up in the store.
        // If BackupFiles was never called (e.g. due to a prior crash) the loadout
        // will be created but no files will ever be extracted to disk ("Unable to extract").
        if (!await AreLibraryFilesInStore(db, nexusItem))
        {
            _logger.LogWarning(
                "Essential mod {Name} (ModId={ModId}) is in library but files are missing from store; re-downloading",
                Name, ModId);
            try
            {
                await DownloadAndInstallFromNexus(nexusItem.FileMetadata);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await DownloadFromGitHub(nexusItem.FileMetadata);
            }
            return;
        }

        await _loadoutManager.InstallItem(nexusItem.AsLibraryItem(), _loadoutId);
    }

    private async Task<bool> AreLibraryFilesInStore(IDb db, NexusModsLibraryItem.ReadOnly nexusItem)
    {
        var libraryFile = LibraryFile.Load(db, nexusItem.Id);
        if (!libraryFile.IsValid()) return false;

        if (libraryFile.TryGetAsLibraryArchive(out var archive))
        {
            var children = archive.Children.ToList();
            if (children.Count == 0) return false;
            foreach (var child in children)
            {
                if (!await _fileStore.HaveFile(child.AsLibraryFile().Hash))
                    return false;
            }
            return true;
        }

        // Single (non-archive) file: check its own hash
        return await _fileStore.HaveFile(libraryFile.Hash);
    }
}
