using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySkype.Client.Models;
using MySkype.Client.ViewModels;

namespace MySkype.Client.Views
{
    public class MainWindowView : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private TextBox _searchTextBox;
        private ListBox _contactsListBox;
        private ListBox _searchResultListBox;
        private Grid _largeGrid;

        public MainWindowView()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);
            _searchTextBox = this.Find<TextBox>("SearchTextBox");
            _contactsListBox = this.Find<ListBox>("Contacts");
            _searchResultListBox = this.Find<ListBox>("SearchResult");
            _largeGrid = this.Find<Grid>("Large");

            _largeGrid.IsVisible = false;

            _searchTextBox.KeyDown += SearchStringEntered;
            _contactsListBox.Tapped += ItemTapped;
            _searchResultListBox.Tapped += ItemTapped;

            Activated += InitAsync;
        }

        private void ItemTapped(object sender, RoutedEventArgs e)
        {
            var dataContext = ((Control)e.Source).DataContext;

            if (dataContext is User user)
            {
                _viewModel.SetLargeUser(user);
                _largeGrid.IsVisible = true;
            }
        }

        private async void SearchStringEntered(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await _viewModel.SearchAsync();
        }

        private async void InitAsync(object sender, EventArgs e)
        {
            Activated -= InitAsync;

            Hide();
            await _viewModel.InitAsync();
            Show();
        }
    }
}