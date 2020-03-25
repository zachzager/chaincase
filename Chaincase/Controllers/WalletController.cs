﻿using NBitcoin;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WalletWasabi.Blockchain.Keys;
using WalletWasabi.Logging;

namespace Chaincase.Controllers
{
	public static class WalletController
	{
		public static Mnemonic GenerateMnemonic(string passphrase, NBitcoin.Network network)
		{
			string walletFilePath = Path.Combine(Global.WalletsDir, $"{network.ToString()}.json");
			KeyManager.CreateNew(out Mnemonic mnemonic, passphrase, walletFilePath);
			return mnemonic;
		}

		public static bool VerifyWalletCredentials(string mnemonicString, string passphrase, NBitcoin.Network network)
		{
			Mnemonic mnemonic = new Mnemonic(mnemonicString);
			ExtKey derivedExtKey = mnemonic.DeriveExtKey(passphrase);

			string walletFilePath = Path.Combine(Global.WalletsDir, $"{network.ToString()}.json");
			ExtKey keyOnDisk;
			try
			{
				keyOnDisk = KeyManager.FromFile(walletFilePath).GetMasterExtKey(passphrase);
			}
			catch
			{
				// bad password
				return false;
			}
			return keyOnDisk.Equals(derivedExtKey);
		}

		public static async Task LoadWalletAsync(NBitcoin.Network network)
		{
			// TODO Nono backup wallet folder!!
			string walletFilePath = Global.GetWalletFullPath(network.ToString());
			KeyManager keyManager = Global.LoadKeyManager(walletFilePath);
			try
			{
				await Global.InitializeWalletServiceAsync(keyManager);
			}
			catch (Exception ex)
			{
				// Initialization failed.
				Logger.LogError(ex, "WalletController");
				await Global.DisposeInWalletDependentServicesAsync();
			}
		}

		public static bool WalletExists(NBitcoin.Network network)
		{
			string walletFilePath = Global.GetWalletFullPath(network.ToString());
			return File.Exists(walletFilePath);
		}

		public static Money GetBalance()
		{
			return Enumerable.Where
				(
					Global.WalletService.Coins,
					c => c.Unspent && !c.SpentAccordingToBackend
				).Sum(c => (long?)c.Amount) ?? 0;
		}

        public static Money GetPrivateBalance()
        {
			return Enumerable.Where
				(
					Global.WalletService.Coins,
					c => c.Unspent && !c.SpentAccordingToBackend && c.AnonymitySet > 1
				).Sum(c => (long?)c.Amount) ?? 0;
		}

		public static Boolean SendTransaction(string addressString, FeeRate rate)
		{
			BitcoinAddress address;
			try
			{
				address = BitcoinAddress.Create(addressString.Trim(), Global.Network);
			}
			catch (FormatException e)
			{
				return false;
			}

			return true;
		}
	}
}
