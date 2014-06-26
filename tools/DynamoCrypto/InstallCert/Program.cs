using System;
using DynamoCrypto;

namespace InstallCert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You must specify the location of a certificate file to install.");
                return;
            }

            Utils.InstallCertificate(args[0]);
        }
    }
}
