using System;

namespace numan.Models;

public class NumanConfig
{
    public List<NugetSource> NugetSources { get; set; } = new();
}