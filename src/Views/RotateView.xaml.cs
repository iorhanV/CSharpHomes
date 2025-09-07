using System.Text.RegularExpressions;
using System.Windows.Input;
using CSharpHomes.ViewModels;

namespace CSharpHomes.Views;

public partial class RotateView
{
    public RotateView(RotateViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    
    private static readonly Regex _regex = new Regex(@"^-?\d*(?:\.\d*)?$");
    private void NumericTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !_regex.IsMatch(e.Text);
    }
}