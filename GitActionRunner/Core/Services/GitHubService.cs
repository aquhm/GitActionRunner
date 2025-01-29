using System.Diagnostics;
using Octokit;
using GitActionRunner.Core.Interfaces;
using Serilog;
using WorkflowRun = GitActionRunner.Core.Models.WorkflowRun;

namespace GitActionRunner.Core.Services
{
    public class GitHubService : IGitHubService
    {
        private GitHubClient _client;
        private readonly IAuthenticationService _authenticationService;

        public GitHubService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            InitializeClientAsync().ConfigureAwait(false);
        }
        
        private async Task InitializeClientAsync()
        {
            var token = await _authenticationService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client = new GitHubClient(new ProductHeaderValue("GitActionRunner"))
                {
                        Credentials = new Credentials(token)
                };
            }
        }

        public async Task<bool> AuthenticateAsync(string token)
        {
            try
            {
                Log.Information("Attempting to authenticate with GitHub");
                _client = new GitHubClient(new ProductHeaderValue("GitActionRunner"))
                {
                    Credentials = new Credentials(token)
                };

                var user = await _client.User.Current();
                await _authenticationService.SaveAccessTokenAsync(token);
                
                Log.Information("GitHub authentication successful for user: {UserLogin}", user.Login);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GitHub authentication failed");
                return false;
            }
        }
        
        private async Task EnsureClientInitialized()
        {
            if (_client == null)
            {
                Log.Debug("Initializing GitHub client");
                var token = await _authenticationService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _client = new GitHubClient(new ProductHeaderValue("GitActionRunner"))
                    {
                            Credentials = new Credentials(token)
                    };
                    
                    Log.Information("GitHub client initialized successfully");
                }
                else
                {
                    Log.Information("Failed to initialize GitHub client: No authentication token available");
                    throw new InvalidOperationException("GitHub client is not initialized. Please authenticate first.");
                }
            }
        }

        public async Task<IEnumerable<Models.Repository>> GetRepositoriesAsync()
        {
            try
            {
                await EnsureClientInitialized();
                Log.Information("Fetching repositories for current user");
            
                var repos = await _client.Repository.GetAllForCurrent();
                var result = repos.Select(r => new Models.Repository
                {
                        Name = r.Name,
                        Owner = r.Owner.Login,
                        Description = r.Description,
                });

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch repositories");
                throw;
            }

            return default;
        }

        public async Task<IEnumerable<Models.WorkflowRun>> GetWorkflowRunsAsync(string owner, string repo)
        {
            var runs = await _client.Actions.Workflows.List(owner, repo);
            return runs.Workflows.Select(r => new Models.WorkflowRun
            {
                    Id = r.Id.ToString(),
                    Name = r.Name,
                    Status = r.State.ToString(),
                    CreatedAt = r.CreatedAt.DateTime
            });
        }

        public async Task<Models.WorkflowRun> TriggerWorkflowAsync(string owner, string repo, string workflowId, string branch)
        {
            await EnsureClientInitialized();
    
            await _client.Actions.Workflows.CreateDispatch(
                                                           owner,
                                                           repo,
                                                           long.Parse(workflowId),
                                                           new CreateWorkflowDispatch(branch));

            await Task.Delay(2000);

            var runs = await _client.Actions.Workflows.Runs.List(owner, repo);

            var latestRun = runs.WorkflowRuns
                    .Where(r => r.WorkflowId == long.Parse(workflowId))
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

            if (latestRun != null)
            {
                return new WorkflowRun
                {
                        Id = workflowId,
                        RunId = latestRun.Id.ToString(),
                        Name = latestRun.Name,
                        Status = latestRun.Status.ToString(),
                        CreatedAt = DateTime.UtcNow
                };
            }

            return new WorkflowRun
            {
                    Id = workflowId,
                    Status = "queued",
                    CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<WorkflowRun> GetWorkflowRunStatusAsync(string owner, string repo, string runId)
        {
            await EnsureClientInitialized();
            try 
            {
                var run = await _client.Actions.Workflows.Runs.Get(owner, repo, long.Parse(runId));
                return new WorkflowRun
                {
                        RunId = runId,
                        Status = run.Status.StringValue,
                        Conclusion = run.Conclusion?.StringValue
                };
            }
            catch (NotFoundException ex)
            {
                return new WorkflowRun
                {
                        RunId = runId,
                        Status = "queued",
                        Conclusion = null
                };
            }
        }

        public async Task<IEnumerable<string>> GetBranchesAsync(string owner, string repo)
        {
            try
            {
                await EnsureClientInitialized();
                var branches = await _client.Repository.Branch.GetAll(owner, repo);
                return branches.Select(b => b.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting branches: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }
        
        public async Task<User> GetCurrentUser()
        {
            try
            {
                await EnsureClientInitialized();
                return await _client.User.Current();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}