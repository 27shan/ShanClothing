using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ShanClothing.Domain.ExternalModels
{
	[FunctionOutput]
	public class PaymentEthereum
	{
		[Parameter("uint256", "amount", 1)]
		public BigInteger Amount { get; set; }

		[Parameter("uint", "timestamp", 2)]
		public uint Timestamp { get; set; }

		[Parameter("address", "from", 3)]
		public string From { get; set; }
	}
}
