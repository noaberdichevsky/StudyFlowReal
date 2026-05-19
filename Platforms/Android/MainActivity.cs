using Android.App;
using Android.Content;
using Android.Content.PM;

namespace StudyFlow
{
    [Activity(Theme = "@style/Maui.SplashTheme",
               MainLauncher = true,
               LaunchMode = LaunchMode.SingleTop,
               ConfigurationChanges = ConfigChanges.ScreenSize |
                                      ConfigChanges.Orientation |
                                      ConfigChanges.UiMode)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "studyflowauth",
        DataHost = "callback"
    )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            HandleIntent(Intent);
        }

        private void HandleIntent(Intent? intent)
        {
            var url = intent?.Data?.ToString();
            System.Diagnostics.Debug.WriteLine($"HandleIntent: {url}");
            if (!string.IsNullOrEmpty(url) && url.Contains("code="))
            {
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');
                foreach (var part in query.Split('&'))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2 && kv[0] == "code")
                    {
                        var code = Uri.UnescapeDataString(kv[1]);
                        System.Diagnostics.Debug.WriteLine($"Got code: {code}");
                        intent!.SetData(null);
                        SecureStorage.Default.SetAsync("oauth_code", code).Wait();
                        break;
                    }
                }
            }
        }
    }
}