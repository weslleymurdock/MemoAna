using MemoAna.Presentation.ViewModels;

namespace MemoAna.Presentation.Views;

public partial class GamePage : ContentPage
{
    private bool _canExit = false;
	public GamePage(GameViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override bool OnBackButtonPressed()
    {
        // Se o jogo já acabou (GameEnded), permite voltar normalmente sem alertas
        if (BindingContext is ViewModels.GameViewModel vm && vm.GameEnded)
        {
            return base.OnBackButtonPressed(); // Retorna false, liberando a navegação
        }

        // Se a flag estiver verdadeira, libera a saída
        if (_canExit)
        {
            return false; // false significa: "Não bloquear, pode voltar"
        }

        // Caso contrário, bloqueia a ação nativa e exibe o aviso ao jogador
        DispararAvisoSaida();

        return true; // true significa: "Interceptado! Bloqueie o botão voltar por enquanto"
    }

    private async void DispararAvisoSaida()
    {
        // Exibe um popup nativo na Thread Principal
        bool exit = await DisplayAlertAsync(
            "Partida em Andamento!",
            "Se você sair agora, seu progresso atual será perdido. Deseja mesmo abandonar o jogo?",
            "Sim, Sair",
            "Continuar Jogando");

        if (exit)
        {
            // O usuário confirmou a saída.
            _canExit = true;

            // Dispara o retorno à página anterior via Shell de forma assíncrona
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("..");
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        if (BindingContext is ViewModels.GameViewModel vm)
            vm.CleanupGame();
    }
}