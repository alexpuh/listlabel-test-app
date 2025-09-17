using System.IO;
using System.Text.RegularExpressions;
using combit.Reporting;
using combit.Reporting.Repository;
using Microsoft.Extensions.Logging;

namespace listlabel_test_app;

public class App(IRepository repository, ILogger<App> logger)
{
    private readonly Lazy<ListLabel> listLabel = new(CreateLicensed);

    public string CreateList(string displayName)
    {
        using var util = new RepositoryImportUtil(repository);
        var templateId = util.CreateNewProject(LlProject.List, displayName, null);
        return templateId;
    }

    public string CreateLabel(string displayName)
    {
        using var util = new RepositoryImportUtil(repository);
        var templateId = util.CreateNewProject(LlProject.Label, displayName, null);
        return templateId;
    }
    
    public string Import(string filePath, string displayName)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        var ll = listLabel.Value;
        using var util = new RepositoryImportUtil(repository);
        var templateId = util.ImportProjectFile(ll, filePath);
        util.SetItemUIName(templateId, displayName);
        
        return templateId;
    }

    public bool TryGetItemByUiName(string uiName, out string? id)
    {
        var found = repository.GetAllItems().FirstOrDefault(ri =>
        {
            var ruiName = RepositoryItemDescriptor.LoadFromDescriptorString(ri.Descriptor).GetUIName(0);
            return ruiName == uiName;
        });
        if (found != null)
        {
            id = found.InternalID;
            return true;
        }
        id = null;
        return false;
    }
    
    public void Design(string repositoryItemId)
    {
        var item = repository.GetItem(repositoryItemId);
        var projectType = RepositoryItemType.ToLlProject(item.Type);

        var ll = CreateLicensed();
        ll.DataSource = new List<string>();
        ll.FileRepository = repository;
        ll.AutoProjectType = projectType;
        ll.AutoShowSelectFile = false;
        ll.AutoProjectFile = repositoryItemId;
        ll.Printerless = true;
        ll.AutoFileAlsoNew = false;
        
#pragma warning disable CS0618 // Type or member is obsolete
        ll.DefineFields += (_, _) =>
        {
            ll.Fields.Add("Pos.Nr", 1);
        };
        ll.DefineVariables += (_, args) =>
        {
            ll.Variables.Add("Pos.Nr", 12345);
            ll.Variables.Add("Customer.Nr", "Mustermann");
        };
#pragma warning restore CS0618 // Type or member is obsolete
        
        var failedFields = new HashSet<string>();
        ll.ExpressionError += (_, args) => OnExpressionError(args, failedFields);
        ll.Design(projectType, repositoryItemId);
    }

#pragma warning disable SYSLIB1045
    private readonly Regex syntaxError = new ("Syntaxfehler: '(?<field>.+)' kann nicht interpretiert werden");
#pragma warning restore SYSLIB1045
    private void OnExpressionError(ExpressionErrorEventArgs e, HashSet<string> failedFields)
    {
        var match = syntaxError.Match(e.ErrorText);
        if (match.Success)
        {
            var field = match.Groups["field"].Value;
            failedFields.Add(field);
        }
        else
        {
            logger.LogError("{Content}: {Error}", e.Contents, e.ErrorText);
        }
    }
    
    private const string LicensingInfo = "gHbEGg";
    private static ListLabel CreateLicensed()
    {
        var ll = new ListLabel();
        ll.LicensingInfo = LicensingInfo;

        return ll;
    }
}