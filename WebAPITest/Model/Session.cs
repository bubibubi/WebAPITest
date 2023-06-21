using System;

namespace WebAPITest.Model
{
    public class Session
    {
        public Session()
        {
            Requests = new RequestCollection();
        }

        public string LoginUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public RequestCollection Requests { get; set; }
    }
}