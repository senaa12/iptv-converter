using IptvConverter.Business.Models.Enum;
using Newtonsoft.Json;

namespace IptvConverter.Business.Models
{
    public class AjaxResponse<T> : AjaxResponse
    {
        public T Data { get; set; }

        protected internal AjaxResponse(T value, AjaxResponseTypeEnum type, string errorMsg) : base(type, errorMsg)
        {
            Data = value;
        }

        public static AjaxResponse<T> Success(T value)
        {
            return new AjaxResponse<T>(value, AjaxResponseTypeEnum.Success, null);
        }

        public static new AjaxResponse<T> Success()
        {
            return new AjaxResponse<T>(default, AjaxResponseTypeEnum.Success, null);
        }

        public static AjaxResponse<T> Warning(T data)
        {
            return new AjaxResponse<T>(data, AjaxResponseTypeEnum.Warning, null);
        }

        public static new AjaxResponse<T> Error(string message)
        {
            return new AjaxResponse<T>(default(T), AjaxResponseTypeEnum.Error, message);
        }

        public static AjaxResponse<T> Error(string message, T data)
        {
            return new AjaxResponse<T>(data, AjaxResponseTypeEnum.Error, message);
        }
    }

    public class AjaxResponse
    {
        public AjaxResponseTypeEnum ResultType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        protected AjaxResponse() { }

        protected AjaxResponse(AjaxResponseTypeEnum result, string errorMsg)
        {
            ResultType = result;
            ErrorMessage = errorMsg;
        }

        public static AjaxResponse Error(string errorMsg)
        {
            return new AjaxResponse
            {
                ResultType = AjaxResponseTypeEnum.Error,
                ErrorMessage = errorMsg
            };
        }

        public static AjaxResponse Warning(string message)
        {
            return new AjaxResponse
            {
                ResultType = AjaxResponseTypeEnum.Warning,
                ErrorMessage = message
            };
        }

        public static AjaxResponse Success()
        {
            return new AjaxResponse
            {
                ResultType = AjaxResponseTypeEnum.Success
            };
        }
    }

}
