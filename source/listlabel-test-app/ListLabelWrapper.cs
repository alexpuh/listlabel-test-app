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
    
    private LlProject? projectType;
    private string? fileId;
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

    public IDisposable Print()
    {
        ll.Print(projectType!.Value, fileId);
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