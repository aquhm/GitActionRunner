using Octokit;

namespace GitActionRunner.Core.Interfaces;

public interface IGitHubService
{
    Task<bool> AuthenticateAsync(string token);
    Task<IEnumerable<Models.Repository>> GetRepositoriesAsync();
    Task<IEnumerable<string>> GetBranchesAsync(string owner, string repo);
    Task<IEnumerable<Models.WorkflowRun>> GetWorkflowRunsAsync(string owner, string repo);
    Task<Models.WorkflowRun> TriggerWorkflowAsync(string owner, string repo, string workflowId, string branch); 
    Task<Models.WorkflowRun> GetWorkflowRunStatusAsync(string owner, string repo, string runId);
    Task<User> GetCurrentUser();
    
}