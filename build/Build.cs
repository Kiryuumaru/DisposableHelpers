using Nuke.Common;
using Nuke.Common.ProjectModel;
using NukeBuildHelpers;

public class Build : BaseNukeBuildHelpers
{
    public static int Main () => Execute<Build>(x => x.Version);

    [Solution(GenerateProjects = true)]
    internal readonly Solution Solution;

    public override string[] EnvironmentBranches { get; } = ["prerelease", "master"];

    public override string MainEnvironmentBranch { get; } = "master";
}
