namespace SoundLoaderMaui
{
    public partial class App : Application
    {
        private static string Tag = "App";

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        // TODO test omission
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Console.WriteLine($"{Tag}: CreateWindow");
            //return new Window(new AppShell());
            return base.CreateWindow(activationState);
        }
    }
}
