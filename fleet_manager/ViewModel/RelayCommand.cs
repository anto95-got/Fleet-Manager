using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FleetManager.ViewModels;

// ===============================================================
// 1. Version CLASSIQUE (Pour les boutons simples sans paramètres)
// ===============================================================
public class RelayCommand : ICommand
{
    private readonly Action? _execute;           // Action simple
    private readonly Func<Task>? _executeAsync;  // Action Asynchrone (Task)
    private readonly Func<bool>? _canExecute;    // Condition (Actif/Inactif)

    // Constructeur Synchrone (ex: () => CloseForm())
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // Constructeur Asynchrone (ex: async () => await Save())
    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        if (_execute != null)
        {
            _execute();
        }
        else if (_executeAsync != null)
        {
            await _executeAsync();
        }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

// ===============================================================
// 2. Version GÉNÉRIQUE <T> (Pour passer un paramètre, ex: le Véhicule)
// ===============================================================
public class RelayCommand<T> : ICommand
{
    private readonly Action<T>? _execute;           // Action avec paramètre
    private readonly Func<T, Task>? _executeAsync;  // Action Async avec paramètre
    private readonly Predicate<T>? _canExecute;

    // Constructeur Synchrone
    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // Constructeur Asynchrone
    public RelayCommand(Func<T, Task> executeAsync, Predicate<T>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute((T)parameter!);
    }

    public async void Execute(object? parameter)
    {
        if (_execute != null)
        {
            _execute((T)parameter!);
        }
        else if (_executeAsync != null)
        {
            await _executeAsync((T)parameter!);
        }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}