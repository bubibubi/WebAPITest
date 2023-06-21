using System;

namespace WebAPITest.Model
{
    public class RequestCollection : System.Collections.ObjectModel.KeyedCollection<string, Request>
    {
        public RequestCollection() : base(StringComparer.CurrentCultureIgnoreCase) { }

        protected override string GetKeyForItem(Request item)
        {
            return item.Name;
        }
    }
}