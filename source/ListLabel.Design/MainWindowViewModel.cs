using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using combit.Reporting;
using combit.Reporting.DataProviders;
using combit.Reporting.Dom;
using combit.Reporting.Repository;
using ListLabel.ArticleLabels;
using ListLabel.LegacyLabels;
using Microsoft.Win32;
using Prism.Commands;

namespace ListLabel.Design;

public sealed class MainWindowViewModel: INotifyPropertyChanged
{
    private readonly IRepository repository;
    private ArticleLabel? selectedArticleLabel;
    private ArticleLabel[] labelList;

    public ArticleLabel? SelectedArticleLabel
    {
        get => selectedArticleLabel;
        set
        {
            SetField(ref selectedArticleLabel, value);
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }

    public ArticleLabel[] LabelList
    {
        get => labelList;
        set
        {
            SetField(ref labelList, value);
            SelectedArticleLabel = null;
        }
    }

    public DelegateCommand EditCommand { get; }
    public DelegateCommand PrintCommand { get; }
    public DelegateCommand NewCommand { get; }
    public DelegateCommand UploadCommand { get; }
    public DelegateCommand ReloadCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand AssignOne { get; }
    public DelegateCommand AssignTwo { get; }

    private string? ExtractParentId(string descriptor)
    {
        var parentId = RepositoryItemDescriptor.LoadFromDescriptorString(descriptor).GetParentId();
        return parentId == String.Empty ? null : parentId;
    }

    public MainWindowViewModel(IRepository repository)
    {
        this.repository = repository;
        EditCommand = new DelegateCommand(()=> Edit(SelectedArticleLabel!.RepositoryId, false), () => SelectedArticleLabel != null);
        PrintCommand = new DelegateCommand(()=> Print(SelectedArticleLabel!.RepositoryId, false), () => SelectedArticleLabel != null);
        DeleteCommand = new DelegateCommand(()=> Delete(SelectedArticleLabel!.RepositoryId), () => SelectedArticleLabel != null);
        NewCommand = new DelegateCommand(NewLabel);
        ReloadCommand = new DelegateCommand(ReloadList);
        UploadCommand = new DelegateCommand(UploadLabel);
        labelList = repository.GetAllItems()
            .Where(i => ExtractParentId(i.Descriptor) == null)
            .Select(i => new ArticleLabel
        {
            RepositoryId = i.InternalID,
            Name = i.ExtractDisplayName(),
            Type = i.Type,
        }).ToArray();
    }

    private void UploadLabel()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select label to upload"
        };
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        using var ll = TrivialSetupListLabel();
        var util = new RepositoryImportUtil(repository);
        var imported = util.ImportProjectFile(ll, openFileDialog.FileName);
        MessageBox.Show(imported);
    }

    private void ReloadList()
    {
        LabelList = repository.GetAllItems()
            .Select(i => new ArticleLabel
        {
            RepositoryId = i.InternalID,
            Name = i.ExtractDisplayName(),
            Type = i.Type,
        }).ToArray();
    }

    private combit.Reporting.ListLabel SetupListLabel<T>(string? labelId = null, T? labelData = default) where T: new()
    {
        var ll = SetupListLabel(labelId);
        ll.DataSource = new ObjectDataProvider(labelData ?? new T());
        return ll;
    }

    private combit.Reporting.ListLabel SetupListLabel(string? labelId = null)
    {
        var projectType = LlProject.Label;
        var isNewLabel = string.IsNullOrWhiteSpace(labelId);
        if (!isNewLabel)
        {
            var item = repository.GetItem(labelId);
            projectType = RepositoryItemType.ToLlProject(item.Type);
        }
        var ll = new combit.Reporting.ListLabel();
        ll.Printerless = true;
        ll.LicensingInfo = "gHbEGg";
        ll.FileRepository = repository;
        ll.AutoProjectType = projectType;
        ll.AutoShowSelectFile = false;
        
        if (!isNewLabel)
        {
            ll.AutoProjectFile = labelId;   
        }
        ll.AutoFileAlsoNew = isNewLabel;
        return ll;
    }

    
    private combit.Reporting.ListLabel TrivialSetupListLabel()
    {
        var ll = new combit.Reporting.ListLabel();
        ll.Printerless = true;
        ll.LicensingInfo = "gHbEGg";
        ll.FileRepository = repository;
        return ll;
    }

    private void NewLabel()
    {
        string templateId;
        using (var util = new RepositoryImportUtil(repository))
        {
            templateId = util.CreateNewProject(LlProject.Label, "My Display", null);    
        }
        
        Edit(templateId, true);
    }

#pragma warning disable SYSLIB1045
    private readonly Regex syntaxError = new ("Syntaxfehler: '(?<field>.+)' kann nicht interpretiert werden");
#pragma warning restore SYSLIB1045
    private void Edit(string? labelId, bool setupPage)
    {
        try
        {
            //using var ll = SetupListLabel(labelId);
            
            using var ll = SetupListLabel<ArticleLabel>(labelId);
            var label = new ArticleLabelData
            {
                Name1 = "Gewürzmischung",
                Name2 = "Paprika",
                Expiration = 1,
                LabelTypeId = 1,
                ArtNr = "7860",
                UnitWeight = 100,
                LabelPrice = 3.49M,
                Description = "Loremipsum, Loremipsum, Loremipsum\nLoremipsum, Loremipsum, Loremipsum",
                Measure = "g",  
                Index = "A88",
                Origin = "EU-",
                UnitAdditionalInfo = "999",
                Barcode = "42|6001199016",
                
            }; 
            ll.DefineLegacyLabelVariables(label);
            
            var failedFields = new HashSet<string>();
            ll.ExpressionError += (_, args) =>
            {
                var match = syntaxError.Match(args.ErrorText);
                if (match.Success)
                {
                    var field = match.Groups["field"].Value;
                    failedFields.Add(field);
                }
                else
                {
                    Console.WriteLine(args.Contents + " " + args.ErrorText);
                }
            };

            ll.ProjectLoaded += (sender, _) =>
            {
                foreach (var failedField in failedFields)
                {
                    Console.WriteLine($"ll.Variables.Add(\"{failedField}\", \"\");");
                }
                
                if (setupPage)
                {
                    var projectLabel = new ProjectLabel((combit.Reporting.ListLabel)sender);
                    projectLabel.GetFromParent();
                    
                    var regionZero = projectLabel.Regions[0];
                    regionZero.Layout.XSize = "74950";
                    regionZero.Layout.YSize = "98960";
                    regionZero.Layout.XOffset = "2000";
                    regionZero.Layout.YOffset = "3000";
                    regionZero.Paper.Format = "256";
                    regionZero.Paper.Extent.Horizontal = "74950";
                    regionZero.Paper.Extent.Vertical = "98960";
                }
            };

            // var projectLabel = new ProjectLabel(ll);
            // projectLabel.GetFromParent();
            // projectLabel.Open(labelId, LlDomFileMode.OpenOrCreate, LlDomAccessMode.ReadWrite);

            ll.Design();
        }
        catch (LL_User_Aborted_Exception)
        {
        }
    }

    
    private void Print(string? labelId, bool setupPage)
    {
        try
        {
            //using var ll = SetupListLabel(labelId);
            
            using var ll = SetupListLabel(labelId);
            ll.DataSource = new List<string>(["DataPlaceHolder"]);
            ll.Printerless = false;
            ll.AutoDestination = LlPrintMode.Normal;
            
            var label = new ArticleLabelData
            {
                Name1 = "Gewürzmischung",
                Name2 = "Paprika",
                Expiration = 1,
                LabelTypeId = 1,
                ArtNr = "7860",
                UnitWeight = 100,
                LabelPrice = 3.49M,
                Description = "Loremipsum, Loremipsum, Loremipsum\nLoremipsum, Loremipsum, Loremipsum",
                Measure = "g",  
                Index = "A88",
                Origin = "EU-",
                UnitAdditionalInfo = "999",
                Barcode = "42|6001199016",
            }; 
            ll.DefineLegacyLabelVariables(label);
            
            var failedFields = new HashSet<string>();
            ll.ExpressionError += (_, args) =>
            {
                var match = syntaxError.Match(args.ErrorText);
                if (match.Success)
                {
                    var field = match.Groups["field"].Value;
                    failedFields.Add(field);
                }
                else
                {
                    Console.WriteLine(args.Contents + " " + args.ErrorText);
                }
            };

            ll.ProjectLoaded += (sender, _) =>
            {
                foreach (var failedField in failedFields)
                {
                    Console.WriteLine($"ll.Variables.Add(\"{failedField}\", \"\");");
                }
                
                if (setupPage)
                {
                    var projectLabel = new ProjectLabel((combit.Reporting.ListLabel) sender);
                    projectLabel.GetFromParent();
                    
                    var regionZero = projectLabel.Regions[0];
                    regionZero.Layout.XSize = "74950";
                    regionZero.Layout.YSize = "98960";
                    regionZero.Layout.XOffset = "2000";
                    regionZero.Layout.YOffset = "3000";
                    regionZero.Paper.Format = "256";
                    regionZero.Paper.Extent.Horizontal = "74950";
                    regionZero.Paper.Extent.Vertical = "98960";
                }
            };

            ll.AutoDefineVariable += (sender, _) =>
            {
                var llJob = (combit.Reporting.ListLabel) sender;
                llJob.DefineLegacyLabelVariables(label);
            };
            
#pragma warning disable CS0618 // Type or member is obsolete
            ll.DefineVariables += (sender, _) =>
#pragma warning restore CS0618 // Type or member is obsolete
            {
                var llJob = (combit.Reporting.ListLabel) sender;
                llJob.DefineLegacyLabelVariables(label);
            };

            // var projectLabel = new ProjectLabel(ll);
            // projectLabel.GetFromParent();
            // projectLabel.Open(labelId, LlDomFileMode.OpenOrCreate, LlDomAccessMode.ReadWrite);
            
            //ll.Print("Brother MFC-J4540DW Printer");
            ll.AutoShowPrintOptions = false;
            ll.Print("Brother MFC-J4540DW Printer");
        }
        catch (LL_User_Aborted_Exception)
        {
        }
    }
    private void Delete(string repositoryId)
    {
        repository.DeleteItem(repositoryId);
        ReloadList();
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(propertyName);
        CommandManager.InvalidateRequerySuggested();
        return true;
    }
}