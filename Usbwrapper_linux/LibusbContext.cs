using System;
using System.Runtime.InteropServices;

namespace Usbwrapper_linux
{
    internal class LibusbContext : SafeHandle
    {
        private LibusbContext() : base(IntPtr.Zero,true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return (handle == IntPtr.Zero);
            }
        }

        [DllImport("libusb-1.0", EntryPoint = "libusb_exit")]
        /// <summary>
        /// called with the context when closing
        /// </summary>
        static extern void libusbExit(IntPtr ctx);

        override protected bool ReleaseHandle()
        {
            libusbExit(handle);
            return true;
        }
    }
}