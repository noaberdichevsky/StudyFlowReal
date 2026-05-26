using Android.Content;
using StudyFlow.Service;
using System.Security.Cryptography;
using System.Text;

namespace StudyFlow
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private static string _codeVerifier = string.Empty;

        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string GenerateCodeChallenge(string verifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            return Convert.ToBase64String(hash)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static async Task<string> ExchangeCodeForToken(string code)
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "842354198030-9geod1hq8c9eq5kgnsdbc7kn6pnimain.apps.googleusercontent.com"),
                new KeyValuePair<string, string>("client_secret", "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"),
                new KeyValuePair<string, string>("redirect_uri", "https://noaberdichevsky.github.io/studyflow-auth/auth.html"),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code_verifier", _codeVerifier),
            });

            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Token response: {json}");

            var tokenData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            if (tokenData.TryGetProperty("access_token", out var accessToken))
                return accessToken.GetString() ?? string.Empty;

            return string.Empty;
        }

        public async Task<string> SignInAsync()
        {
            SecureStorage.Default.Remove("oauth_code");

            _codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);

            var url = "https://accounts.google.com/o/oauth2/v2/auth" +
                "?client_id=842354198030-9geod1hq8c9eq5kgnsdbc7kn6pnimain.apps.googleusercontent.com" +
                "&redirect_uri=https%3A%2F%2Fnoaberdichevsky.github.io%2Fstudyflow-auth%2Fauth.html" +
                "&response_type=code" +
                "&scope=email%20profile%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.courses.readonly%20https%3A%2F%2Fwww" +
                ".googleapis.com%2Fauth%2Fclassroom.coursework.me.readonly%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.coursework.students" +
                "&code_challenge=" + codeChallenge +
                "&code_challenge_method=S256" +
                "&prompt=select_account";

            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!;
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            intent.AddFlags(ActivityFlags.NewTask);
            activity.StartActivity(intent);

            var timeout = Task.Delay(180000);
            while (!timeout.IsCompleted)
            {
                await Task.Delay(1000);
                try
                {
                    var code = await SecureStorage.Default.GetAsync("oauth_code");
                    System.Diagnostics.Debug.WriteLine($"SecureStorage code: '{code}'");
                    if (!string.IsNullOrEmpty(code) && code.Length > 10)
                    {
                        SecureStorage.Default.Remove("oauth_code");
                        return await ExchangeCodeForToken(code);
                    }
                }
                catch { }
            }

            return string.Empty;
        }
    }
}