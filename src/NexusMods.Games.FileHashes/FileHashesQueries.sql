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

-- gets all the loadouts, locatorids, and stores
CREATE MACRO file_hashes.loadout_locatorids(db) AS TABLE
SELECT install.Store::file_hashes.Stores Store, loadout.id Loadout, unnest(locatorIds) AS LocatorId
FROM MDB_LOADOUT(Db=>db) loadout
         LEFT JOIN MDB_GAMEINSTALLMETADATA(Db=>db) install on loadout.Installation = install.id;  
    
-- gets all the paths and hashes of game files for steam loadouts
CREATE OR REPLACE MACRO file_hashes.steam_loadout_files(db) AS TABLE
SELECT files.Loadout, files.FileId PathId FROM
    (SELECT Loadout, ManifestId, unnest(Files) FileId
     FROM file_hashes.loadout_locatorids(db) locators
              LEFT JOIN MDB_STEAMMANIFEST(DbName=>'hashes') manifest ON manifest.ManifestId = locators.LocatorID::UBIGINT
     WHERE locators.Store = 'Steam') files;


-- gets all the paths and hashes for game files in loadouts
CREATE MACRO file_hashes.loadout_files(db) AS TABLE
WITH
       relations AS (SELECT pathRel.Id, pathRel.Path, hashRel.xxHash3 Hash, hashRel.Size
                  FROM MDB_PathHashRelation(DBName=>"hashes") pathRel
                  INNER JOIN MDB_hashrelation(DBName=>"hashes") hashRel ON pathRel.Hash = hashRel.Id),
       files AS (SELECT Loadout, PathId FROM file_hashes.steam_loadout_files(db))
SELECT files.Loadout, relations.Path, relations.Hash, relations.Size FROM files
INNER JOIN relations ON files.PathId = relations.Id;

       
       
