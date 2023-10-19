/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
namespace CQRS.Common.Constants
{
    public static class Contents
    {
        /// <summary>
        /// Represents HTML content, usually rendered by web browsers.
        /// </summary>
        public const string TextHTML = "text/html";

        /// <summary>
        /// Represents XML content similar to application/xml.
        /// </summary>
        public const string TextXML = "text/xml";

        /// <summary>
        /// Represents plain text content without any formatting.
        /// </summary>
        public const string TextPLAIN = "text/plain";

        /// <summary>
        /// Represents Cascading Style Sheets (CSS) content for styling web pages.
        /// </summary>
        public const string TextCSS = "text/css";

        /// <summary>
        /// Represents JSON data, often used for data exchange between client and server.
        /// </summary>
        public const string JSON = "application/json";

        /// <summary>
        /// Represents problem in a json content.
        /// </summary>
        public const string JSONError = "application/problem+jason";

        /// <summary>
        /// Represents XML data, commonly used for structured data exchange.
        /// </summary>
        public const string XML = "application/xml";

        /// <summary>
        /// Represents problem in a xml content.
        /// </summary>
        public const string XMLError = "application/problem+xml";

        /// <summary>
        /// Represents RSS (Really Simple Syndication) feeds for distributing web content.
        /// </summary>
        public const string RssXML = "application/rss+xml";

        /// <summary>
        /// Represents JavaScript code, typically used for client-side scripting in web pages.
        /// </summary>
        public const string JavaScript = "application/javascript";

        /// <summary>
        /// Represents PDF (Portable Document Format) files, used for documents.
        /// </summary>
        public const string PDF = "application/pdf";

        /// <summary>
        /// Represents binary data, commonly used for file downloads or generic binary data exchange.
        /// </summary>
        public const string Stream = "application/octet-stream";

        /// <summary>
        /// Represents form data submitted via HTTP POST requests with file uploads.
        /// </summary>
        public const string FormDataMULTIPART = "multipart/form-data";

        /// <summary>
        /// Represents URL-encoded form data, often used in standard HTML forms.
        /// </summary>
        public const string FormDataURLENCODED = "application/x-www-form-urlencoded";

        /// <summary>
        /// Represents GIF image files, known for supporting animation.
        /// </summary>
        public const string ImageGIF = "image/gif";

        /// <summary>
        /// Represents JPEG image files, commonly used for photographs.
        /// </summary>
        public const string ImageJPG = "image/jpeg";

        /// <summary>
        /// Represents PNG image files, often used for graphics with transparency support.
        /// </summary>
        public const string ImagePNG = "image/png";
    }
}
