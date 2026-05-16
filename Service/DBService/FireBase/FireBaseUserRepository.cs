using Firebase.Database;
using Firebase.Database.Query;
using StudyFlow.Model;
using StudyFlow.Service.DBService.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service.DBService.FireBase
{
    // This class handles all user operations in Firebase (create, read, update, delete)
    // It inherits from FirebaseRealtimeService to get the _firebaseClient connection
    // It implements IAppUserRepository so the rest of the app can use it without knowing it's Firebase
    public class FireBaseUserRepository: FireBaseRealtimeService, IAppUserRepository
    {
        // _authService handles Firebase Authentication (login/register)
        private IAuthService _authService;

        // _appLogger handles debug logging so we can see what's happening
        private IAppLogger _appLogger;

        // Constructor - receives authService and logger via Dependency Injection
        public FireBaseUserRepository(IAuthService authService, IAppLogger appLogger)
        {
            _authService = authService;
            _appLogger = appLogger;
        }

        // Signs in a user - first authenticates then fetches their data from the database
        public async Task<AppUser> SignInAsync(string userEmail, string userPassword)
        {
            try
            {
                // Step 1: Sign in with Firebase Auth and get the userId
                string userId = await _authService.SignIn(userEmail, userPassword);

                // Step 2: Use the userId to fetch full user data from Realtime Database
                AppUser appUser = await GetUserByIdAsync(userId);

                _appLogger.LogDebug($"User {userEmail} signed in successfully");
                return appUser;
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"SignIn failed: {ex.Message}");
                if (!ex.Message.Contains("Incorrect email or password"))
                    throw new Exception("SignIn failed!");
                throw new Exception(ex.Message);
            }
        }
        // Creates a new user - first creates Firebase Auth account then saves data to database
        public async Task<string> CreateAsync(AppUser appUser)
        {
            try
            {
                // Step 1: Create Firebase Auth account and get the userId
                string userId = await _authService.CreateAuth(appUser.UserEmail!, appUser.UserPassword!);

                // Step 2: Save the userId into the user object
                appUser.Id = userId;

                // Step 3: Save full user data to Realtime Database
                await RegisterAppUser(appUser);

                _appLogger.LogDebug($"User {appUser.UserEmail} created successfully");
                return userId;
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"CreateAsync failed: {ex.Message}");
                if (!ex.Message.Contains("RealTimeDB"))
                    throw new Exception(ex.Message);
                throw new Exception("SignUp failed!");
            }
        }

        // Deletes a user from both Firebase Auth and Realtime Database
        public async Task DeleteAsync(AppUser appUser)
        {
            try
            {
                // Step 1: Remove from Firebase Auth
                await _authService.RemoveAuth(appUser.UserEmail!, appUser.UserPassword!);

                // Step 2: Remove from Realtime Database using the user's Id
                await _firebaseClient!
                    .Child("users")       // go to the "users" node in the database
                    .Child(appUser.Id)    // go to this specific user's node
                    .DeleteAsync();       // delete it

                _appLogger.LogDebug($"User {appUser.UserEmail} deleted successfully");
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"DeleteAsync failed: {ex.Message}");
                throw new Exception("Delete user failed!");
            }
        }

        // Gets a single user from the database using their userId
        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            try
            {
                // Go to /users/{userId} in the database and read it as an AppUser object
                var user = await _firebaseClient!
                    .Child("users")    // go to the "users" node
                    .Child(userId)     // go to this specific user's node
                    .OnceSingleAsync<AppUser>(); // read it once as an AppUser object

                return user;
            }
            catch (FirebaseException ex)
            {
                string errorMessage;

                // Check what kind of Firebase error it is
                if (ex.Message.Contains("401") || ex.Message.Contains("Permission denied"))
                    errorMessage = "GetUserByIdAsync failed: Permissions denied!";
                else if (ex.Message.Contains("404"))
                    errorMessage = "GetUserByIdAsync failed: Wrong db path!";
                else
                    errorMessage = "GetUserByIdAsync failed: Unknown exception!";

                _appLogger.LogDebug(errorMessage);
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetUserByIdAsync failed: {ex.Message}");
            }
        }

        // Updates a user's details in the database (only FirstName, LastName, Mobile)
        public async Task UpdateAsync(AppUser appUser)
        {
            try
            {
                // PatchAsync only updates the fields we specify, leaves everything else untouched
                await _firebaseClient!
                    .Child("users")
                    .Child(appUser.Id)
                    .PatchAsync(new
                    {
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName,
                        UserMobile = appUser.UserMobile
                    });

                _appLogger.LogDebug($"User {appUser.UserEmail} updated successfully");
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"UpdateAsync failed: {ex.Message}");
                throw new Exception("Update failed!");
            }
        }

        // Saves a new user's full data to the Realtime Database
        public async Task RegisterAppUser(AppUser appUser)
        {
            try
            {
                // PutAsync writes the full user object to /users/{userId}
                await _firebaseClient!
                    .Child("users")
                    .Child(appUser.Id)
                    .PutAsync(new AppUser()
                    {
                        Id = appUser.Id,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName,
                        UserEmail = appUser.UserEmail,
                        UserPassword = appUser.UserPassword,
                        UserMobile = appUser.UserMobile,
                        RegDate = appUser.RegDate,
                        UBDate = appUser.UBDate,
                        IsAdmin = appUser.IsAdmin
                    });
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"RegisterAppUser failed: {ex.Message}");
                throw new Exception("RealTimeDB add new user failed");
            }
        }

        // Sets a user as admin in the database
        public async Task SetToAdmin(string userId)
        {
            try
            {
                // PatchAsync only updates IsAdmin field, leaves everything else untouched
                await _firebaseClient!
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(new { IsAdmin = true });

                _appLogger.LogDebug("User set as admin successfully");
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"SetToAdmin failed: {ex.Message}");
                throw new Exception("SetToAdmin failed!");
            }
        }

        // Gets all users from the database - required by IAppUserRepository interface
        public List<AppUser> GetAllAsync()
        {
            try
            {
                // Fetch all users from /users/ node in Firebase
                var users = _firebaseClient!
                    .Child("users")
                    .OnceAsync<AppUser>()
                    .Result
                    .Select(u => u.Object)
                    .ToList();

                return users;
            }
            catch (Exception ex)
            {
                _appLogger.LogDebug($"GetAllAsync failed: {ex.Message}");
                return new List<AppUser>();
            }
        }
    }
}
