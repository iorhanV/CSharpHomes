using System.Windows;
using System.Windows.Controls;
using CSharpHomes.ViewModels;

namespace CSharpHomes.Views;

public partial class ProjectSetupView : Window
{
    public ProjectSetupView(ProjectSetupViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ApplyBtnClick(object sender, RoutedEventArgs e)
    {
        // Commit any in-progress edits
        setupData.CommitEdit(DataGridEditingUnit.Cell, true);
        setupData.CommitEdit(DataGridEditingUnit.Row, true);

        DialogResult = true;
    }

    private void setupData_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}