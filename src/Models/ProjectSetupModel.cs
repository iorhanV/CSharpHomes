using System.ComponentModel;

namespace CSharpHomes.Models
{
    public class ProjectSetupModel : INotifyPropertyChanged
    {
        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        // Original PDF value (read-only in UI)
        private string? _originalValue;
        public string? OriginalValue
        {
            get => _originalValue;
            set { _originalValue = value; OnPropertyChanged(nameof(OriginalValue)); }
        }

        // Editable value (Clean Input)
        private string? _input;
        public string? Input
        {
            get => _input;
            set { _input = value; OnPropertyChanged(nameof(Input)); }
        }

        private List<string?> _options = new List<string?>();
        public List<string?> Options
        {
            get => _options;
            set { _options = value; OnPropertyChanged(nameof(Options)); }
        }

        private bool _apply;
        public bool Apply
        {
            get => _apply;
            set { _apply = value; OnPropertyChanged(nameof(Apply)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}