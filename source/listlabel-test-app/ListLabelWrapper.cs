using System.IO;
using combit.Reporting;
using combit.Reporting.Repository;

namespace listlabel_test_app;

public class ListLabelWrapper: IDisposable
{
    private readonly ListLabel ll;
    
    public static ListLabelWrapper Create(string license)
    {
        return new ListLabelWrapper(license);
    }

    public void Dispose()
    {
        ll.Dispose();
    }
    
    public ListLabelWrapper(string license)
    {
        ll = new ListLabel();
        ll.LicensingInfo = license;
    }

    public ListLabelWrapper SetFileRepository(IRepository repository)
    {
        ll.FileRepository = repository;
        return this;
    }

    /*
    public ListLabelWrapper SetupDesign(string fileId)
    {
        var repository = ll.FileRepository;
        if (repository == null)
        {
            throw new InvalidOperationException("File repository not set");
        }
        var item = repository.GetItem(fileId);
        var projectType = RepositoryItemType.ToLlProject(item.Type);
        return SetupDesign(projectType, fileId);
    }
    
    public ListLabelWrapper SetupDesign(LlProject projectType, string fileId)
    {
        this.projectType = projectType;
        this.fileId = fileId;
        ll.AutoProjectType = projectType;
        ll.AutoShowSelectFile = false;
        ll.AutoProjectFile = fileId;
        ll.Printerless = true;
        ll.AutoFileAlsoNew = false;
        return this;
    }

    public IDisposable Design()
    {
        ll.Design(projectType!.Value, fileId);
        return this;
    }
    */


    private LlProject GetProjectType(string? templateId)
    {
        var repository = ll.FileRepository;
        if (repository == null)
        {
            throw new InvalidOperationException("File repository not set");
        }
        var item = repository.GetItem(templateId);
        var projectType = RepositoryItemType.ToLlProject(item.Type);
        return projectType;
    }
    
    private void SetupListLabel(LlProject projectType, string? templateId)
    {
        //ll.DataSource = new List<string>(["DataPlaceHolder"]);
        
        ll.Printerless = false;
        ll.AutoDestination = LlPrintMode.Normal;

        ll.Core.LlSetOption(LlOption.NoFileVersionUpgradeWarning, 1);
        ll.Core.LlSetOption(LlOption.IncludeFontDescent, 0);
        
        ll.AutoProjectType = projectType;
        ll.AutoShowSelectFile = false;
        
        
        ll.AutoProjectFile = templateId;
        ll.AutoFileAlsoNew = false;
    }
    
    
    public IDisposable Print(string templateId)
    {
        //var projectType = GetProjectType(templateId);
        var repository = ll.FileRepository;
        if (repository == null)
        {
            throw new InvalidOperationException("File repository not set");
        }
        var item = repository.GetItem(templateId);
        var projectType = RepositoryItemType.ToLlProject(item.Type);
        
        SetupListLabel(projectType, templateId);
        
        var tempFile = Path.GetTempFileName();
        using (var s1 = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096)) //, FileOptions.DeleteOnClose
        {
            repository.LoadItem(templateId, s1, CancellationToken.None);
            s1.Flush();
            s1.Close();
        }
        ll.Print("Bullzip1", projectType, tempFile);
        
        //ll.Print("Bullzip1");
        //ll.Print(projectType, templateId);;
        /*
        byte[] templateData;
        using (var memoryStream = new MemoryStream())
        {
            repository.LoadItem(templateId, memoryStream, CancellationToken.None);
            templateData = memoryStream.ToArray();
        }
        using (var stream = new MemoryStream(templateData))
        {
            //ll.Core.FixedPrinterName = printerName;
            ll.Print(projectType, stream);    
        }        
        */

        
        
        return this;
    }

    public IDisposable Design(string templateId)
    {
        //var projectType = GetProjectType(templateId);
        var repository = ll.FileRepository;
        if (repository == null)
        {
            throw new InvalidOperationException("File repository not set");
        }
        var item = repository.GetItem(templateId);
        var projectType = RepositoryItemType.ToLlProject(item.Type);
        
        SetupListLabel(projectType, templateId);
        
        ll.Design(projectType, templateId);
        
        //ll.Print("Bullzip1");
        //ll.Print(projectType, templateId);;
        /*
        byte[] templateData;
        using (var memoryStream = new MemoryStream())
        {
            repository.LoadItem(templateId, memoryStream, CancellationToken.None);
            templateData = memoryStream.ToArray();
        }
        using (var stream = new MemoryStream(templateData))
        {
            //ll.Core.FixedPrinterName = printerName;
            ll.Print(projectType, stream);
        }
        */

        
        
        return this;
    }

    public ListLabelWrapper DefineVariables(Action<VariableCollection> define)
    {
        ll.DefineVariables += (_, _) =>
        {
            define(ll.Variables);
        };
        return this;
    }
    
    public ListLabelWrapper DefineFields(Func<FieldCollection, bool> nextLine)
    {
        ll.DefineFields += (_, args) =>
        {
            args.IsLastRecord = !nextLine(ll.Fields);
        };
        return this;
    }
}