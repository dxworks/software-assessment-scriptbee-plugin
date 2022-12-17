using DxWorks.ScriptBee.Plugin.Api;
using DxWorks.ScriptBee.Plugins.Honeydew;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Dxworks.ScriptBee.Plugins.InspectorGit.Model;
using DxWorks.ScriptBee.Plugins.JiraMiner.Models;
using InspectorGitLoader = Dxworks.ScriptBee.Plugins.InspectorGit.ScriptBeeModelLoader;
using JiraMinerLoader = DxWorks.ScriptBee.Plugins.JiraMiner.Loaders.ModelLoader;
using HoneydewLoader = DxWorks.ScriptBee.Plugins.Honeydew.Loaders.ModelLoader;

namespace DxWorks.ScriptBee.Plugins.SoftwareAssessment;

public class SoftwareAssessmentLinker : IModelLinker
{
    private readonly string _inspectorGitPluginName;
    private readonly string _jiraMinerPluginName;
    private readonly string _honeydewPluginName;

    public SoftwareAssessmentLinker(IEnumerable<IModelLoader> loaders)
    {
        var modelLoaders = loaders.ToList();
        _inspectorGitPluginName = modelLoaders.OfType<InspectorGitLoader>().First().GetName();
        _jiraMinerPluginName = modelLoaders.OfType<JiraMinerLoader>().First().GetName();
        _honeydewPluginName = modelLoaders.OfType<HoneydewLoader>().First().GetName();
    }

    public string GetName() => "software-assessment";

    public Task LinkModel(Dictionary<Tuple<string, string>, Dictionary<string, ScriptBeeModel>> context,
        Dictionary<string, object>? configuration = default, CancellationToken cancellationToken = default)
    {
        var repos = context[Tuple.Create("Repository", _inspectorGitPluginName)].Values.Cast<Repository>();
        var issues = context[Tuple.Create("Issue", _jiraMinerPluginName)].Values.Cast<Issue>();
        var files = context[Tuple.Create("File", _honeydewPluginName)].Values.Cast<FileModel>();

        foreach (var issue in issues)
        {
            foreach (var repository in repos)
            {
                foreach (var commit in repository.Commits)
                {
                    if (commit.Message.Contains(issue.Key))
                    {
                        if (commit.HasProperty("Issues"))
                        {
                            (commit["Issues"] as List<Issue>).Add(issue);
                        }
                        else
                        {
                            commit["Issues"] = new List<Issue> { issue };
                        }

                        if (issue.HasProperty("Commits"))
                        {
                            (issue["Commits"] as List<Commit>).Add(commit);
                        }
                        else
                        {
                            issue["Commits"] = new List<Commit> { commit };
                        }
                    }
                }
            }
        }


        return Task.CompletedTask;
    }
}
