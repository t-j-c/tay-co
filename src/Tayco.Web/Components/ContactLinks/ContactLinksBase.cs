using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Tayco.Web.Components
{
    public class ContactLinksBase : ComponentBase
    {
        public class Link
        {
            public static readonly IEnumerable<Link> All = new List<Link>
            {
                new Link("Gmail", "mailto:tayco2702@gmail.com", "envelope"),
                new Link("LinkedIn", "https://www.linkedin.com/in/tayco2702/", "linkedin"),
                new Link("Stack Overflow", "https://stackoverflow.com/users/5803406/devnull?tab=profile", "stack-overflow"),
                new Link("Github", "https://github.com/t-j-c", "github"),
                new Link("Twitter", "https://twitter.com/devNull_4", "twitter")
            };

            private Link(string name, string url, string icon)
            {
                Name = name;
                Url = url;
                Icon = icon;
            }

            public string Name { get; }
            public string Url { get; }
            public string Icon { get; }
        }
    }
}
