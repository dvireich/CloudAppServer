using System.Collections.Generic;

namespace ContentManager.Model
{
    public class ContentPage : List<ContentBase>
    {
        #region Members

        public int PageNumber { get; }

        #endregion

        #region Ctor

        public ContentPage(int pageNumber)
        {
            PageNumber = pageNumber;
        }

        #endregion

    }
}
