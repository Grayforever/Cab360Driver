using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.IO;
using Java.Lang;
using Math = System.Math;

namespace Cab360Driver.FaceDetectorUtils.Common
{
    public class CameraSourcePreview : ViewGroup
    {
        string TAG = "CameraSourcePreview";
        private Context context;
        private SurfaceView surfaceView;
        private bool startRequested;
        private bool surfaceAvailable;
        private CameraSource cameraSource;

        private GraphicOverlay overlay;

        private string CAMERA_PERMISSION = Manifest.Permission.Camera;

        private bool showRequestPopup;
        private AndroidX.Fragment.App.Fragment currentFragment;
        public bool isActionPending = true;
        private bool isPermissionGranted;
        private bool StartRequested;

        IRunnable mCameraPermissionSender;

        public int PERMISSION_REQUEST_CAMERA { get; private set; }

        public CameraSourcePreview(Context context, IAttributeSet attrs): base(context, attrs)
        {

            this.context = context;
            startRequested = false;
            surfaceAvailable = false;

            surfaceView = new SurfaceView(context);
            surfaceView.Holder.AddCallback(new SurfaceCallback());
            AddView(surfaceView);
        }

        public void Start(CameraSource cameraSource)
        {
            if (cameraSource == null) 
            {
              Stop();
            }

            this.cameraSource = cameraSource;

            if (this.cameraSource != null) {
                StartRequested = true;
                StartIfReady();
            }
        }

        public void Start(CameraSource cameraSource, GraphicOverlay overlay)
        {
            this.overlay = overlay;
                Start(cameraSource);
        }

        public void Stop()
        {
            if (cameraSource != null)
            {
                cameraSource.Stop();
            }
        }

        public void release()
        {
            if (cameraSource != null)
            {
                cameraSource.Release();
                cameraSource = null;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int previewWidth = 320;
            int previewHeight = 240;
            if (cameraSource != null)
            {
                Android.Gms.Common.Images.Size size = cameraSource.PreviewSize;
                if (size != null)
                {
                    previewWidth = size.Width;
                    previewHeight = size.Height;
                }
            }

            // Swap width and height sizes when in portrait, since it will be rotated 90 degrees
            if (IsPortraitMode())
            {
                int tmp = previewWidth;
                previewWidth = previewHeight;
                previewHeight = tmp;
            }

            int viewWidth = r - l;
            int viewHeight = b - t;

            int childWidth;
            int childHeight;
            int childXOffset = 0;
            int childYOffset = 0;
            float widthRatio = (float)viewWidth / (float)previewWidth;
            float heightRatio = (float)viewHeight / (float)previewHeight;

            if (widthRatio > heightRatio)
            {
                childWidth = viewWidth;
                childHeight = (int)((float)previewHeight * widthRatio);
                childYOffset = (childHeight - viewHeight) / 2;
            }
            else
            {
                childWidth = (int)((float)previewWidth * heightRatio);
                childHeight = viewHeight;
                childXOffset = (childWidth - viewWidth) / 2;
            }

            for (int i = 0; i < ChildCount; ++i)
            {
                GetChildAt(i).Layout(-1 * childXOffset, -1 * childYOffset, childWidth - childXOffset, childHeight - childYOffset);
            }

            try
            {
                StartIfReady();
            }
            catch (IOException e)
            {
                Log.Error(TAG, "Could not start camera source.", e);
            }
        }

        private bool IsPortraitMode()
        {
            var orientation = (int)context.Resources.Configuration.Orientation;
            if (orientation == (int)Android.Content.Res.Orientation.Landscape)
            {
                return false;
            }
            if (orientation == (int)Android.Content.Res.Orientation.Portrait)
            {
                return true;
            }

            Log.Debug(TAG, "isPortraitMode returning false by default");
            return false;
        }

        private void StartIfReady()
        {
            if (startRequested && surfaceAvailable)
            {
                cameraSource.Start();
                if (overlay != null) 
                {
                    Android.Gms.Common.Images.Size size = cameraSource.PreviewSize;
                    int min = Math.Min(size.Width, size.Height);
                    int max = Math.Max(size.Width, size.Height);
                    if (IsPortraitMode()) {
                        
                        overlay.SetCameraInfo(min, max, cameraSource.CameraFacing);
                    } 
                    else 
                    {
                        overlay.SetCameraInfo(max, min, cameraSource.CameraFacing);
                    }
                    overlay.Clear();
                }
              startRequested = false;
            }
        }

        public bool IsPermissionGranted()
        {
            return IsPermissionGranted(showRequestPopup);
        }

        public bool IsPermissionGranted(bool showRequestPopup)
        {
            return IsPermissionGranted(showRequestPopup, null);
        }

        public bool IsPermissionGranted(bool showRequestPopup, IRunnable mCameraPermissionSender)
        {

            this.mCameraPermissionSender = mCameraPermissionSender;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (ContextCompat.CheckSelfPermission(context, CAMERA_PERMISSION) == Permission.Granted)
                {
                    Log.Verbose(TAG, "Permission is granted");
                    return true;
                }
                else
                {

                    Log.Verbose(TAG, "Permission is revoked");
                    if (showRequestPopup)
                    {
                        if (currentFragment != null)
                        {
                            currentFragment.RequestPermissions(new string[] { CAMERA_PERMISSION }, PERMISSION_REQUEST_CAMERA);
                        }
                        else
                            ActivityCompat.RequestPermissions((Activity)context, new string[] { CAMERA_PERMISSION }, PERMISSION_REQUEST_CAMERA);
                    }
                    return false;
                }
            }
            else
            {
                //permission is automatically granted on sdk<23 upon installation
                Log.Verbose(TAG, "Permission is granted");
                return true;
            }
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
        {
            if (requestCode == PERMISSION_REQUEST_CAMERA && grantResults.Length > 0)
            if (grantResults[0] == (int)Permission.Granted)
            {
                isPermissionGranted = true;
                if (isActionPending)
                {
                    isActionPending = false;

                    if (mCameraPermissionSender != null)
                        new Thread(mCameraPermissionSender).Start();
                }
            }
            else
            {
                if (currentFragment != null)
                {
                    if (!currentFragment.ShouldShowRequestPermissionRationale(CAMERA_PERMISSION))
                    {
                        ShowRequestPermissionRationale();
                    }
                    else
                    {
                        Log.Debug(TAG, "onRequestPermissionsResult: Permission is not provided");

                    }
                }
                else
                {
                    if (!ActivityCompat.ShouldShowRequestPermissionRationale((Activity)context, CAMERA_PERMISSION))
                    {
                        ShowRequestPermissionRationale();
                    }
                    else
                    {
                        Log.Debug(TAG, "onRequestPermissionsResult: Permission is not provided");

                    }
                }
            }
        }

        public void ShowRequestPermissionRationale()
        {
            Log.Info("Go to settings", "and enable permissions");

            // To redirect the settings-> app details page

            Intent intent = new Intent();
            intent.SetAction(Settings.ActionApplicationDetailsSettings);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
            intent.SetData(uri);
            context.StartActivity(intent);
        }

        public void setShowRequestPopup(bool showRequestPopup)
        {
            this.showRequestPopup = showRequestPopup;
        }

        // To show the source is Activity or Fragment

        public void SetCurrentFragment(AndroidX.Fragment.App.Fragment currentFragment)
        {
            this.currentFragment = currentFragment;
        }

        public class SurfaceCallback : Java.Lang.Object, ISurfaceHolderCallback
        {
            public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
            {

            }

            public void SurfaceCreated(ISurfaceHolder holder)
            {
                surfaceAvailable = true;
                try
                {
                    StartIfReady();
                }
                catch (IOException e)
                {
                    Log.Error(TAG, "Could not start camera source.", e);
                }
            }

            public void SurfaceDestroyed(ISurfaceHolder holder)
            {
                surfaceAvailable = false;
            }
        }
    }
    
}