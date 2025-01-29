using CommunityToolkit.Mvvm.ComponentModel;

namespace GitActionRunner.Core.Models
{
    public class WorkflowRun : ObservableObject
    {
        private string _status;
        public string Id { get; set; }
        public string Name { get; set; }
        public string RunId { get; set; }
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public DateTime CreatedAt { get; set; }
        public string Conclusion { get; set; }  // "success", "failure", "cancelled" 등
    }
}

