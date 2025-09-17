// See https://aka.ms/new-console-template for more information

using System.IO;
using Alexpuh.ListLabel.JsonFilesRepository;
using combit.Reporting.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace listlabel_test_app;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var sc = new ServiceCollection();

        var testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
        if (!Directory.Exists(testDataDirectory))
        {
            throw new DirectoryNotFoundException("Test data directory not found: " + testDataDirectory);   
        }
        var repositoryDirectory = Path.Combine(Directory.GetCurrentDirectory(), "_TestRepository");
        Directory.CreateDirectory(repositoryDirectory);
        var repositoryOptions = new JsonFilesRepositoryOptions { DirectoryPath = repositoryDirectory };

        sc.AddScoped<App>()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            })
            .AddSingleton(repositoryOptions)
            .AddScoped<IRepository, JsonFilesRepository>()
            ;

        var sp = sc.BuildServiceProvider();

        var app = sp.GetRequiredService<App>();

        if (!app.TryGetItemByUiName("new-list", out var listId))
        {
            app.Import(Path.Combine(testDataDirectory, "etikett.lbl"), "label");
            app.Import(Path.Combine(testDataDirectory, "rechnung.lst"), "list");
            listId = app.CreateList("My List");
        }

/*
        var uiName = "NewList";
        if (!app.TryGetItemByUiName(uiName, out var labelId))
        {
            labelId = app.CreateLabel(uiName);    
        }
*/        
        app.Design(listId!);
        
  /*      
        app.Design(labelId!);
        */

        /*
        var repo = new JsonFilesRepository(NullLogger<JsonFilesRepository>.Instance, repositoryOptions);
        string templateId = "repository://{7CE97635-290A-4459-BE37-68B4F3800394}";
        using (var util = new RepositoryImportUtil(repo))
        {
            templateId = util.CreateNewProject(LlProject.Label, "My Display", null);    
        }
        
        
        
        var projectType = LlProject.Label;
        using (var ll = new combit.Reporting.ListLabel())
        {
            ll.Printerless = true;
            ll.LicensingInfo = "gHbEGg";
            ll.FileRepository = repo;
            ll.AutoProjectType = projectType;
            ll.AutoShowSelectFile = false;
        
            ll.AutoProjectFile = templateId;
            ll.AutoFileAlsoNew = false;
            ll.AutoDestination = LlPrintMode.Normal;
            var a = new { Customer = "Mustermann", OrderNr = "1234567890"};
            ll.DataSource = new ObjectDataProvider(a);
            ll.Design();
        }
        
        using (var ll = new combit.Reporting.ListLabel())
        {
            ll.Printerless = true;
            ll.LicensingInfo = "gHbEGg";
            ll.FileRepository = repo;
            ll.AutoProjectType = projectType;
            ll.AutoShowSelectFile = false;
        
            ll.AutoProjectFile = templateId;
            ll.AutoFileAlsoNew = false;
            ll.AutoDestination = LlPrintMode.Normal;
            var a = new { Customer = "Mustermann", OrderNr = "1234567890"};
            ll.DataSource = new ObjectDataProvider(a);
            ll.Design();
        }
        */
    }
}