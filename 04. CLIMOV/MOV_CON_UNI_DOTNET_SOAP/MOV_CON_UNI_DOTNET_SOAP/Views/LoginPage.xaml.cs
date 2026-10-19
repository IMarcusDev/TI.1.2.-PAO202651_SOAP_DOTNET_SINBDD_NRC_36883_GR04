using MOV_CON_UNI_DOTNET_SOAP.Controllers;

namespace MOV_CON_UNI_DOTNET_SOAP.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginController _controller;

    public LoginPage(LoginController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    private async void OnIngresarClicked(object sender, EventArgs e)
    {
        var resultado = await _controller.LoginAsync(UsernameEntry.Text, PasswordEntry.Text);

        if (resultado.Success)
            await Shell.Current.GoToAsync("//main/home");
        else
        {
            ErrorLabel.Text = resultado.Message;
            ErrorLabel.IsVisible = true;
        }
    }
}