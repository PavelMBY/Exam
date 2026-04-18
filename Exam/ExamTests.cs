using Exam.DTO;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Exam
{
    public class Tests
    {
        private RestClient client;
        private static string movieId; 

        [OneTimeSetUp] 
        public void Setup()
        {
            string jwtToken = GetJwtToken("paveltest123@example.com", "paveltest123");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }


        [Order(1)]
        [Test]
        public void CreateMovie_With_RequieredData_ShouldBeSuccess()
        {
            MovieDTO movie = new MovieDTO
            {
                Title = "MovieTest",
                Description =  "Test"
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));
            movieId = readyResponse.Movie.Id;
        }


        [Order(2)]
        [Test]
        public void EditMovie_with_Valid_Id_ShouldBeSuccess() 
        {
            MovieDTO editedMovie = new MovieDTO
            {
                Title = "Edited Title",
                Description = "edited descr"
            };

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", movieId);
            request.AddJsonBody(editedMovie);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyresponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyresponse.Msg, Is.EqualTo("Movie edited successfully!"));


        }

        [Order(3)]
        [Test]
        public void Get_all_Movies_returns_a_NONempty_array()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            List<MovieDTO> readyresponse = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(readyresponse, Is.Not.Null);
            Assert.That(readyresponse, Is.Not.Empty);
            Assert.That(readyresponse.Count, Is.GreaterThanOrEqualTo(1));


        }

        [Order(4)]
        [Test]
        public void Delete_movie_with_valid_ID()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", movieId);
            
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyresponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyresponse.Msg, Is.EqualTo("Movie deleted successfully!"));

        }

        [Order(5)]
        [Test]
        public void Create_Movie_with_incorectData_Should_Bad_Request()
        {
            MovieDTO movie = new MovieDTO
            {
                Title = "",
                Description = ""
            };


            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Order(6)]
        [Test]
        public void Edit_a_movie_with_NONExistingId_Shoudl_return_BadRequest()
        {
            string nonexId = "nonexID";

            MovieDTO movie = new MovieDTO
            {
                Title = "TestTitle",
                Description = "TestDescription"
            };

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonexId);
            request.AddJsonBody(movie);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyresponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyresponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));


        }

        [Order(7)]
        [Test]
        public void Delete_a_movie_That_does_not_exist_returns_badrequest()
        {
            string nonexId = "nonexID";

            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonexId);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyresponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyresponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));


        }


        [OneTimeTearDown] 
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}