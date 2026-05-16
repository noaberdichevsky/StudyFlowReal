using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service.DBService.FireBase
{
    internal class FireBaseAuthService:IAuthService
    {
        private FirebaseAuthClient? _authClient;
        private IAppLogger _logger;
        public FireBaseAuthService(IAppLogger logger)
        {
            _logger = logger;

            var config = new FirebaseAuthConfig()
            {
                // current_api_key from google-services.json
                ApiKey = "AIzaSyCXcogsbrbnmObz0i4tgKtxD5XVwGVechw",

                // project_id from google-services.json + ".firebaseapp.com"
                AuthDomain = "studyflowdb.firebaseapp.com",

                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
            };
            _authClient = new FirebaseAuthClient(config);
        }
        public async Task<string> SignIn(string userEmail, string userPassword)
        {
            string errorMessage = string.Empty;
            try
            {
                await _authClient!.SignInWithEmailAndPasswordAsync(userEmail, userPassword);
                return _authClient.User.Info.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                    errorMessage = "Incorrect email or password!";
                else
                    errorMessage = "SignIn failed: Unknown exception!";

                _logger.LogDebug($"SignIn failed: {userEmail}, {errorMessage}");
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"SignIn failed: {ex.Message}");
                throw new Exception("SignIn failed!");
            }
        }

        public async Task<string> CreateAuth(string userEmail, string userPassword)
        {
            try
            {
                await _authClient!.CreateUserWithEmailAndPasswordAsync(userEmail, userPassword);
                _logger.LogDebug($"User {userEmail} created successfully");
                return _authClient.User.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                string errorMessage = string.Empty;

                if (ex.Message.Contains("INVALID_EMAIL"))
                    errorMessage = "Invalid email address!";
                else if (ex.Message.Contains("EMAIL_EXISTS"))
                    errorMessage = "This email already exists!";
                else if (ex.Message.Contains("WEAK_PASSWORD"))
                    errorMessage = "Weak password!";

                _logger.LogDebug($"CreateAuth failed: {ex.Message}");
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"CreateAuth failed: {ex.Message}");
                throw new Exception("SignUp failed!");
            }
        }

        public async Task RemoveAuth(string userEmail, string userPassword)
        {
            try
            {
                await _authClient!.SignInWithEmailAndPasswordAsync(userEmail, userPassword);
                await _authClient.User.DeleteAsync();
                _logger.LogDebug($"User {userEmail} removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"RemoveAuth failed: {ex.Message}");
                throw new Exception("Remove user failed!");
            }
        }

        public Task SignOut()
        {
            throw new NotImplementedException();
        }
    }
}


