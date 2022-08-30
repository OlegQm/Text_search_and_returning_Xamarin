using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Text_search_and_returning
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void GoToTextEditingPage_Clicked(object sender, EventArgs e)
        {
            TextEditing page = new TextEditing();
            await Navigation.PushAsync(page);
        }
    }
}