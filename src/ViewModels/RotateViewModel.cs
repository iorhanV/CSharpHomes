using CSharpHomes.Views.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace CSharpHomes.ViewModels;

public partial class RotateViewModel : ObservableObject
{
    [ObservableProperty] private double _angle;

    [RelayCommand]
    private void Ok()
    {
        RibbonController.HideOptionsBar();
    }
}