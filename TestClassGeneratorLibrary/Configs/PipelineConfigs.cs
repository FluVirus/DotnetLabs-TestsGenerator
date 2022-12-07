using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Configs;

public record class PipelineConfigs (int MaxRead, int MaxGenerate, int MaxWrite);
