using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using NukeBuildHelpers;
using NukeBuildHelpers.Attributes;
using NukeBuildHelpers.Enums;
using NukeBuildHelpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _build;

public class DisposableHelpersTest : AppTestEntry<Build>
{
    public override bool RunParallel => false;

    public override RunsOnType RunsOn => RunsOnType.Ubuntu2204;

    public override Type[] AppEntryTargets => [typeof(DisposableHelpers)];

    public override void Run()
    {
        DotNetTasks.DotNetClean(_ => _
            .SetProject(NukeBuild.Solution.DisposableHelpersTest));
        DotNetTasks.DotNetTest(_ => _
            .SetProjectFile(NukeBuild.Solution.DisposableHelpersTest));
    }
}
