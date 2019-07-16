namespace GemstarPaymentCore.Business.Utility
{
    /// <summary>
    /// 字符串数组扩展类
    /// </summary>
    public static class StringArrayExtension
    {
        /// <summary>
        /// 获取指定位置上的值
        /// 如果指定位置超出索引或者值为空，则使用默认值
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="index">索引</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>值</returns>
         public static string GetNotEmptyValue(this string[] array,int index,string defaultValue)
        {
            if (index < array.Length)
            {
                var temp = array[index];
                if (!string.IsNullOrEmpty(temp))
                {
                    return temp;
                }
            }
            return defaultValue;
        }
    }
}
