using System;

namespace Usbwrapper_linux
{
    /// <summary>
    /// A class that represents a device connected to the computer.  This
    /// class can be used as an item in the device list dropdown box.
    /// </summary>
    public class DeviceListItem
    {
        private String privateText;

        /// <summary>
        /// The text to display to the user in the list to represent this
        /// device.  By default, this text is "#" + serialNumberString,
        /// but it can be changed to suit the application's needs
        /// (for example, adding model information to it).
        /// </summary>
        public String text
        {
            get
            {
                return privateText;
            }
            set
            {
                privateText = value;
            }
        }

        readonly String privateSerialNumber;

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public String serialNumber
        {
            get
            {
                return privateSerialNumber;
            }
        }

        readonly IntPtr privateDevicePointer;

        /// <summary>
        /// Gets the device pointer.
        /// </summary>
        internal IntPtr devicePointer
        {
            get
            {
                return privateDevicePointer;
            }
        }

        readonly UInt16 privateProductId;

        /// <summary>
        /// Gets the USB product ID of the device.
        /// </summary>
        public UInt16 productId
        {
            get
            {
                return privateProductId;
            }    
        }
        
        /// <summary>
        /// true if the devices are the same
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool isSameDeviceAs(DeviceListItem item)
        {
            return (devicePointer == item.devicePointer);
        }

        /// <summary>
        /// Creates an item that doesn't actually refer to a device; just for populating the list with things like "Disconnected"
        /// </summary>
        /// <param name="text"></param>
        public static DeviceListItem CreateDummyItem(String text)
        {
            var item = new DeviceListItem(IntPtr.Zero,text,"",0);
            return item;
        }

        internal DeviceListItem(IntPtr devicePointer, string text, string serialNumber, UInt16 productId)
        {
            privateDevicePointer = devicePointer;
            privateText = text;
            privateSerialNumber = serialNumber;
            privateProductId = productId;
        }

        ~DeviceListItem()
        {
            if(privateDevicePointer != IntPtr.Zero)
                UsbDevice.libusbUnrefDevice(privateDevicePointer);
        }
    }

}