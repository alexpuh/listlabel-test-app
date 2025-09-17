using combit.Reporting.Repository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Alexpuh.ListLabel.JsonFilesRepository;

public class JsonFilesRepository: IRepository
{
    private readonly ILogger<JsonFilesRepository> logger;
    private readonly Dictionary<string, JsonFilesRepositoryItem> items;
    private readonly string metadataFileName;
    private readonly string dataDirectoryPath;

    public JsonFilesRepository(ILogger<JsonFilesRepository> logger, JsonFilesRepositoryOptions options): this(logger, options.DirectoryPath)
    {
    }
        
    private JsonFilesRepository(ILogger<JsonFilesRepository> logger, string directoryPath)
    {
        this.logger = logger;
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }
        dataDirectoryPath = directoryPath;
        
        metadataFileName = Path.Combine(dataDirectoryPath, "metadata.json");
        if (File.Exists(metadataFileName))
        {
            var repoData = File.ReadAllText(metadataFileName);
            items = JsonConvert.DeserializeObject<Dictionary<string, JsonFilesRepositoryItem>>(repoData) ?? new Dictionary<string, JsonFilesRepositoryItem>();
        }
        else
        {
            logger.LogDebug("Metadata file not found, create new one: {MetadataFileName}", metadataFileName);
            items = new Dictionary<string, JsonFilesRepositoryItem>();
            SaveMetadata();
        }
    }

    private void SaveMetadata()
    {
        logger.LogDebug("SaveMetadata: {MetadataFileName}", metadataFileName);
        var save = JsonConvert.SerializeObject(items, Formatting.Indented);
        File.WriteAllText(metadataFileName, save);
    }


    public bool ContainsItem(string id)
    {
        logger.LogDebug("ContainsItem: {Id}", id);
        return items.ContainsKey(id);
    }

    public void LoadItem(string id, Stream destinationStream, CancellationToken cancelToken)
    {
        logger.LogDebug("LoadItem: {Id}", id);
        if (!items.TryGetValue(id, out var jsonRepositoryItem))
        {
            logger.LogDebug("Item not found: {Id}", id);
            return;
            // throw new ApplicationException($"Id not found: {id}");    
        }
        var itemDataFileName = Path.Combine(dataDirectoryPath, jsonRepositoryItem.FileName);
        var content = Array.Empty<byte>();        
        if (File.Exists(itemDataFileName))
        {
            content = File.ReadAllBytes(itemDataFileName);
        }
        else
        {
            logger.LogError("File not exists: {ItemDataFileName}", itemDataFileName);
        }
        destinationStream.Write(content, 0, content.Length);
    }

    public IEnumerable<RepositoryItem> GetAllItems()
    {
        var ret = items.Select(i => i.Value.ToRepositoryItem(i.Key));
        return ret;
    }

    public RepositoryItem? GetItem(string id)
    {
        logger.LogDebug("GetItem: {Id}", id);
        return items.TryGetValue(id, out var repositoryItem) ? repositoryItem.ToRepositoryItem(id) : null;
    }

    public void DeleteItem(string id)
    {
        logger.LogDebug("DeleteItem: {Id}", id);
        if (items.Remove(id))
        {
            SaveMetadata();
        }
    }

    public void CreateOrUpdateItem(RepositoryItem item, string importUserData, Stream? sourceStream)
    {
        var streamPresent = sourceStream != null;
        logger.LogDebug("CreateOrUpdateItem, InternalId: {InternalId}, Stream: {Stream}", item.InternalID, streamPresent);
        var found = items.TryGetValue(item.InternalID, out var itemMetadata);
        if (!found)
        {
            itemMetadata = new JsonFilesRepositoryItem(item) {IsEmpty = !streamPresent};
            items[item.InternalID] = itemMetadata;
        }
        else
        {
            itemMetadata!.LastModificationUtc = item.LastModificationUTC;
            itemMetadata.Descriptor = item.Descriptor;
            itemMetadata.IsEmpty = itemMetadata.IsEmpty || !streamPresent;
            itemMetadata.UIName = item.UIName;
        }
        SaveMetadata();

        if (streamPresent)
        {
            using var memStream = new MemoryStream();
            sourceStream!.CopyTo(memStream);
            var fileContent = memStream.ToArray();
            var targetFileName = Path.Combine(dataDirectoryPath, itemMetadata.FileName);
            logger.LogDebug("Write Stream to {FileName}", targetFileName);
            File.WriteAllBytes(targetFileName, fileContent);
        }
    }

    public bool LockItem(string id)
    {
        logger.LogDebug("LockItem: {Id}", id);
        return true;
    }

    public void UnlockItem(string id)
    {
        logger.LogDebug("UnLockItem: {Id}", id);
    }
}