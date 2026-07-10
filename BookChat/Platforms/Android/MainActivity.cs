using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;

namespace BookChat
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        internal static TaskCompletionSource<string?> FolderPickTcs { get; private set; }
        const int PickFolderRequestCode = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
        }

        public Task<string?> PickFolderAsync()
        {
            FolderPickTcs = new TaskCompletionSource<string?>();

            var intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantPersistableUriPermission);
            StartActivityForResult(intent, PickFolderRequestCode);

            return FolderPickTcs.Task;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PickFolderRequestCode)
            {
                if (resultCode == Result.Ok && data != null)
                {
                    var uri = data.Data;
                    try
                    {
                        var takeFlags = data.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                        ContentResolver.TakePersistableUriPermission(uri, takeFlags);
                    }
                    catch { }

                    FolderPickTcs?.TrySetResult(uri?.ToString());
                }
                else
                {
                    FolderPickTcs?.TrySetResult(null);
                }
            }
        }
    }
}
