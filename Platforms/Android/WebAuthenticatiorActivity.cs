using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace StudyFlow
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.companyname.studyflow",
        DataHost = "oauth2redirect")]
    public class MauiWebAuthenticatorActivity : WebAuthenticatorCallbackActivity
    {
    }
}