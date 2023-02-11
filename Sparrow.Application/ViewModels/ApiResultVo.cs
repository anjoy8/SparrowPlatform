namespace SparrowPlatform.Application.ViewModels
{
    /// <summary>
    /// Output model of API results.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResultVo<T>
    {

        public int status { get; set; } = 200;

        public bool success { get; set; } = false;

        public string msg { get; set; } = "";

        public T response { get; set; }


        public static ApiResultVo<T> ok(T response)
        {
            return Message(true, "success", response);
        }

        public static ApiResultVo<T> ok(T response, string msg)
        {
            return Message(true, msg, response);
        }

        public static ApiResultVo<T> error(string msg)
        {
            return Message(false, msg, default);
        }

        public static ApiResultVo<T> error(T response, string msg)
        {
            return Message(false, msg, response);
        }

        public static ApiResultVo<T> Message(bool success, string msg, T response)
        {
            return new ApiResultVo<T>() { msg = msg, response = response, success = success };
        }
    }

}
