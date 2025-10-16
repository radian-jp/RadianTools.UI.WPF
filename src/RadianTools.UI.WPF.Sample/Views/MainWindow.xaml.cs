using RadianTools.UI.WPF.Sample.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace RadianTools.UI.WPF.Sample.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ButtonSetItem_Click(object sender, RoutedEventArgs e)
    {
        folderTreeView.SelectItemFromFilePath(textSetFilePath.Text);
    }
}