using System.Text.RegularExpressions;
using combit.Reporting.Repository;

namespace Alexpuh.ListLabel.JsonFilesRepository;

internal class JsonFilesRepositoryItem
{
    public string? UIName { get; set; }
    
    public string? Descriptor { get; set; }
        
    public string? FolderId { get; set; }
        
    public string? FolderPath { get; set; }
        
    public string? Type { get; set; }
        
    public required bool IsEmpty { get; set; }
        
    public DateTime LastModificationUtc { get; set; }
        
    public string FileName { get; set; }
        
    public JsonFilesRepositoryItem()
    {
        FileName = $"{Guid.NewGuid():N}.data";
    }
        
    
#pragma warning disable SYSLIB1045
    private static readonly Regex RepositoryIdRegex = new("^repository://{(?<guid>.+)}$"); 
#pragma warning restore SYSLIB1045
    private bool TryExtractGuidFromInternalId(string internalId, out Guid id)
    {
        var match = RepositoryIdRegex.Match(internalId);
        if (!match.Success)
        {
            id = Guid.Empty;
            return false;
        }
        var guidStr = match.Groups["guid"].Value;
        id = Guid.Parse(guidStr);
        return true;
    }

    
    public JsonFilesRepositoryItem(RepositoryItem item): this()
    {
        Descriptor = item.Descriptor;
        FolderId = item.FolderId;
        FolderPath = item.FolderPath;
        Type = item.Type;
        LastModificationUtc = item.LastModificationUTC;
        IsEmpty = item.IsEmpty;
        UIName = item.UIName;
        if (TryExtractGuidFromInternalId(item.InternalID, out var id))
        {
            FileName = $"{id:N}.data";    
        }
    }
    
    public RepositoryItem ToRepositoryItem(string id)
    {
        var ret = new RepositoryItem(id, Descriptor, Type, LastModificationUtc)
        {
            FolderId = FolderId,
            FolderPath = FolderPath,
            IsEmpty = IsEmpty,
            UIName = UIName
        };
        return ret;
    } 
}
