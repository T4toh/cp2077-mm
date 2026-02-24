using DynamicData.Kernel;
using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;
using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.Sdk.NexusModsApi;

namespace NexusMods.Abstractions.NexusWebApi;

/// <summary>
/// Consolidates URL building for links that point to nexusmods.com.
/// Anything that exposes a URL to the user that points to Nexus Mods should use this class.
/// </summary>
[PublicAPI]
public static class NexusModsUrlBuilder
{
    private const string BaseUrl = "https://www.nexusmods.com";
    private const string UsersBaseUrl = "https://users.nexusmods.com";
    private const string ParameterValueSource = "nexusmodsapp";

    private const string ParameterNameCampaign = "mtm_campaign";
    private const string ParameterNameKeyword  = "mtm_keyword";
    private const string ParameterNameMedium   = "mtm_medium";
    private const string ParameterNameSource   = "mtm_source";
    private const string ParameterNameContent  = "mtm_content";
    private const string ParameterNameGroup    = "mtm_group";

    /// <summary>Campaign value for mod updates.</summary>
    public const string CampaignUpdates = "updates";

    /// <summary>Campaign value for collections.</summary>
    public const string CampaignCollections = "collections";

    /// <summary>Campaign value for Nexus Mods Premium.</summary>
    public const string CampaignPremium = "premium";

    /// <summary>Campaign value for diagnostics.</summary>
    public const string CampaignDiagnostics = "diagnostics";

    /// <summary>Creates a URI with optional tracking parameters.</summary>
    public static Uri CreateUri(string baseUrl, string? source = ParameterValueSource, string? campaign = null, string? medium = null)
    {
        var parameters = new Dictionary<string, string?>
        {
            { ParameterNameSource, source },
            { ParameterNameCampaign, campaign },
            { ParameterNameMedium, medium },
        };

        var updated = QueryHelpers.AddQueryString(baseUrl, parameters);
        return new Uri(updated);
    }

    /// <summary>Uri for the user settings page.</summary>
    public static readonly Uri UserSettingsUri = CreateUri(UsersBaseUrl);

    /// <summary>Returns a URI for a user profile.</summary>
    public static Uri GetProfileUri(UserId userId, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/users/{userId}";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for a game page.</summary>
    public static Uri GetGameUri(GameDomain gameDomain, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/games/{gameDomain}";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for a mod page.</summary>
    public static Uri GetModUri(GameDomain gameDomain, ModId modId, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/{gameDomain}/mods/{modId}";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for a file download page.</summary>
    public static Uri GetFileDownloadUri(GameDomain gameDomain, ModId modId, FileId fileId, bool useNxmLink, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/{gameDomain}/mods/{modId}?tab=files&file_id={fileId}&nmm={Convert.ToInt32(useNxmLink)}";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for a game's browse collections page.</summary>
    public static Uri GetBrowseCollectionsUri(GameDomain gameDomain, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/games/{gameDomain}/collections";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for a collection page.</summary>
    public static Uri GetCollectionUri(GameDomain gameDomain, CollectionSlug collectionSlug, Optional<RevisionNumber> revisionNumber, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/games/{gameDomain}/collections/{collectionSlug}{(revisionNumber.HasValue ? $"/revisions/{revisionNumber.Value}" : string.Empty)}";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for the bugs page of a collection.</summary>
    public static Uri GetCollectionBugsUri(GameDomain gameDomain, CollectionSlug collectionSlug, Optional<RevisionNumber> revisionNumber, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/games/{gameDomain}/collections/{collectionSlug}{(revisionNumber.HasValue ? $"/revisions/{revisionNumber.Value}" : string.Empty)}/bugs";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Returns a URI for the changelog page of a collection.</summary>
    public static Uri GetCollectionChangelogUri(GameDomain gameDomain, CollectionSlug collectionSlug, Optional<RevisionNumber> revisionNumber, string? source = ParameterValueSource, string? campaign = null)
    {
        var url = $"{BaseUrl}/games/{gameDomain}/collections/{collectionSlug}{(revisionNumber.HasValue ? $"/revisions/{revisionNumber.Value}" : string.Empty)}/changelog";
        return CreateUri(url, source: source, campaign: campaign);
    }

    /// <summary>Uri for the premium benefits page.</summary>
    public static readonly Uri LearnAboutPremiumUri = CreateUri($"{BaseUrl}/premium", campaign: CampaignPremium);

    /// <summary>Uri for the upgrade to premium page.</summary>
    public static readonly Uri UpgradeToPremiumUri = CreateUri($"{UsersBaseUrl}/account/billing/premium", campaign: CampaignPremium);
}
