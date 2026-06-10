global using APP = Microsoft.Maui.Controls.Application;
global using Microsoft.Extensions.DependencyInjection;
global using MemoAna.Presentation.Views;
namespace MemoAna
{
    public partial class App : APP
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new(new AppShell());
        
    }
}