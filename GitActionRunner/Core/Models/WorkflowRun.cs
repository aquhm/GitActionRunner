namespace GitActionRunner.Core.Models
{
    public class WorkflowRun
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

