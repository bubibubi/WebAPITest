using System;

namespace WebAPITest.Model
{
    public class Request
    {
        public string Name { get; set; }
        public string Verb { get; set; }
        public string Url { get; set; }
        public string Payload { get; set; }
        public string Answer { get; set; }
        public bool SendAuth { get; set; }


        public override string ToString()
        {
            return Name;
        }
    }
}
