using UraniumUI;

#if ANDROID
using SoundLoaderMaui.Platforms.Android;
#endif

namespace SoundLoaderMaui
{
    public static class MauiProgram
    {
        private static readonly string Tag = nameof(MauiProgram);
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFontAwesomeIconFonts();
                    fonts.AddMaterialIconFonts();
                });

#if ANDROID
            Console.WriteLine($"{Tag} building android app");
            
#endif

            // dependency injection
            builder.Services.AddSingleton<MainPage>();
#if ANDROID
            builder.Services.AddTransient<IServiceDownload, DownloadService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
