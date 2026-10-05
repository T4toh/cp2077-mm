-- namespace: NexusMods.Games.FileHashes
CREATE SCHEMA IF NOT EXISTS file_hashes;

-- ENUM of all the store names
CREATE TYPE file_hashes.Stores AS ENUM ('Unknown', 'Steam', 'Manually Added');

-- Find all the steam manifests that match the given game's files, and rank them by the number of files that match
CREATE MACRO file_hashes.resolve_steam_manifests(GameMetadataId) AS TABLE
SELECT ANY_VALUE(steam.depotId) DepotId, COUNT(*) matching_count, ANY_VALUE(steam.AppId) AppId, ANY_VALUE(steam.ManifestId)
FROM MDB_DISKSTATEENTRY() entry
         LEFT JOIN MDB_HASHRELATION(DBName=>"hashes") hashrel on entry.Hash = hashRel.xxHash3
         LEFT JOIN MDB_PATHHASHRELATION(DBName=>"hashes") pathrel on pathrel.Path = entry.Path.Item3 AND pathrel.Hash = hashrel.Id
         LEFT JOIN (SELECT AppId, ManifestId, DepotId, unnest(Files) File FROM MDB_STEAMMANIFEST(DBName=>"hashes")) steam on steam.File = pathrel.Id
WHERE entry.Game = GameMetadataId
GROUP BY steam.ManifestId
ORDER BY COUNT(*) DESC;

-- Find all the depots (LocatorIds) for a given game. This will be the most matching depot for every AppId found in a given game folder
CREATE MACRO file_hashes.resolve_steam_depots(GameMetadataId) AS TABLE 
SELECT arg_max(ManifestId, matching_count) DepotId 
FROM file_hashes.resolve_steam_manifests(GameMetadataId) manifests
GROUP BY manifests.AppId
Having DepotId is not null;
