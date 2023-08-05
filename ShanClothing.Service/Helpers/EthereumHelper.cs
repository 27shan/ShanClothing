using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC;
using System.Numerics;
using Nethereum.Util;
using static Nethereum.Util.UnitConversion;

namespace ShanClothing.Service.Helpers
{
	public static class EthereumHelper
	{
		public static string? Keccak256(byte[] input)
		{
			try
			{
				KeccakDigest digest = new KeccakDigest(256);
				byte[] hash = new byte[digest.GetDigestSize()];

				digest.BlockUpdate(input, 0, input.Length);
				digest.DoFinal(hash, 0);

				string hashString = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
				return hashString;
			}
			catch
			{
				return null;
			}
		}

		public static async Task<decimal> GetEthToRubRate()
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					var json = await httpClient.GetStringAsync("https://api.coingecko.com/api/v3/simple/price?ids=ethereum&vs_currencies=rub");
					dynamic data = JsonConvert.DeserializeObject(json);
					var rate = (decimal)data.ethereum.rub;
					return rate;
				}
			}
			catch
			{
				return 0;
			}
		}

		public static async Task<BigInteger> ConvertRubToWei(decimal inputRub)
		{
			try
			{
				decimal rate = await GetEthToRubRate();
				decimal eth = inputRub / rate;
				BigInteger outputWei = UnitConversion.Convert.ToWei(eth, EthUnit.Ether);
				return outputWei;
			}
			catch
			{
				return BigInteger.Zero;
			}
		}

		public static async Task<PaymentEthereum?> GetPaymentEthereum(string orderId)
		{
			try
			{
				var web3 = new Web3("https://eth-sepolia.g.alchemy.com/v2/2aR6URY11RqetrqX5eZh2zDs3SgRBHja");
				var contractAddress = "0xe4f506d6c53b032edeab33db7d26e1807903afbd";
				var contractABI = @"[
				{
					""inputs"": [],
					""stateMutability"": ""nonpayable"",
					""type"": ""constructor""
				},
				{
					""inputs"": [],
					""name"": ""getBalance"",
					""outputs"": [
						{
							""internalType"": ""uint256"",
							""name"": """",
							""type"": ""uint256""
						}
					],
					""stateMutability"": ""view"",
					""type"": ""function""
				},
				{
					""inputs"": [
						{
							""internalType"": ""string"",
							""name"": ""orderId"",
							""type"": ""string""
						}
					],
					""name"": ""getPayment"",
					""outputs"": [
						{
							""components"": [
								{
									""internalType"": ""uint256"",
									""name"": ""amount"",
									""type"": ""uint256""
								},
								{
									""internalType"": ""uint256"",
									""name"": ""timestamp"",
									""type"": ""uint256""
								},
								{
									""internalType"": ""address"",
									""name"": ""from"",
									""type"": ""address""
								}
							],
							""internalType"": ""struct PaymentShanClothing.Payment"",
							""name"": """",
							""type"": ""tuple""
						}
					],
					""stateMutability"": ""view"",
					""type"": ""function""
				},
				{
					""inputs"": [
						{
							""internalType"": ""string"",
							""name"": ""orderId"",
							""type"": ""string""
						}
					],
					""name"": ""pay"",
					""outputs"": [],
					""stateMutability"": ""payable"",
					""type"": ""function""
				},
				{
					""inputs"": [
						{
							""internalType"": ""address payable"",
							""name"": ""_to"",
							""type"": ""address""
						},
						{
							""internalType"": ""uint256"",
							""name"": ""amount"",
							""type"": ""uint256""
						}
					],
					""name"": ""withdraw"",
					""outputs"": [],
					""stateMutability"": ""payable"",
					""type"": ""function""
				},
				{
					""inputs"": [
						{
							""internalType"": ""address payable"",
							""name"": ""_to"",
							""type"": ""address""
						}
					],
					""name"": ""withdrawAll"",
					""outputs"": [],
					""stateMutability"": ""payable"",
					""type"": ""function""
				}]";
				var contract = web3.Eth.GetContract(contractABI, contractAddress);
				var function = contract.GetFunction("getPayment");
				var result = await function.CallDeserializingToObjectAsync<PaymentEthereum>(orderId);
				return result;
			}
			catch(Exception ex)
			{
				return null;
			}
		}
	}
}
