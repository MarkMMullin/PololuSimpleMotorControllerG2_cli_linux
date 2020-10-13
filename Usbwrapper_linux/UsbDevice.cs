using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Usbwrapper_linux
{
    public abstract class UsbDevice : IDisposable
    {
        protected ushort getProductID()
        {
            return LibUsb.getDeviceDescriptor(deviceHandle).idProduct;
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public String getSerialNumber()
        {
            return LibUsb.getSerialNumber(deviceHandle);
        }


        protected unsafe void controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index)
        {
            int ret = libusbControlTransfer(deviceHandle, RequestType, Request,
                Value, Index, (byte*) 0, 0, (ushort) 5000);
            LibUsb.throwIfError(ret, "Control transfer failed");
        }

        protected unsafe uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, byte[] data)
        {
            fixed (byte* pointer = data)
            {
                return controlTransfer(RequestType, Request,
                    Value, Index, pointer, (ushort) data.Length);
            }
        }

        protected unsafe uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, void* data,
            ushort length)
        {
            int ret = libusbControlTransfer(deviceHandle, RequestType, Request,
                Value, Index, data, length, (ushort) 5000);
            LibUsb.throwIfError(ret, "Control transfer failed");
            return (uint) ret;
        }

        IntPtr privateDeviceHandle;

        internal IntPtr deviceHandle
        {
            get { return privateDeviceHandle; }
        }

        /// <summary>
        /// Create a usb device from a deviceListItem
        /// </summary>
        /// <param name="handles"></param>
        protected UsbDevice(DeviceListItem deviceListItem)
        {
            LibUsb.throwIfError(libusbOpen(deviceListItem.devicePointer, out privateDeviceHandle),
                "Error connecting to device.");
        }

        /// <summary>
        /// disconnects from the usb device.  This is the same as Dispose().
        /// </summary>
        public void disconnect()
        {
            libusbClose(deviceHandle);
        }

        /// <summary>
        /// Disconnects from the USB device, freeing all resources
        /// that were allocated when the connection was made.
        /// This is the same as disconnect().
        /// </summary>
        public void Dispose()
        {
            disconnect();
        }

        [DllImport("libusb-1.0", EntryPoint = "libusb_control_transfer")]
        /// <returns>the number of bytes transferred or an error code</returns>
        static extern unsafe int libusbControlTransfer(IntPtr device_handle, byte requesttype,
            byte request, ushort value, ushort index,
            void* bytes, ushort size, uint timeout);

        [DllImport("libusb-1.0", EntryPoint = "libusb_get_device_descriptor")]
        internal static extern int libusbGetDeviceDescriptor(IntPtr device,
            out LibusbDeviceDescriptor device_descriptor);

        [DllImport("libusb-1.0", EntryPoint = "libusb_init")]
        /// <summary>
        /// called to initialize the device context before any using any libusb functions
        /// </summary>
        /// <returns>an error code</returns>
        internal static extern int libusbInit(out LibusbContext ctx);

        [DllImport("libusb-1.0", EntryPoint = "libusb_get_device_list")]
        /// <summary>
        /// gets a list of device pointers - must be freed with libusbFreeDeviceList
        /// </summary>
        /// <returns>number of devices OR an error code</returns>
        internal static unsafe extern int libusbGetDeviceList(LibusbContext ctx, out IntPtr* list);

        [DllImport("libusb-1.0", EntryPoint = "libusb_free_device_list")]
        /// <summary>
        /// Frees a device list.  Decrements the reference count for each device by 1
        /// if the unref_devices parameter is set.
        /// </summary>
        internal static unsafe extern void libusbFreeDeviceList(IntPtr* list, int unref_devices);

        [DllImport("libusb-1.0", EntryPoint = "libusb_unref_device")]
        /// <summary>
        /// Decrements the reference count on a device.
        /// </summary>
        internal static extern void libusbUnrefDevice(IntPtr device);

        [DllImport("libusb-1.0", EntryPoint = "libusb_get_string_descriptor_ascii")]
        /// <summary>
        /// Gets the simplest version of a string descriptor
        /// </summary>
        internal static unsafe extern int libusbGetStringDescriptorASCII(IntPtr device_handle, byte index, byte* data,
            int length);

        [DllImport("libusb-1.0", EntryPoint = "libusb_open")]
        /// <summary>
        /// Gets a device handle for a device.  Must be closed with libusb_close.
        /// </summary>
        internal static extern int libusbOpen(IntPtr device, out IntPtr device_handle);

        [DllImport("libusb-1.0", EntryPoint = "libusb_close")]
        /// <summary>
        /// Closes a device handle.
        /// </summary>
        internal static extern void libusbClose(IntPtr device_handle);

        [DllImport("libusb-1.0", EntryPoint = "libusb_get_device")]
        /// <summary>
        /// Gets the device from a device handle.
        /// </summary>
        internal static extern IntPtr libusbGetDevice(IntPtr device_handle);

        /// <summary>
        /// true if the devices are the same
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool isSameDeviceAs(DeviceListItem item)
        {
            return (libusbGetDevice(deviceHandle) == item.devicePointer);
        }

        /// <summary>
        /// gets a list of devices
        /// </summary>
        /// <returns></returns>
        protected static unsafe List<DeviceListItem> getDeviceList(Guid deviceInterfaceGuid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// gets a list of devices by vendor and product ID
        /// </summary>
        /// <returns></returns>
        protected static unsafe List<DeviceListItem> getDeviceList(UInt16 vendorId, UInt16[] productIdArray)
        {
            var list = new List<DeviceListItem>();

            IntPtr* device_list;
            int count = LibUsb.throwIfError(UsbDevice.libusbGetDeviceList(LibUsb.context, out device_list),
                "Error from libusb_get_device_list.");

            int i;
            for (i = 0; i < count; i++)
            {
                IntPtr device = device_list[i];

                foreach (UInt16 productId in productIdArray)
                {
                    if (LibUsb.deviceMatchesVendorProduct(device, vendorId, productId))
                    {
                        IntPtr device_handle;
                        LibUsb.throwIfError(UsbDevice.libusbOpen(device, out device_handle),
                            "Error connecting to device to get serial number (" + (i + 1) + " of " + count + ", " +
                            device.ToString("x8") + ").");

                        string serialNumber = LibUsb.getSerialNumber(device_handle);
                        list.Add(new DeviceListItem(device, "#" + serialNumber, serialNumber, productId));

                        UsbDevice.libusbClose(device_handle);
                    }
                }
            }


            // Free device list without unreferencing.
            // Unreference/free the individual devices in the
            // DeviceListItem destructor.
            UsbDevice.libusbFreeDeviceList(device_list, 0);

            return list;
        }

        //protected AsynchronousInTransfer newAsynchronousInTransfer(byte endpoint, uint size, uint timeout)
        //{
        //    return new AsynchronousInTransfer(this, endpoint, size, timeout);
        //}
    }
}
