using NotionFlow.App.ViewModels.Admin;

namespace NotionFlow.App.Views.Admin;

public partial class CreateStudentPage : ContentPage
{
	public CreateStudentPage(AdminViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
