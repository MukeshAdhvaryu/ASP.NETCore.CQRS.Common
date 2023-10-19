/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using CQRS.Common.Models;

namespace CQRS.Common.Models
{
    #region MESSAGE
    public readonly struct Message
 
    {
        readonly string? LongMessage;
        public readonly string Name;
        //[JsonConverter(typeof(StringEnumConverter))]
        public readonly ResultStatus Status;

        public Message(ResultStatus notification, string name, string? message = null)
        {
            LongMessage = message;
            Status = notification;
            Name = name;
        }

        #region DEFAULT MESSAGES
        public static Message Sucess(string name)=>
            new Message(ResultStatus.Sucess, name);
        public static Message Failure(string name) =>
            new Message(ResultStatus.Failure, name);
        public static Message MissingValue(string name) =>
            new Message(ResultStatus.MissingValue, name);
        public static Message MissingRequiredValue(string name) =>
            new Message(ResultStatus.MissingRequiredValue, name);
        public static Message Ignored(string name) =>
            new Message(ResultStatus.Ignored, name);
        #endregion

        public override string ToString()
        {
            var result = Status + ": value of " + Name;
            if (!string.IsNullOrEmpty(LongMessage))
                return result + "; " + LongMessage;
            return result;
        }

        public static implicit operator string (Message msg) =>
            msg.ToString();
    }
    #endregion

}
