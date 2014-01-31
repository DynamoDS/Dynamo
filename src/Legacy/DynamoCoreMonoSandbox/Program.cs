using System;
using Dynamo;

namespace DynamoREPL
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var env = new Dynamo.FSchemeInterop.ExecutionEnvironment ();
			FScheme.REPL (true);
		}
	}
}
