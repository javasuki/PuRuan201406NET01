using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;

namespace Win.AdoNet
{
    public static class ControlBindExt
    {
        public static void Bind<T,V>(this T t, Expression<Func<T, V>> exp, object dataSource, string colName) where T : System.Windows.Forms.Control
        {
            MemberExpression e = null;
            if (exp.Body.NodeType == ExpressionType.MemberAccess)
                e = exp.Body as MemberExpression;
            else
                throw new ArgumentException("must is MemberAccess.", "exp");

            string propName = e.Member.Name;
            if (t.DataBindings[propName] != null) return;
            t.DataBindings.Add(e.Member.Name, dataSource, colName, true);
            
        }
    }
}
