using System.IO;
using Alexpuh.ListLabel.JsonFilesRepository;
using combit.Reporting.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace ListLabel.Design;

public static class Setup
{
    private const string ConnectionString = "mongodb://localhost:27017";
    private const string Database = "LLRepoLabels";
    private const string LegacyMappingCollectionName = "LegacyLabelsMapping";

    
    public static IServiceProvider CreateServiceProvider()
    {
        var repositoryDirectory = Path.Combine(Directory.GetCurrentDirectory(), "_TestRepository");
        Directory.CreateDirectory(repositoryDirectory);
        var repositoryOptions = new JsonFilesRepositoryOptions { DirectoryPath = repositoryDirectory };
        
        var sc = new ServiceCollection();
        sc
            .AddTransient<MainWindowViewModel>()
            .AddSingleton(repositoryOptions)
            .AddScoped<IRepository, JsonFilesRepository>()
            .AddLogging()
            ;
        return sc.BuildServiceProvider();
    }  
    
    
}