using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
// Этот using нужен, если вы будете использовать специфичные для WPF типы из LiveChartsCore,
// например, темы или мапперы, специфичные для WPF.
// Пакет LiveChartsCore.SkiaSharpView.WPF обычно сам всё настраивает.
using LiveChartsCore.SkiaSharpView.WPF;


namespace FitnessTrainerPro.UI
{
    public partial class App : Application
    {
        public App() // Или protected override void OnStartup(StartupEventArgs e)
        {
            // Для LiveChartsCore 2.0 и выше, явный вызов Configure может быть не всегда обязателен,
            // если вы используете стандартные настройки и пакет LiveChartsCore.SkiaSharpView.WPF,
            // так как он автоматически регистрирует необходимые компоненты.
            // Однако, если вы хотите настроить мапперы или темы глобально, это место для этого.

            LiveCharts.Configure(config =>
                config
                    // Зарегистрируйте SkiaSharp в качестве движка отрисовки (это делает пакет SkiaSharpView)
                    // Пакет LiveChartsCore.SkiaSharpView.WPF уже должен это делать.
                    // .AddSkiaSharp() // Обычно уже включено пакетом

                    // Зарегистрируйте типы, которые вы будете отображать в графиках.
                    // Это важно, чтобы LiveCharts знала, как интерпретировать ваши данные.
                    // Например, если у вас есть модель данных 'MyDataPoint':
                    // .Register<MyDataPoint>(mapper =>
                    //     mapper
                    //         .Y((dataPoint, index) => dataPoint.Value) // Как получить Y значение
                    //         .X((dataPoint, index) => index)           // Как получить X значение (если нужно)
                    // )
                    // LiveChartsCore имеет встроенные мапперы для многих примитивных типов.
                    .AddDefaultMappers() // Хорошо иметь это, чтобы покрыть стандартные случаи

                    // Вы можете настроить тему здесь (необязательно, по умолчанию светлая)
                    // .AddLightTheme()
                    // .AddDarkTheme()
                    // Или даже создать свою
            );

            // Убедитесь, что InitializeComponent() вызывается, если у вас есть файл App.xaml
            // с ресурсами или другим содержимым. Если App.xaml пуст, эту строку можно опустить,
            // но обычно она генерируется Visual Studio и должна быть.
            // Если у вас нет строки InitializeComponent() и есть App.xaml, добавьте ее:
            // InitializeComponent();
        }

        // Если вы предпочитаете OnStartup:
        // protected override void OnStartup(StartupEventArgs e)
        // {
        //     base.OnStartup(e);
        //
        //     LiveCharts.Configure(config =>
        //         config
        //             .AddDefaultMappers()
        //             // .AddSkiaSharp() // Обычно уже включено пакетом LiveChartsCore.SkiaSharpView.WPF
        //             // .AddLightTheme()
        //     );
        //
        //     // InitializeComponent(); // Если есть App.xaml и не было в конструкторе
        // }
    }
}