using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyFlow.Model
{
    public class SubTask : INotifyPropertyChanged
    {
        // Unique ID for this subtask
        public string Id { get; set; } = string.Empty;

        // The subtask title
        public string Title { get; set; } = string.Empty;

        // IsCompleted notifies the UI when it changes
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}