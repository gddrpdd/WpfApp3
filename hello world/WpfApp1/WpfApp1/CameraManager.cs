using DirectShowLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;


namespace CameraCaptureTool
{
    public class CameraManager
    {
        private DsDevice[] _cameras;
        private IGraphBuilder _graphBuilder;
        private IBaseFilter _captureFilter;
        private IMediaControl _mediaControl;
        private ISampleGrabber _sampleGrabber;
        private bool _isRunning = false;

        // 获取所有摄像头设备
        public string[] GetCameraDevices()
        {
            _cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            string[] deviceNames = new string[_cameras.Length];

            for (int i = 0; i < _cameras.Length; i++)
            {
                deviceNames[i] = _cameras[i].Name;
            }

            return deviceNames;
        }

        // 修正后的启动摄像头方法
        public bool StartCamera(int cameraIndex, IntPtr windowHandle)
        {
            try
            {
                if (_cameras == null || cameraIndex >= _cameras.Length)
                    return false;

                // 创建 Filter Graph
                _graphBuilder = (IGraphBuilder)new FilterGraph();
                _mediaControl = (IMediaControl)_graphBuilder;

                // 创建 Sample Grabber
                _sampleGrabber = (ISampleGrabber)new SampleGrabber();
                IBaseFilter baseGrabFlt = (IBaseFilter)_sampleGrabber;

                // 添加到 Filter Graph
                int hr = _graphBuilder.AddFilter(baseGrabFlt, "Grabber");
                if (hr < 0) return false;

                // 设置媒体类型
                AMMediaType mediaType = new AMMediaType();
                mediaType.majorType = MediaType.Video;
                mediaType.subType = MediaSubType.RGB24;
                mediaType.fixedSizeSamples = true;
                mediaType.temporalCompression = false;
                mediaType.sampleSize = 0;

                hr = _sampleGrabber.SetMediaType(mediaType);
                if (hr < 0) return false;

                //// 修正：使用 BindToObject 方法获取摄像头过滤器
                //IBaseFilter videoInput = null;
                //object sourceObject;

                //// 通过 Moniker 绑定到摄像头设备
                //_cameras[cameraIndex].Moniker.BindToObject(null, null, typeof(IBaseFilter).GUID, out sourceObject);
                //videoInput = (IBaseFilter)sourceObject;

                //// 添加摄像头过滤器到 Graph
                //hr = _graphBuilder.AddFilter(videoInput, "Video Input");
                //if (hr < 0) return false;

                //// 使用 Capture Graph Builder 连接过滤器
                //ICaptureGraphBuilder2 captureGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                //captureGraph.SetFiltergraph(_graphBuilder);

                //// 渲染视频流
                //hr = captureGraph.RenderStream(PinCategory.Capture, MediaType.Video, videoInput, null, baseGrabFlt);
                //if (hr < 0) return false;

                // 设置回调
                _sampleGrabber.SetBufferSamples(true);
                _sampleGrabber.SetOneShot(false);

                // 设置回调处理视频帧
                SampleGrabberCallback callback = new SampleGrabberCallback(this);
                _sampleGrabber.SetCallback(callback, 1);

                // 启动视频流
                hr = _mediaControl.Run();
                if (hr < 0) return false;

                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动摄像头失败: {ex.Message}");
                return false;
            }
        }

        // 停止摄像头
        public void StopCamera()
        {
            try
            {
                if (_mediaControl != null)
                {
                    _mediaControl.Stop();
                    _isRunning = false;
                }

                // 释放 COM 对象
                if (_captureFilter != null)
                {
                    Marshal.ReleaseComObject(_captureFilter);
                    _captureFilter = null;
                }

                if (_sampleGrabber != null)
                {
                    Marshal.ReleaseComObject(_sampleGrabber);
                    _sampleGrabber = null;
                }

                if (_graphBuilder != null)
                {
                    Marshal.ReleaseComObject(_graphBuilder);
                    _graphBuilder = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止摄像头失败: {ex.Message}");
            }
        }

        // 拍照功能
        public Bitmap CaptureImage()
        {
            if (!_isRunning) return null;

            try
            {
                // 这里简化实现，实际应该从 SampleGrabber 获取当前帧
                // 由于实时视频流处理较复杂，这里返回一个测试位图
                Bitmap bitmap = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.LightBlue);
                    g.DrawString($"Capture {DateTime.Now:HH:mm:ss}",
                                new Font("Arial", 20),
                                System.Drawing.Brushes.Black,
                                new PointF(10, 10));
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"拍照失败: {ex.Message}");
                return null;
            }
        }

        // 转换 Bitmap 为 BitmapImage (用于 WPF 显示)
        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        // 回调类用于处理视频帧
        private class SampleGrabberCallback : ISampleGrabberCB
        {
            private CameraManager _parent;

            public SampleGrabberCallback(CameraManager parent)
            {
                _parent = parent;
            }

            public int SampleCB(double sampleTime, IMediaSample pSample)
            {
                return 0;
            }

            public int BufferCB(double sampleTime, IntPtr pBuffer, int bufferLen)
            {
                // 这里可以处理实时视频帧数据
                // 注意：这个方法在非UI线程中执行
                return 0;
            }
        }
    }
}