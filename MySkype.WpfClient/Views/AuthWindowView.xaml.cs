﻿using System.Windows;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for AuthWindowView.xaml
    /// </summary>
    public partial class AuthWindowView : Window
    {
        private readonly AuthWindowViewModel _viewModel;

        public AuthWindowView()
        {
            InitializeComponent();

            _viewModel = new AuthWindowViewModel();
            DataContext = _viewModel;


            SignInButton.Click += OnSignInButtonClick;
        }

        private async void OnSignInButtonClick(object sender, RoutedEventArgs e)
        {
            var token = await _viewModel.SignInAsync();

            if (token == null) return;

            Token = token;
            Close();
        }

        public string Token { get; set; }

    }
}