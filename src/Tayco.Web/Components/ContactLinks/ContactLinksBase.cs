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
                SolidLink("Gmail", "mailto:tayco2702@gmail.com", "envelope"),
                BrandLink("LinkedIn", "https://www.linkedin.com/in/tayco2702/", "linkedin"),
                BrandLink("Stack Overflow", "https://stackoverflow.com/users/5803406/devnull?tab=profile", "stack-overflow"),
                BrandLink("Github", "https://github.com/t-j-c", "github"),
                BrandLink("Twitter", "https://twitter.com/devNull_4", "twitter")
            };

            private static Link SolidLink(string name, string url, string icon) => new Link(name, url, icon, "fas");
            private static Link BrandLink(string name, string url, string icon) => new Link(name, url, icon, "fab");

            private Link(string name, string url, string icon, string stylePrefix)
            {
                Name = name;
                Url = url;
                Icon = icon;
                StylePrefix = stylePrefix;
            }

            public string Name { get; }
            public string Url { get; }
            public string Icon { get; }
            public string StylePrefix { get; }
        }
    }
}
