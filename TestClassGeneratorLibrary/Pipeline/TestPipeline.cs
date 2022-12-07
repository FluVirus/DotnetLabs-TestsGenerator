using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Configs;
namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Pipeline;

public class TestPipeline
{
    private readonly ITestClassGenerator _generator;
    private readonly PipelineConfigs _configs;

    public TestPipeline(ITestClassGenerator generator, PipelineConfigs configs)
    {
        _generator = generator;
        _configs = configs;
    }

    private static async Task<string> ReadFileAsync(string fileName)
    { 
        using StreamReader streamReader = new StreamReader(fileName);
        return await streamReader.ReadToEndAsync();
    }

    private static async Task WriteFileAsync(string code, string file, string folder)
    { 
        string path = Path.Combine(folder, file) + "_" + Guid.NewGuid().ToString() + ".cs";
        using StreamWriter streamWriter = new StreamWriter(path);
        await streamWriter.WriteAsync(code);    
    }

    public Task Generate(string file, string folder)
    {
        TransformBlock<string, string> readFileBlock = new TransformBlock<string, string>(
            async fileName => await ReadFileAsync(fileName),
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = _configs.MaxRead
            });

        TransformBlock<string, string> generateCodeBlock = new TransformBlock<string, string>(
            source => _generator.GenerateTestClassCode(source),
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = _configs.MaxGenerate
            }
            );

        ActionBlock<string> writeFileBlock = new ActionBlock<string>(
            async source => await WriteFileAsync(source, file, folder),
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = _configs.MaxWrite
            }
            );

        DataflowLinkOptions linkOptions = new DataflowLinkOptions()
        {
            PropagateCompletion = true
        };
        readFileBlock.LinkTo(generateCodeBlock, linkOptions);
        generateCodeBlock.LinkTo(writeFileBlock, linkOptions);

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }




    }

}
