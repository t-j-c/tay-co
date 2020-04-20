using Microsoft.AspNetCore.Components;

namespace Tayco.Web.Components
{
    public class PageHeaderBase : ComponentBase
    {
        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Subtitle { get; set; }
    }
}
