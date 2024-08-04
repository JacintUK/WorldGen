using OpenTK.Compute.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGenerator.Compute
{
    internal class Computer
    {
        private static CLPlatform[]? platforms;
        private static CLDevice[]? devices;

        public static void Initialize()
        {
            CLResultCode result;

            result = CL.GetPlatformIds(out platforms);

            result = CL.GetDeviceIds(platforms[0], DeviceType.Gpu, out devices);
            CheckResult(result);
        }

        private static void CheckResult(CLResultCode result)
        {
            if (result != CLResultCode.Success)
            {
                Console.WriteLine("OpenCL Error: " + result.ToString());
            }
        }
    }
}
