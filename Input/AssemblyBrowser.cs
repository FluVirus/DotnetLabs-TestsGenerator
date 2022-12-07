using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using University.DotnetLabs.Lab3.AssemblyBrowserLibrary.Structures;

namespace University.DotnetLabs.Lab3.AssemblyBrowserLibrary;

public sealed class AssemblyBrowser : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Assembly? BrowsedAssembly
    {
        set
        {
            OnPropertyChanged();
            Demonstration = (value is null) ? null : new AssemblyStructure(value);
        }
    }

    private AssemblyStructure? _assemblyStructure;
    public AssemblyStructure? Demonstration
    {
        get
        {
            return _assemblyStructure;
        }
        private set
        {
            if (_assemblyStructure == value)
                return;
            _assemblyStructure = value;
            OnPropertyChanged();
        }
    }
}