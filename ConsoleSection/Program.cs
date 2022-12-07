using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Configs;
using System.Text;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Pipeline;

namespace University.DotnetLabs.Lab4.ConsoleSection;

public class Program
{
    static string inputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\Input";
    static string outputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\Output";
    static void Main(string[] args)
    {
        ITestClassGenerator generator = new SophisticatedTestClassGenerator();
        PipelineConfigs configs = new PipelineConfigs(3, 3, 3);
        TestPipeline pipeline = new TestPipeline(generator, configs);

        ICollection<Task> observableTasks = new List<Task>();
        string[] files = Directory.GetFiles(inputDirectory);
        foreach (string file in files) 
        {
            Task task = pipeline.Generate(file, outputDirectory);
            observableTasks.Add(task);
        }
        Task.WaitAll(observableTasks.ToArray());
        Console.WriteLine("Everything is done");
    }
}