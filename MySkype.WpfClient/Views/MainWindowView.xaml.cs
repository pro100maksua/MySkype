using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class MainWindowView : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowView()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            SearchTextBox.KeyDown += SearchStringEntered;
            Contacts.SelectionChanged += ItemTapped;
            SearchResult.SelectionChanged += ItemTapped;
            Loaded += InitAsync;
        }

        private void ItemTapped(object sender, RoutedEventArgs e)
        {
            var listBox = sender as ListBox;

            var user = listBox.SelectedItem as User;

            _viewModel.SetLargeArea(user);
        }

        private async void SearchStringEntered(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await _viewModel.SearchAsync();
        }

        private async void InitAsync(object sender, EventArgs e)
        {
            Loaded -= InitAsync;

            Hide();
            await _viewModel.InitAsync();
            Show();
        }
    }
}