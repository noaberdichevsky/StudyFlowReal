using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace StudyFlow
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.companyname.studyflow")]
    public class OAuthCallbackActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Diagnostics.Debug.WriteLine($"OAuthCallback: {Intent?.Data}");

            if (Intent?.Data != null)
            {
                var url = Intent.Data.ToString()!;
                var uri = new System.Uri(url);
                var query = uri.Query.TrimStart('?');
                var parts = query.Split('&');
                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2 && kv[0] == "code")
                    {
                        var code = System.Uri.UnescapeDataString(kv[1]);
                        SecureStorage.Default.SetAsync("oauth_code", code).GetAwaiter().GetResult();
                        System.Diagnostics.Debug.WriteLine($"Code saved: {code}");
                        break;
                    }
                }
            }

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);
            Finish();
        }
    }
}