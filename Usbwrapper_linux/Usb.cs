using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Usbwrapper_linux
{
public static class Usb
    {
        public static int WM_DEVICECHANGE { get { return 0; } }        

        public static bool supportsNotify { get { return false; } }

        public static IntPtr notificationRegister(Guid guid, IntPtr handle)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a list of port names (e.g. "COM2", "COM3") for all
        /// ACM USB serial ports.  Ignores the deviceInstanceIdPrefix argument. 
        /// </param>
        /// <returns></returns>
        public static IList<String> getPortNames(String deviceInstanceIdPrefix)
        {
            IList<String> l = new List<String>();
            foreach(string s in Directory.GetFiles("/dev/"))
            {
                if(s.StartsWith("/dev/ttyACM") || s.StartsWith("/dev/ttyUSB"))
                    l.Add(s);
            }
            return l;
        }

        public static void check()
        {
            LibUsb.handleEvents();
        }
    }

    internal static class LibUsb
    {
        /// <summary>
        /// Raises an exception if its argument is negative, with a
        /// message describing which LIBUSB_ERROR it is.
        /// </summary>
        /// <returns>the code, if it is non-negative</returns>
        internal static int throwIfError(int code)
        {
            if(code >= 0)
                return code;

            throw new Exception(LibUsb.errorDescription(code));
        }

        /// <summary>
        /// Raises an exception if its argument is negative, with a
        /// message prefixed by the message parameter and describing
        /// which LIBUSB_ERROR it is.
        /// </summary>
        /// <returns>the code, if it is non-negative</returns>
        internal static int throwIfError(int code, string message)
        {
            try
            {
                return LibUsb.throwIfError(code);
            }
            catch(Exception e)
            {
                throw new Exception(message, e);
            }
        }

        internal static string errorDescription(int error)
        {
            switch(error)
            {
            case -1:
                return "I/O error.";
            case -2:
                return "Invalid parameter.";
            case -3:
                return "Access denied.";
            case -4:
                return "Device does not exist.";
            case -5:
                return "No such entity.";
            case -6:
                return "Busy.";
            case -7:
                return "Timeout.";
            case -8:
                return "Overflow.";
            case -9:
                return "Pipe error.";
            case -10:
                return "System call was interrupted.";
            case -11:
                return "Out of memory.";
            case -12:
                return "Unsupported/unimplemented operation.";
            case -99:
                return "Other error.";
            default:
                return "Unknown error code " + error + ".";
            };
        }

        /// <summary>
        /// Do not use directly.  The property below initializes this
        /// with libusbInit when it is first used.
        /// </summary>
        private static LibusbContext privateContext;

        internal static LibusbContext context
        {
            get
            {
                if(privateContext == null || privateContext.IsInvalid)
                {
                    LibUsb.throwIfError(UsbDevice.libusbInit(out privateContext));
                }
                return privateContext;
            }
        }

        internal static void handleEvents()
        {
            LibUsb.throwIfError(libusb_handle_events(context));
        }

        [DllImport("libusb-1.0")]
        static unsafe extern int libusb_handle_events(LibusbContext ctx);

        /// <returns>the serial number</returns>
        internal static unsafe string getSerialNumber(IntPtr device_handle)
        {
            LibusbDeviceDescriptor descriptor = getDeviceDescriptor(device_handle);
            byte[] buffer = new byte[100];
            int length;
            fixed(byte* p = buffer)
            {
                length = LibUsb.throwIfError(UsbDevice.libusbGetStringDescriptorASCII(device_handle, descriptor.iSerialNumber, p, buffer.Length), "Error getting serial number string from device (pid="+descriptor.idProduct.ToString("x")+", vid="+descriptor.idVendor.ToString("x")+").");
            }

            String serial_number = "";
            for(int i=0;i<length;i++)
            {
                serial_number += (char)buffer[i];
            }
            return serial_number;
        }

        /// <returns>true iff the vendor and product ids match the device</returns>
        internal static bool deviceMatchesVendorProduct(IntPtr device, ushort idVendor, ushort idProduct)
        {
            LibusbDeviceDescriptor descriptor = getDeviceDescriptorFromDevice(device);
            return idVendor == descriptor.idVendor && idProduct == descriptor.idProduct;
        }

        /// <returns>the device descriptor</returns>
        internal static LibusbDeviceDescriptor getDeviceDescriptor(IntPtr device_handle)
        {
            return getDeviceDescriptorFromDevice(UsbDevice.libusbGetDevice(device_handle));
        }
        
        /// <returns>the device descriptor</returns>
        static LibusbDeviceDescriptor getDeviceDescriptorFromDevice(IntPtr device)
        {
            LibusbDeviceDescriptor descriptor;
            LibUsb.throwIfError(UsbDevice.libusbGetDeviceDescriptor(device, out descriptor),
                               "Failed to get device descriptor");
            return descriptor;
        }

        
    }
}