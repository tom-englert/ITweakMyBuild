namespace ITweakMyBuild
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Interaction logic for BuildNotificationControl.xaml
    /// </summary>
    public partial class BuildNotificationControl
    {
        public BuildNotificationControl()
        {
            InitializeComponent();
        }

        // pack uri won't find the image since extensions are loaded dynamically ...
        public static ImageSource ImageSource => Properties.Resources.Icon16.ToBitmapSource();
    }

    internal static class BitmapExtensions
    {
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            using (var handle = new SafeHBitmapHandle(source))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            public SafeHBitmapHandle(Bitmap bitmap)
                : base(true)
            {
                SetHandle(bitmap.GetHbitmap());
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                return NativeMethods.DeleteObject(handle) > 0;
            }

            private static class NativeMethods
            {

                [DllImport("gdi32")]
                public static extern int DeleteObject(IntPtr o);
            }
        }
    }
}
