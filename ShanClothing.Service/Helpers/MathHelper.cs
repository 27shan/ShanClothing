using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Helpers
{
	public static class MathHelper
	{
		public static bool IsApproximatelyEqual(BigInteger value1, BigInteger value2, BigInteger tolerance)
		{
			BigInteger difference = BigInteger.Abs(value1 - value2);
			return difference <= tolerance;
		}
	}
}
