namespace BookChat
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1) ;
            //CounterBtn.Text = $"Clicked {count} time";
            else;
                //CounterBtn.Text = $"Clicked {count} times";

                //SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Navigate to SettingsPage using Shell routing
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
