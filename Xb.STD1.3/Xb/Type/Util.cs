using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xb.Type
{
    /// <summary>
    /// 型関連関数群
    /// </summary>
    /// <remarks></remarks>
    public class Util
    {
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

                decimal result;
                return decimal.TryParse(value.ToString(), out result)
                        ? result
                        : 0;
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

                int result;
                return int.TryParse(value.ToString(), out result)
                        ? result
                        : 0;
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

                DateTime result;
                return DateTime.TryParse(value.ToString(), out result)
                        ? result
                        : DateTime.MinValue;
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
                if (value == null 
                    || DBNull.Value == value)
                {
                    return TimeSpan.MinValue;
                }

                TimeSpan result;
                return TimeSpan.TryParse(value.ToString(), out result)
                        ? result
                        : TimeSpan.MinValue;
            }
            catch (Exception)
            {
                return TimeSpan.MinValue;
            }
        }


        /// <summary>
        /// インスタンスが保持するプロパティ／フィールド値をコンソールに書き出す。
        /// </summary>
        /// <param name="obj"></param>
        public static void DumpObjectStatus(object obj)
        {
            var msg = new StringBuilder();
            msg.AppendFormat("[Type: {0} ]\r\n", obj.GetType());
            msg.AppendFormat("{0}\r\n", string.Join("\r\n", Xb.Type.Util.GetPropertyString(obj)));
            msg.AppendFormat("{0}\r\n", string.Join("\r\n", Xb.Type.Util.GetFieldString(obj)));
            Xb.Util.OutHighlighted(msg.ToString());
        }

        /// <summary>
        /// インスタンスのメソッドをコンソールに書き出す。
        /// </summary>
        /// <param name="obj"></param>
        public static void DumpObjectMethod(object obj)
        {
            var msg = new StringBuilder();
            msg.AppendFormat("[Type: {0} ]\r\n", obj.GetType());
            msg.AppendFormat("{0}\r\n", string.Join("\r\n", Xb.Type.Util.GetMethodString(obj)));
            Xb.Util.OutHighlighted(msg.ToString());
        }

        /// <summary>
        /// インスタンスのイベントをコンソールに書き出す。
        /// </summary>
        /// <param name="obj"></param>
        public static void DumpObjectEvent(object obj)
        {
            var msg = new StringBuilder();
            msg.AppendFormat("[Type: {0} ]\r\n", obj.GetType());
            msg.AppendFormat("{0}\r\n", string.Join("\r\n", Xb.Type.Util.GetEventString(obj)));
            Xb.Util.OutHighlighted(msg.ToString());
        }

        ///// <summary>
        ///// インスタンスの型をコンソールに書き出す。
        ///// </summary>
        ///// <param name="obj"></param>
        //public static void DumpObjectTypes(object obj, bool withinInterfaces = false)
        //{
        //    var msg = new StringBuilder();
        //    msg.AppendFormat("[Type: {0} ]\r\n", obj.GetType());
        //    msg.AppendFormat("{0}\r\n", string.Join("\r\n", Xb.Type.Util.GetTypes(obj, withinInterfaces)));
        //    Xb.Util.OutHighlighted(msg.ToString());
        //}

        ///// <summary>
        ///// 渡し値の型名／インタフェース名を、継承関係を遡って取得する。
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="withinInterfaces"></param>
        ///// <returns></returns>
        //public static string[] GetTypes(object obj, bool withinInterfaces = false)
        //{
        //    var result = new List<string>();
        //    result.AddRange(Xb.Type.Util.GetTypes(obj.GetType(), withinInterfaces));
        //    result.Sort();
        //    return result.ToArray();
        //}

        ///// <summary>
        ///// 渡し値の型名／インタフェース名を、継承関係を遡って取得する。
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="withinInterfaces"></param>
        ///// <returns></returns>
        //public static string[] GetTypes(System.Type type, bool withinInterfaces = false)
        //{
        //    var result = new List<string>();
        //    result.Add(type.FullName);

        //    type.

        //    if (type.BaseType != null)
        //    {
        //        var bases = Xb.Type.Util.GetTypes(type.BaseType);
        //        result.AddRange(bases.AsEnumerable().Where(val => !result.Contains(val)));

        //        if (withinInterfaces)
        //        {
        //            var baseInterfaces = Xb.Type.Util.GetInterfaces(type.BaseType);
        //            result.AddRange(baseInterfaces.AsEnumerable().Where(val => !result.Contains(val)));
        //        }
        //    }

        //    if (withinInterfaces)
        //    {
        //        foreach (var intfc in type.GetInterfaces())
        //        {
        //            result.Add(intfc.FullName);
        //            var bases = Xb.Type.Util.GetInterfaces(intfc);
        //            result.AddRange(bases.AsEnumerable().Where(val => !result.Contains(val)));
        //        }
        //    }
        //    result.Sort();
        //    return result.ToArray();
        //}

        ///// <summary>
        ///// 渡し値のインタフェース名を、継承関係を遡って取得する。
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static string[] GetInterfaces(System.Type type)
        //{
        //    var result = new List<string>();
        //    result.AddRange(type.GetInterfaces().Select(val => val.FullName).ToArray());

        //    foreach (System.Type @interface in type.GetInterfaces())
        //    {
        //        result.Add(@interface.FullName);

        //        if (@interface.BaseType != null)
        //        {
        //            var bases = Xb.Type.Util.GetInterfaces(@interface);
        //            result.AddRange(bases.Where(val => result.Contains(val)));
        //        }
        //    }
        //    result.Sort();
        //    return result.ToArray();
        //}

        /// <summary>
        /// プロパティ名／値のリストを改行付き文字列で取得する。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GetPropertyString(object obj)
        {
            var result = new List<string>();
            var type = obj.GetType();

            //プロパティの一覧を取得する
            PropertyInfo[] properties = type.GetRuntimeProperties().ToArray();


            var row = new StringBuilder();
            foreach (PropertyInfo p in properties)
            {
                //特別な名前のメソッドは表示しない
                if (p.IsSpecialName)
                    continue;

                row.Clear();

                //アクセシビリティを表示
                row.Append("public ");

                //p.MemberType
                //戻り値を表示
                if (p.PropertyType == typeof(void))
                    row.Append("void ");
                else
                    row.Append(p.PropertyType.ToString());
                row.Append(" ");

                //メソッド名を表示
                row.Append("property ");
                row.Append(p.Name);

                try
                {
                    row.Append($" = {p.GetValue(obj, null)} ");
                }
                catch (Exception)
                {
                    row.Append(" = [Fail] ");
                }

                result.Add(row.ToString());
            }
            return result.ToArray();
        }

        /// <summary>
        /// フィールド名／値のリストを改行付き文字列で取得する。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="goDeep"></param>
        /// <returns></returns>
        public static string[] GetFieldString(object obj)
        {
            var result = new List<string>();
            var type = obj.GetType();

            //フィールドの一覧を取得する
            FieldInfo[] fields = type.GetRuntimeFields().ToArray();

            var row = new StringBuilder();
            foreach (FieldInfo m in fields)
            {
                //特別な名前のメソッドは表示しない
                if (m.IsSpecialName)
                    continue;

                row.Clear();

                //アクセシビリティを表示
                if (m.IsPublic)
                    row.Append("public ");
                if (m.IsPrivate)
                    row.Append("private ");
                if (m.IsAssembly)
                    row.Append("internal ");
                if (m.IsFamily)
                    row.Append("protected ");
                if (m.IsFamilyOrAssembly)
                    row.Append("internal protected ");

                //その他修飾子を表示
                if (m.IsStatic)
                    row.Append("static ");

                //戻り値を表示
                if (m.FieldType == typeof(void))
                    row.Append("void ");
                else
                    row.Append(m.FieldType.ToString() + " ");

                //メソッド名を表示
                row.Append("field ");
                row.Append(m.Name);

                try
                {
                    row.AppendFormat(" = {0}", m.GetValue(obj));
                }
                catch (Exception)
                {
                    row.Append(" = [Fail] ");
                }

                result.Add(row.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// メソッドのリストを改行付き文字列で取得する。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GetMethodString(object obj)
        {
            var result = new List<string>();
            var type = obj.GetType();

            //メソッドの一覧を取得する
            MethodInfo[] methods = type.GetRuntimeMethods().ToArray();

            var row = new StringBuilder();
            foreach (MethodInfo m in methods)
            {
                //特別な名前のメソッドは表示しない
                if (m.IsSpecialName)
                    continue;

                row.Clear();

                //アクセシビリティを表示
                if (m.IsPublic)
                    row.Append("public ");
                if (m.IsPrivate)
                    row.Append("private ");
                if (m.IsAssembly)
                    row.Append("internal ");
                if (m.IsFamily)
                    row.Append("protected ");
                if (m.IsFamilyOrAssembly)
                    row.Append("internal protected ");

                //その他修飾子を表示
                if (m.IsStatic)
                    row.Append("static ");
                if (m.IsAbstract)
                    row.Append("abstract ");
                else if (m.IsVirtual)
                    row.Append("virtual ");

                //戻り値を表示
                if (m.ReturnType == typeof(void))
                    row.Append("void ");
                else
                    row.Append(m.ReturnType.ToString() + " ");

                //メソッド名を表示
                row.Append(m.Name);

                //パラメータを表示
                ParameterInfo[] prms = m.GetParameters();
                row.Append("(");
                for (int i = 0; i < prms.Length; i++)
                {
                    ParameterInfo p = prms[i];
                    row.Append(p.ParameterType.ToString() + " " + p.Name);
                    if (prms.Length - 1 > i)
                        row.Append(", ");
                }
                row.Append(")");

                result.Add(row.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// イベントのリストを改行付き文字列で取得する。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GetEventString(object obj)
        {
            var result = new List<string>();
            var type = obj.GetType();

            //イベントの一覧を取得する
            EventInfo[] events = type.GetRuntimeEvents().ToArray();

            var row = new StringBuilder();
            foreach (EventInfo m in events)
            {
                //特別な名前のメソッドは表示しない
                if (m.IsSpecialName)
                    continue;

                row.Clear();

                //アクセシビリティを表示
                row.Append("public ");

                //メソッド名を表示
                row.Append("event ");
                row.Append(m.Name);

                result.Add(row.ToString());
            }

            return result.ToArray();
        }

    }
}
