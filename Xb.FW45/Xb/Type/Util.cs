using System;
using System.Collections;
using System.Collections.Generic;

namespace Xb.Type
{
    /// <summary>
    /// 型関連関数群
    /// </summary>
    /// <remarks></remarks>
    public class Util
    {
        /// <summary>
        /// 型のキャストが可能かどうかを検証する。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// コマンドライン引数のキャストを目的としているため、ごく基本的な型を対象とする。
        /// </remarks>
        public static bool IsConvertable(object value, System.Type type)
        {
            try
            {
                if (object.ReferenceEquals(type, typeof(bool)))
                {
                    if (value.ToString().ToLower() == "true")
                    {
                        value = true;
                    }
                    else if (value.ToString().ToLower() == "false")
                    {
                        value = false;
                    }
                    else
                    {
                        value = Convert.ToBoolean(value);
                    }
                }
                else if (object.ReferenceEquals(type, typeof(Int16)))
                {
                    value = Convert.ToInt16(value);
                }
                else if (object.ReferenceEquals(type, typeof(Int32)))
                {
                    value = Convert.ToInt32(value);
                }
                else if (object.ReferenceEquals(type, typeof(Int64)))
                {
                    value = Convert.ToInt64(value);
                }
                else if (object.ReferenceEquals(type, typeof(UInt16)))
                {
                    value = Convert.ToUInt16(value);
                }
                else if (object.ReferenceEquals(type, typeof(UInt32)))
                {
                    value = Convert.ToUInt32(value);
                }
                else if (object.ReferenceEquals(type, typeof(UInt64)))
                {
                    value = Convert.ToUInt64(value);
                }
                else if (object.ReferenceEquals(type, typeof(Decimal)))
                {
                    value = Convert.ToDecimal(value);
                }
                else if (object.ReferenceEquals(type, typeof(Single)))
                {
                    value = Convert.ToSingle(value);
                }
                else if (object.ReferenceEquals(type, typeof(Double)))
                {
                    value = Convert.ToDouble(value);
                }
                else if (object.ReferenceEquals(type, typeof(DateTime)))
                {
                    value = (DateTime)value;
                    //注) .Net仕様上、Date型とDateTime型は同じもの。
                    //https://msdn.microsoft.com/ja-jp/library/47zceaw7.aspx
                }
                else if (object.ReferenceEquals(type, typeof(Str)))
                {
                    value = Convert.ToString(value);
                }
                else if (object.ReferenceEquals(type, typeof(char)))
                {
                    value = Convert.ToChar(value);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 値がNull(Nothing) or DbNull.Value か否か
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsNull(object value)
        {
            return (value == null || value == DBNull.Value);
        }


        /// <summary>
        /// 文字列変数のNull変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string NullToString(object value)
        {
            try
            {
                if (value == null
                    || value == DBNull.Value)
                {
                    value = "";
                }

                return value.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }


        /// <summary>
        /// 数値変数のNull変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static decimal NullToDecimal(object value)
        {
            try
            {
                if (value == null
                    || value == DBNull.Value)
                {
                    value = 0;
                }
                
                return decimal.Parse(value.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 整数変数のNull変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int NullToInteger(object value)
        {
            try
            {
                if (value == null 
                    || value == DBNull.Value)
                {
                    value = 0;
                }

                return int.Parse(value.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 日付変数のNull変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime NullToDate(object value)
        {
            try
            {
                if (value == null
                    || value == DBNull.Value)
                {
                    return DateTime.MinValue;
                }
                
                return Convert.ToDateTime(value);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }


        /// <summary>
        /// 日付変数のNull変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TimeSpan NullToTime(object value)
        {
            try
            {
                if (DBNull.Value == value)
                {
                    return TimeSpan.MinValue;
                }
                else if (value == null)
                {
                    return TimeSpan.MinValue;
                }
                return (TimeSpan)value;
            }
            catch (Exception)
            {
                return TimeSpan.MinValue;
            }
        }


        /// <summary>
        /// 渡し値の日付型変数が、入力済みかNullかを判定する。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsDateNull(System.DateTime value)
        {
            //if (value == null)
            //    return true;
            //if (DBNull.Value == value)
            //    return true;

            if (value < DateTime.Parse("1868/09/08"))
                return true;

            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(string[] array, string value)
        {
            if (array == null 
                || System.Array.IndexOf(array, value) == -1)
                return false;

            return true;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(short[] array, string value)
        {
            if (array == null)
                return false;

            foreach (short val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(int[] array, string value)
        {
            if (array == null)
                return false;

            foreach (int val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(long[] array, string value)
        {
            if (array == null)
                return false;

            foreach (long val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(decimal[] array, string value)
        {
            if (array == null)
                return false;

            foreach (decimal val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(List<string> array, string value)
        {
            if (array == null || !array.Contains(value))
                return false;
            return true;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(List<short> array, string value)
        {
            if (array == null)
                return false;

            foreach (short val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(List<int> array, string value)
        {
            if (array == null)
                return false;

            foreach (int val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(List<long> array, string value)
        {
            if (array == null)
                return false;

            foreach (long val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(List<decimal> array, string value)
        {
            if (array == null)
                return false;

            foreach (decimal val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(Hashtable array, string value)
        {
            if (array == null)
                return false;

            foreach (object val in array.Values)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列の中に指定値が存在しているか否かを判定する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArray(ArrayList array, string value)
        {
            if (array == null)
                return false;

            foreach (object val in array)
            {
                if (val.ToString() == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(string[] array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (string val in array)
            {
                if(val == null)
                    continue;

                if (val.IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(short[] array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (short val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(int[] array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (int val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(long[] array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (long val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(decimal[] array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (decimal val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(List<string> array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (string val in array)
            {
                if (val == null)
                    continue;

                if (val.IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(List<short> array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (short val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(List<int> array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (int val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(List<long> array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (long val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(List<decimal> array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (decimal val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(Hashtable array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (object val in array.Values)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 配列中の各値を文字列化し、渡し値に部分合致するものが存在するか否かを検証する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InArrayMatch(ArrayList array, string value)
        {
            if (array == null || value == null)
                return false;

            foreach (object val in array)
            {
                if (val.ToString().IndexOf(value) != -1)
                    return true;
            }
            return false;
        }
    }
}
