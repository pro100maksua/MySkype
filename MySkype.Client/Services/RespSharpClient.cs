using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MySkype.Client.Models;
using RestSharp;

namespace MySkype.Client.Services
{
    public class RestSharpClient
    {
        private readonly string _token;
        private readonly RestClient _restClient = new RestClient("http://localhost:5000/");

        public RestSharpClient(string token)
        {
            _token = token;
        }


        public async Task<List<User>> GetUsersAsync(string searchQuery)
        {
            var request = new RestRequest("/api/users", Method.GET);
            request.AddQueryParameter("searchQuery", searchQuery);
            request.AddHeader("Authorization", "Bearer " + _token);

            var response = await _restClient.ExecuteGetTaskAsync<List<User>>(request);

            return response.Data;
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            var request = new RestRequest("/api/users/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            request.AddHeader("Authorization", "Bearer " + _token);

            var response = await _restClient.ExecuteGetTaskAsync<User>(request);

            return response.Data;
        }

        public async Task<byte[]> GetPhotoAsync(Guid avatarId)
        {
                var request = new RestRequest("/api/photos/{id}", Method.GET);
                request.AddUrlSegment("id", avatarId.ToString());
                request.AddHeader("Authorization", "Bearer " + _token);

                var file = _restClient.DownloadData(request);

                return file;
            
        }

        public async Task<List<User>> GetFriendsAsync()
        {
            var request = new RestRequest("/api/user/friends", Method.GET);
            request.AddHeader("Authorization", "Bearer " + _token);
            var response = await _restClient.ExecuteGetTaskAsync<List<User>>(request);

            return response.Data;
        }

        public async Task<bool> AddFriendAsync(Guid friendId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}", Method.PUT);
            request.AddUrlSegment("friendId", friendId);
            request.AddHeader("Authorization", "Bearer " + _token);

            var response = await _restClient.ExecuteTaskAsync<bool>(request);

            return response.Data;
        }

        public async Task<string> RequestTokenAsync(string login, string password)
        {
            var tokenRequest = new { Login = login, Password = password };
            var request = new RestRequest("/api/identity", Method.POST);
            request.AddJsonBody(tokenRequest);

            var response = await _restClient.ExecutePostTaskAsync<string>(request);

            return response.StatusCode == HttpStatusCode.OK ? response.Data : null;
        }

        public async Task SendAudioCallRequestAsync(Guid targetId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}/call", Method.POST);
            request.AddUrlSegment("friendId", targetId);
            request.AddHeader("Authorization", "Bearer " + _token);

            await _restClient.ExecuteTaskAsync(request);
        }

        public async Task ConfirmAudioCallAsync(Guid callerId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}/confirmCall", Method.POST);
            request.AddUrlSegment("friendId", callerId);
            request.AddHeader("Authorization", "Bearer " + _token);

            await _restClient.ExecuteTaskAsync(request);
        }

        public async Task SaveCallInfoAsync(Guid friendId, TimeSpan duration)
        {
            var call = new Call { Duration = duration.Ticks };
        }

        public async Task<bool> SendFriendRequestAsync(Guid targetId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}", Method.POST);
            request.AddUrlSegment("friendId", targetId);
            request.AddHeader("Authorization", "Bearer " + _token);

            var response = await _restClient.ExecuteTaskAsync<bool>(request);

            return response.Data;
        }

        public async Task<Photo> SetPhotoAsync(User user, string path)
        {
            var request = new RestRequest("/api/photos/{avatarId}/photo", Method.POST);
            request.AddUrlSegment("avatarId", user.Id.ToString());
            request.AddHeader("Authorization", "Bearer " + _token);
            request.AddFile("file", path, "image / " + Path.GetExtension(path));

            var response = await _restClient.ExecuteTaskAsync<Photo>(request);

            var photo = response.Data;
            return photo;
        }
    }
}
