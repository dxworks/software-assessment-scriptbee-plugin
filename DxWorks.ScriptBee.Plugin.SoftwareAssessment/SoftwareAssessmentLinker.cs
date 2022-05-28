using DxWorks.ScriptBee.Plugin.Api;
using Dxworks.ScriptBee.Plugins.InspectorGit.Model;
using Honeydew.ScriptBeePlugin;
using Honeydew.ScriptBeePlugin.Models;
using JiraScriptBeePlugin.Models;

namespace DxWorks.ScriptBee.Plugin.SoftwareAssessment;

public class SoftwareAssessmentLinker : IModelLinker
{
    public string GetName() => "Software Assessment";

    public Task LinkModel(Dictionary<Tuple<string, string>, Dictionary<string, ScriptBeeModel>> context,
        Dictionary<string, object>? configuration = default, CancellationToken cancellationToken = default)
    {
        var repos = context[Tuple.Create("Repository", "InspectorGit")].Values.Cast<Repository>();
        var issues = context[Tuple.Create("Issue", "jira")].Values.Cast<Issue>();
        var files = context[Tuple.Create("File", "honeydew")].Values.Cast<FileModel>();

        foreach (var issue in issues)
        {
            foreach (var repository in repos)
            {
                foreach (var commit in repository.Commits)
                {
                    if (commit.Message.Contains(issue.key))
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
