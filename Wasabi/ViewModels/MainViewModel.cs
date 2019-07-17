﻿using System.Threading.Tasks;
using System.Windows.Input;
using Wasabi.Controllers;
using Wasabi.Navigation;
using Xamarin.Forms;

namespace Wasabi.ViewModels
{
	public class MainViewModel : ViewModelBase
	{

		public MainViewModel(INavigationService navigationService) : base(navigationService)
		{
		}
		private string _passphrase { get; set; }
		public string Passphrase
		{
			get
			{
				return _passphrase;
			}
			set
			{
				_passphrase = value;
				RaisePropertyChanged(() => Passphrase);
			}
		}

		public ICommand SubmitCommand => new Command(async () => await PushMnemonicAsync());

		private async Task PushMnemonicAsync()
		{
			await _navigationService.NavigateAsync(
				"MnemonicPage",
				WalletController.GenerateMnemonicAsync(_passphrase).Result.ToString());
		}
	}
}
