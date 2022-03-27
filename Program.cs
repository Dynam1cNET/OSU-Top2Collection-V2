using System;
using System.Net;
using System.Diagnostics;
using OsuParsers.Decoders;
using OsuParsers.Database;
using Newtonsoft.Json;

/// Todolist /////////////////////////////////////////////////////////////////////////|---|
/// - Get Top 100 of Player                                                           | ✓ | fetchTopPlay(mode, apikey, targetuser, limit);
/// - Look up Wich map the user Currently has Installed                               | ✓ | OsuDatabase osuDB = DatabaseDecoder.DecodeOsu(@"G:\osu!\osu!.db");
/// - Compare Top 100 with users Installed Beatmaps                                   | ✓ | filterOsuDB(osudb, top);
/// - For every map that was not found make a API request                             | ✓ | fetchUnknownMaps(topmapid, apikey);
/// - From that API Request get MapsetID and MD5 hash                                 | ✓ | -||-
/// - Create new Collection in collection.db                                          | ✓ | addCollection("MyTop100");
/// - Store all Maps in the new Collection                                            | ✓ | addToCollection(collectionDb.Collections.Count - 1, md5list);
/// - Save New Collection as collection.db                                            | ✓ | writeCollection(@"G:\osu!\osu!.db");
/// - Read config from file                                                           | ✓ | parseConfig(@"./top2collection.json");  
/// - make a automatic sync delete and replace old collection with new one            | ✓ | deleteOld(collectionname[i]);
/// - Create a Backup of the old collection.db Called collection.db_backup-DATE       | ✓ | backupCollectionDB();
/// - Delete collection.db                                                            | ✓ | deleteCollectionDB();
/// - Build a HTML File containing map links and OSU!Direct Links                     | ✓ | createMissingMapsHTML(collectionname[i]);
/// //////////////////////////////////////////////////////////////////////////////////|---|

/// Config ////////////////////////////////////////////////
string osupath = string.Empty;                          ///
List<string> mode = new List<string>();                 ///
string apikey = string.Empty;                           ///
List<string> targetuser = new List<string>();           ///
List<string> limit = new List<string>();                ///
List<string> collectionname = new List<string>();       ///
int i = 0;                                              ///
bool startosu = false;                                  ///
bool backup = false;                                    ///
bool generateHTML = false;                              ///
bool automode = false;                                  ///
parseConfig(@"./top2collection.json");                  ///
/// ///////////////////////////////////////////////////////

/// Temp Files and Static Variables ///////////////////////////////////////////////////////////////////
string savepath = @$"{osupath}collection.db";                                                       ///
CollectionDatabase collectionDb = DatabaseDecoder.DecodeCollection(@$"{osupath}collection.db");     ///
OsuDatabase osuDB = DatabaseDecoder.DecodeOsu(@$"{osupath}osu!.db");                                ///
List<int> osudb = osuDB.Beatmaps.Select(map => map.BeatmapId).ToList();                             ///
List<string> md5top = new List<string>();                                                           ///
List<string> missingmaps = new List<string>();                                                      ///
List<string> missingmaps_SETID = new List<string>();                                                ///
string[] top = { };                                                                                 ///
/// ///////////////////////////////////////////////////////////////////////////////////////////////////

/// ////////////////////////////////////////////////////// ///
/// Main loop. Does execute for every userentry in the Config
/// ////////////////////////////////////////////////////// ///
backupCollectionDB();
if (Directory.Exists(@$"{osupath}MissingMaps"))
{
Directory.Delete(@$"{osupath}MissingMaps", true); 
}
foreach (var user in targetuser)
{
    debug("INFO", $"Building Collection {collectionname[i]}.");
    deleteOld(collectionname[i]);
    fetchTopPlay(mode[i], apikey, user, limit[i]);
    filterOsuDB(osudb, top);
    addCollection(collectionname[i]);
    addToCollection(collectionDb.Collections.Count - 1, md5top);
    createMissingMapsHTML(collectionname[i]);
    i++;
    Array.Clear(top);
    md5top.Clear();
    missingmaps.Clear();
    missingmaps_SETID.Clear();
}
deleteCollectionDB();
writeCollection(savepath);
/// ////////////////////////////////////////////////////// ///
/// Creates HTML file with all missing maps
/// ////////////////////////////////////////////////////// ///
void createMissingMapsHTML(string collectionname)
{
    if (missingmaps.Count > 0)
    {
        if (generateHTML)
        {
            debug("WARNING", $"Maps missing in {collectionname} check \"MissingMaps\" folder in your osu! folder!.");
            Directory.CreateDirectory(@$"{osupath}MissingMaps");

            List<string> lines = new List<string>();
            lines.Add("<html>");
            lines.Add("<body>");
            lines.Add("<h1>Default Links:</h1>");
            foreach (var mapid in missingmaps_SETID)
            {
                lines.Add($"<a href=\"https://osu.ppy.sh/beatmapsets/{mapid}/\">{mapid}</a><br>");
            }
            lines.Add("<h1>Osu!direct:</h1>");
            foreach (var mapid in missingmaps)
            {
                lines.Add($"<a href=\"osu://b/{mapid}\">osu://b/{mapid}</a><br>");
            }
            lines.Add("<h1>chimu.moe:</h1>");
            foreach (var mapid in missingmaps_SETID)
            {
                lines.Add($"<a href=\"https://chimu.moe/d/{mapid}\">chimu.moe/d/{mapid}</a><br>");
            }
            lines.Add("</body>");
            lines.Add("</html>");

            File.WriteAllLines(@$"{osupath}MissingMaps/{collectionname}_{DateTime.Now.ToString("dd-MM-yyyy HH_mm_ss")}.html", lines);
        }
    }
}
/// ////////////////////////////////////////////////////// ///
/// Deletes Collection.db
/// ////////////////////////////////////////////////////// ///
void deleteCollectionDB()
{
    File.Delete($@"{osupath}collection.db");
}
/// ////////////////////////////////////////////////////// ///
/// Creates a backup folder in the osu folder and copys 
/// database in to it before starting anything
/// ////////////////////////////////////////////////////// ///
void backupCollectionDB()
{
    if (backup)
    {
        Directory.CreateDirectory(@$"{osupath}collecionBackups");
        File.Copy(@$"{osupath}collection.db", @$"{osupath}collecionBackups/collection.db_{DateTime.Now.ToString("dd-MM-yyyy HH_mm_ss")}");
    }
}
/// ////////////////////////////////////////////////////// ///
/// Deletes old Collentions if already exits
/// ////////////////////////////////////////////////////// ///
void deleteOld(string collectionName)
{
    try
    {
        collectionDb.Collections.RemoveAt(collectionDb.Collections.FindIndex(collection => collection.Name.ToString() == collectionName));
    }
    catch (Exception)
    {

    }
}
/// ////////////////////////////////////////////////////// ///
/// Parses the Config file in the according Varaibles and lists
/// ////////////////////////////////////////////////////// ///
void parseConfig(string cfgpath)
{
    using (StreamReader sr = new StreamReader(cfgpath))
    {
        string line = sr.ReadToEnd();
        List<config>? cfg = JsonConvert.DeserializeObject<List<config>?>(line);

        osupath = cfg[0].Osupath;
        mode = cfg[0].ModeList;
        apikey = cfg[0].Apikey;
        targetuser = cfg[0].TargetsList;
        limit = cfg[0].LimitsList;
        collectionname = cfg[0].CollectionnamesList;
        startosu = cfg[0].Startosuaftersync;
        backup = cfg[0].BackupCollectionsDB;
        generateHTML = cfg[0].GenerateMissingMaps;
        automode = cfg[0].AutomatedMode;
    }
}
/// ////////////////////////////////////////////////////// ///
/// Starts osu when Enabled in Config
/// ////////////////////////////////////////////////////// ///
if (startosu)
{
    Process.Start(@$"{osupath}osu!.exe");
}
/// ////////////////////////////////////////////////////// ///
/// Filters through the osu!db and compares mapid's with
/// the top 100 map id's in order to find wich mape the
/// user dosn't already have.
/// ////////////////////////////////////////////////////// ///
void filterOsuDB(List<int> mapidlist, string[] dbmapid)
{

    Parallel.ForEach(top, topmapid =>
    {
        if (osudb.Find(mapid => mapid.ToString() == topmapid) == 0)
        {
            //debug($"SEARCH: {topmapid}");
            foreach (var map in fetchUnknownMaps(topmapid, apikey))
            {
                md5top.Add(map.MD5hash);
                missingmaps.Add(map.BeatMapId);
                missingmaps_SETID.Add(map.BestmapSetId);
                //debug(map.BestmapSetId);
            }
        }
        else
        {
            //debug($"FOUND: {osudb.Find(mapid => mapid.ToString() == topmapid)}");
            //Console.WriteLine(osudb.IndexOf(Int32.Parse(topmapid)));
            md5top.Add(osuDB.Beatmaps[osudb.IndexOf(Int32.Parse(topmapid))].MD5Hash);
        }
    });
}
/// ////////////////////////////////////////////////////// ///
/// Appends a Collection to the end and gives it a Name
/// ////////////////////////////////////////////////////// ///
void addCollection (string name)
{
    collectionDb.Collections.Add(new OsuParsers.Database.Objects.Collection());
    collectionDb.Collections[collectionDb.Collections.Count - 1].Name = $"{name}";
    collectionDb.CollectionCount = collectionDb.Collections.Count;
}
/// ////////////////////////////////////////////////////// ///
/// Addes a List of MD5 hashes to a Collection
/// ////////////////////////////////////////////////////// ///
void addToCollection (int collectionindex, List<string> md5list)
{
    foreach (string md5 in md5list)
    {
        collectionDb.Collections[collectionindex].MD5Hashes.Add(md5);
    }
    collectionDb.Collections[collectionindex].Count = collectionDb.Collections[collectionindex].MD5Hashes.Count();
}
/// ////////////////////////////////////////////////////// ///
/// print debug line
/// ////////////////////////////////////////////////////// ///
void debug (string type, string message)
{
    switch (type)
    {
        case "ERROR":
            Console.Write(@$"[{DateTime.Now.ToString("HH:mm:ss")}] ");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ERROR");
            Console.ResetColor();
            Console.Write($" {message}");
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("PRESS ANY KEY");
            Console.ResetColor();
            Console.ReadKey();
            break;
        case "WARNING":
            Console.Write(@$"[{DateTime.Now.ToString("HH:mm:ss")}] ");
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("WARN:");
            Console.ResetColor();
            Console.Write($" {message}");
            Console.WriteLine();
            Console.ResetColor();

            break;
        case "INFO":
            Console.Write(@$"[{DateTime.Now.ToString("HH:mm:ss")}] ");
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("INFO:");
            Console.ResetColor();
            Console.Write($" {message}");
            Console.WriteLine();
            Console.ResetColor();
            break;
        default:
            Console.Write(@$"[{DateTime.Now.ToString("HH:mm:ss")}] ");
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("INFO:");
            Console.ResetColor();
            Console.Write($" {message}");
            Console.WriteLine();
            Console.ResetColor();
            break;
    }
    //Console.WriteLine($"DEBUG: {message}");
}
/// ////////////////////////////////////////////////////// ///
/// Writes / Saves Collection
/// ////////////////////////////////////////////////////// ///
void writeCollection (string path)
{
    collectionDb.Save(@$"{path}");
}
/// ////////////////////////////////////////////////////// ///
/// Fetches the API for a users top 100
/// ////////////////////////////////////////////////////// ///
void fetchTopPlay(string mode, string apikey, string user, string limit)
{
    List<topplays>? beatmaplist = (List<topplays>?)JsonConvert.DeserializeObject(Get($"https://osu.ppy.sh/api/get_user_best?k={apikey}&m={mode}&u={user}&limit={limit}"), typeof(List<topplays>));
    beatmaplist?.ToArray();

    top = beatmaplist.Select(map => map.BeatmapId).ToArray();
}
/// ////////////////////////////////////////////////////// ///
/// Fetches uinknown maps by mapid
/// ////////////////////////////////////////////////////// ///
List<missingmaps>? fetchUnknownMaps(string mapid, string apikey)
{
    debug("INFO", $"Collecting Information about missing map {mapid}.");
    List<missingmaps>? beatmaplist = (List<missingmaps>?)JsonConvert.DeserializeObject(Get($"https://osu.ppy.sh/api/get_beatmaps?k={apikey}&b={mapid}"), typeof(List<missingmaps>));
    return beatmaplist;
}
/// ////////////////////////////////////////////////////// ///
/// Make Simple API Request non Async
/// ////////////////////////////////////////////////////// ///
string Get(string uri)
{
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
    using (Stream stream = response.GetResponseStream())
    using (StreamReader reader = new StreamReader(stream))
    {
        return reader.ReadToEnd();
    }
}
/// ////////////////////////////////////////////////////// ///
/// Data Class for the /api/get_user_best API
/// ////////////////////////////////////////////////////// ///
[System.Serializable]
public class topplays
{
    [JsonProperty("beatmap_id")] private string? _beatmapId;
    [JsonProperty("countmiss")] private string? _countmiss;
    [JsonProperty("perfect")] private string? _perfect;
    [JsonProperty("rank")] private string? _rank;

    public string? BeatmapId
    {
        get { return _beatmapId; }
        set { _beatmapId = value; }
    }

    public string? Countmiss
    {
        get { return _countmiss; }
        set { _countmiss = value; }
    }

    public string? Perfect
    {
        get { return _perfect; }
        set { _perfect = value; }
    }

    public string? Rank
    {
        get { return _rank; }
        set { _rank = value; }
    }

}
/// ////////////////////////////////////////////////////// ///
/// Data Class for the /api/get_beatmaps API 
/// (searching missing maps)
/// ////////////////////////////////////////////////////// ///
[System.Serializable]
public class missingmaps
{
    [JsonProperty("beatmapset_id")] private string? _beatmapset_id;
    [JsonProperty("beatmap_id")] private string? _beatmap_id;
    [JsonProperty("file_md5")] private string? _file_md5;

    public string? BestmapSetId
    {
        get { return _beatmapset_id; }
        set { _beatmapset_id = value; }
    }

    public string? BeatMapId
    {
        get { return _beatmap_id; }
        set { _beatmap_id = value; }
    }

    public string? MD5hash
    {
        get { return _file_md5; }
        set { _file_md5 = value; }
    }
}
/// ////////////////////////////////////////////////////// ///
/// Data Class for the Config file
/// ////////////////////////////////////////////////////// ///
[System.Serializable]
public class config
{
    [JsonProperty("osupath")] private string? _osupath;
    [JsonProperty("mode")] private List<string>? _modeList;
    [JsonProperty("apikey")] private string? _apikey;
    [JsonProperty("targets")] private List<string>? _targetsList;
    [JsonProperty("limits")] private List<string>? _limitsList;
    [JsonProperty("collectionnames")] private List<string>? _collectionnamesList;
    [JsonProperty("backupCollectionsDB")] private bool _backupCollectionsDB;
    [JsonProperty("startosuaftersync")] private bool _startosuaftersync;
    [JsonProperty("generateMissingMaps")] private bool _generateMissingMaps;
    [JsonProperty("automatedmode")] private bool _automatedmode;
    public string? Osupath
    {
        get { return _osupath; }
        set { _osupath = value; }
    }
    public List<string>? ModeList
    {
        get { return _modeList; }
        set { _modeList = value; }
    }
    public string? Apikey
    {
        get { return _apikey; }
        set { _apikey = value; }
    }
    public List<string>? TargetsList
    {
        get { return _targetsList; }
        set { _targetsList = value; }
    }
    public List<string>? LimitsList
    {
        get { return _limitsList; }
        set { _limitsList = value; }
    }
    public List<string>? CollectionnamesList
    {
        get { return _collectionnamesList; }
        set { _collectionnamesList = value; }
    }
    public bool BackupCollectionsDB
    {
        get { return _backupCollectionsDB; }
        set { _backupCollectionsDB = value; }
    }
    public bool Startosuaftersync
    {
        get { return _startosuaftersync; }
        set { _startosuaftersync = value; }
    }
    public bool GenerateMissingMaps
    {
        get { return _generateMissingMaps; }
        set { _generateMissingMaps = value; }
    }
    public bool AutomatedMode
    {
        get { return _automatedmode; }
        set { _automatedmode = value; }
    }
}