using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestSharp;

namespace MySkype.WpfClient.Services
{
    public class RestSharpClient
    {
        private readonly RestClient _restClient = new RestClient("http://localhost:5000/");

        public RestSharpClient(string token)
        {
            _restClient.AddDefaultHeader("Authorization", "Bearer "+ token);
        }
        
        public async Task<List<User>> GetUsersAsync(string searchQuery)
        {
            var request = new RestRequest("/api/users", Method.GET);
            request.AddQueryParameter("searchQuery", searchQuery);

            var response = await _restClient.ExecuteGetTaskAsync<List<User>>(request);

            return response.Data;
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            var request = new RestRequest("/api/users/{id}", Method.GET);
            request.AddUrlSegment("id", id);

            var response = await _restClient.ExecuteGetTaskAsync<User>(request);

            return response.Data;
        }

        public async Task<byte[]> GetPhotoAsync(Guid avatarId)
        {
            var request = new RestRequest("/api/photos/{id}", Method.GET);
            request.AddUrlSegment("id", avatarId.ToString());

            var file = _restClient.DownloadData(request);

            return file;

        }

        public async Task<List<User>> GetFriendsAsync()
        {
            var request = new RestRequest("/api/user/friends", Method.GET);
            var response = await _restClient.ExecuteGetTaskAsync<List<User>>(request);

            return response.Data;
        }

        public async Task<bool> AddFriendAsync(Guid friendId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}", Method.PUT);
            request.AddUrlSegment("friendId", friendId);

            var response = await _restClient.ExecuteTaskAsync<bool>(request);

            return response.Data;
        }

        public async Task<string> RequestTokenAsync(TokenRequest tokenRequest)
        {
            var request = new RestRequest("/api/identity", Method.POST);
            request.AddJsonBody(tokenRequest);

            var response = await _restClient.ExecutePostTaskAsync<string>(request);

            return response.StatusCode == HttpStatusCode.OK ? response.Data : null;
        }



        public async Task<HttpStatusCode> SignUpAsync(SignUpRequest signUpRequest)
        {
            var request = new RestRequest("/api/users/", Method.POST);
            request.AddJsonBody(signUpRequest);

            var result = await _restClient.ExecuteTaskAsync(request);

            return result.StatusCode;
        }

        public async Task SendDataAsync(Guid friendId, byte[] data)
        {
            var request = new RestRequest("/api/user/friends/{friendId}/data", Method.POST);
            request.AddUrlSegment("friendId", friendId);
            request.AddJsonBody(data);

            await _restClient.ExecuteTaskAsync(request);
        }

        public async Task<List<Call>> GetUserCallsAsync()
        {
            var request = new RestRequest("/api/calls/", Method.GET);

            var result = await _restClient.ExecuteTaskAsync<List<Call>>(request);

            return result.Data;
        }

        public async Task SaveCallInfoAsync(Call call)
        {
            var request = new RestRequest("/api/calls/", Method.POST);
            request.AddJsonBody(call);

            await _restClient.ExecuteTaskAsync(request);
        }

        public async Task<bool> SendFriendRequestAsync(Guid targetId)
        {
            var request = new RestRequest("/api/user/friends/{friendId}", Method.POST);
            request.AddUrlSegment("friendId", targetId);

            var response = await _restClient.ExecuteTaskAsync<bool>(request);

            return response.Data;
        }

        public async Task<Photo> SetPhotoAsync(User user, string path)
        {
            var request = new RestRequest("/api/photos/{userId}", Method.POST);
            request.AddUrlSegment("userId", user.Id.ToString());
            request.AddFile("file", path, "image / " + Path.GetExtension(path));

            var response = await _restClient.ExecuteTaskAsync<Photo>(request);

            var photo = response.Data;
            return photo;
        }
    }
}
