using Microsoft.Extensions.DependencyInjection;

namespace MOV_CON_UNI_DOTNET_SOAP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}