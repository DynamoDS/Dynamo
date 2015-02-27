using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.RevitAddIns;

namespace RevitInstallDetective
{
    public static class Program
    {
        // if the version asked for is present, return status code 0
        // otherwise return status code -1
        static int Main(string[] args)
        {
            if (args.Length == 0) return -1;

            try
            {
                return Detective.InstallationExists(args[0]) ? 0 : -1;
            }
            catch
            {
                // suppress all errors!
            }

            return -1;
        }
    }
}
