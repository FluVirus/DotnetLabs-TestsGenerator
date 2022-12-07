using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;
using System.Text;

namespace University.DotnetLabs.Lab4.ConsoleSection;

public class Program
{
    static string inputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\Input";
    static string outputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\Output";
    static void Main(string[] args)
    {
        ITestClassGenerator generator = new PrimitiveTestClassGenerator();
        string[] files = Directory.GetFiles(inputDirectory);
        foreach (string file in files) 
        { 
            string source = File.ReadAllText(file);
            string testsource = generator.GenerateTestClassCode(source);
            string testfile = file + ".TESTS.cs";
            File.WriteAllText(testfile, testsource, Encoding.Unicode);
        }
        Console.WriteLine("Everything is done");
    }
}