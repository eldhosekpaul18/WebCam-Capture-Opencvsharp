using DirectShowLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Camera_Configuration
{
    public static class Camera
    {
        #region Public Properties

        public static VideoCapture CaptureImage { get; set; }

        #endregion Public Properties

        #region Public Methods

        #region Init
        /// <summary>
        /// Initialize openCV VideoCapture object to start camera
        /// </summary>
        public static string Init(int cameraIndex)
        {
            string result = string.Empty;
            int width = 0;
            int height = 0;
            List<System.Drawing.Point> resolution = new List<System.Drawing.Point>();
            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            if (_SystemCamereas != null && _SystemCamereas.Length > 0)
            {
                resolution = GetAllAvailableResolution(_SystemCamereas[cameraIndex]);
            }

            if (resolution != null && resolution.Count > 0)
            {
                var highestResolution = resolution.OrderByDescending(p => p.X).FirstOrDefault();
                width = Convert.ToInt32(highestResolution.X);
                height = Convert.ToInt32(highestResolution.Y);
                result = "Resolution(device) :- " + width + "X" + height;
            }
            else
            {
                width = 640;
                height = 480;
                result = "Resolution(default) :- " + width + "X" + height;
            }

            CaptureImage = new VideoCapture(cameraIndex);
            CaptureImage.FrameWidth = width;
            CaptureImage.FrameHeight = height;
            return result;
        }
        #endregion Init 

        public static void ResetCamera()
        {
            if (CaptureImage != null)
            {
                CaptureImage.Release();
                CaptureImage = null;
            }
        }

        #region ScanImage
        /// <summary>
        /// Scan the image 
        /// </summary>
        /// <param name="scannedImageName">Image location with name</param>
        /// <returns>Scan Status</returns>
        public static Bitmap ScanImage()
        {
            Bitmap imgOriginal = null;

            Mat frame = new Mat();

            if (CaptureImage != null)
            {
                CaptureImage.Read(frame);
                if (frame != null && frame.Width > 0 && frame.Height > 0)
                {
                    imgOriginal = frame.ToBitmap();
                    imgOriginal.SetResolution(300.0F, 300.0F);
                    frame.Dispose();
                }
            }

            return imgOriginal;
        }
        #endregion ScanImage

        #region DetectCamerasConnected
        /// <summary>
        /// Find the cameras connected to system
        /// </summary>
        /// <returns></returns>
        public static List<Item> DetectCamerasConnected()
        {
            List<Item> ListCamerasData = new List<Item>();
            int _DeviceIndex = 0;

            try
            {
                DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                foreach (DsDevice _Camera in _SystemCamereas)
                {
                    ListCamerasData.Add(new Item() { Value = _DeviceIndex, Text = _Camera.Name });
                    _DeviceIndex++;
                }
            }
            catch (Exception ex)
            {

            }
            return ListCamerasData;
        }
        #endregion DetectCamerasConnected

        #endregion  Public Methods

        #region  Private Methods

        #region GetAllAvailableResolution
        /// <summary>
        /// Gets the Available Resolutions the camera
        /// </summary>
        /// <param name="vidDev">Device</param>
        /// <returns>List of Resolutions</returns>
        private static List<System.Drawing.Point> GetAllAvailableResolution(DsDevice vidDev)
        {
            var AvailableResolutions = new List<System.Drawing.Point>();

            try
            {
                int hr;
                int max = 0;
                int bitCount = 0;

                IBaseFilter sourceFilter = null;

                var m_FilterGraph2 = new FilterGraph() as IFilterGraph2;

                hr = m_FilterGraph2.AddSourceFilterForMoniker(vidDev.Mon, null, vidDev.Name, out sourceFilter);

                var pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);

                VideoInfoHeader v = new VideoInfoHeader();
                IEnumMediaTypes mediaTypeEnum;
                hr = pRaw2.EnumMediaTypes(out mediaTypeEnum);

                AMMediaType[] mediaTypes = new AMMediaType[1];
                IntPtr fetched = IntPtr.Zero;
                hr = mediaTypeEnum.Next(1, mediaTypes, fetched);

                while (fetched != null && mediaTypes[0] != null)
                {
                    Marshal.PtrToStructure(mediaTypes[0].formatPtr, v);
                    if (v.BmiHeader.Size != 0 && v.BmiHeader.BitCount != 0)
                    {
                        if (v.BmiHeader.BitCount > bitCount)
                        {
                            AvailableResolutions.Clear();
                            max = 0;
                            bitCount = v.BmiHeader.BitCount;
                        }
                        AvailableResolutions.Add(new System.Drawing.Point(v.BmiHeader.Width, v.BmiHeader.Height));
                        if (v.BmiHeader.Width > max || v.BmiHeader.Height > max)
                            max = (Math.Max(v.BmiHeader.Width, v.BmiHeader.Height));
                    }
                    hr = mediaTypeEnum.Next(1, mediaTypes, fetched);
                }
                return AvailableResolutions;
            }
            catch (Exception ex)
            {

            }
            return AvailableResolutions;
        }
        #endregion GetAllAvailableResolution

        #endregion  Private Methods
    }

    public class Item
    {
        public int Value { set; get; }
        public string Text { set; get; }
    }
}
