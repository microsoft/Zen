// <copyright file="Unroller.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Research.Zen.AstTranformations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Perform basic simplifications over a Zen expression. This includes:
    /// (1) Unrolling any match expressions fully
    /// (2) Removing combinations of createObject, getField, withField
    /// (3) Combining and removing consecutive adapter calls
    /// (4) Performing constant propagations.
    /// </summary>
    internal sealed class Simplifier : IZenExprTransformer
    {
        /// <summary>
        /// Cache of results based on unique object hashcode.
        /// </summary>
        private Dictionary<object, object> cache = new Dictionary<object, object>();

        private Zen<T> LookupOrCompute<T>(object expression, Func<Zen<T>> callback)
        {
            if (cache.TryGetValue(expression, out var value))
            {
                return (Zen<T>)value;
            }

            var result = callback();
            cache[expression] = result;
            return result;
        }

        public Zen<T1> VisitZenAdapterExpr<T1, T2>(ZenAdapterExpr<T1, T2> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var e = expression.Expr.Accept(this);

                // adapt(t1, t2, adapt(t2, t1, e)) == e
                if (e is ZenAdapterExpr<T2, T1> inner1)
                {
                    return inner1.Expr;
                }

                // adapt(t1, t2, adapt(t2, t3, e)) == adapt(t1, t3, e)
                /* var type = e.GetType();
                if (type.GetGenericTypeDefinition() == typeof(ZenAdapterExpr<,>))
                {
                    if (type.GetGenericArguments()[0] == typeof(T2))
                    {
                        var type1 = typeof(T1);
                        var type3 = type.GetGenericArguments()[1];
                        var astType = typeof(ZenAdapterExpr<,>).MakeGenericType(type1, type3);
                        var paramType1 = typeof(Zen<>).MakeGenericType(type3);
                        var paramType2 = typeof(ImmutableList<>).MakeGenericType(typeof(Func<object, object>));
                        var method = astType.GetMethod("CreateMulti");
                        var param1 = type.GetProperty("Expr").GetValue(e);
                        var converters = (ImmutableList<Func<object, object>>)type.GetProperty("Converters").GetValue(e);
                        var param2 = converters.AddRange(expression.Converters);
                        return (Zen<T1>)method.Invoke(null, new object[] { param1, param2 });
                    }
                } */

                // adapt(t1,t2,if e1 then e2 else e3) == if e1 then adapt(t1,t2,e2) else adapt(t1,t2,e3)
                if (e is ZenIfExpr<T2> inner2)
                {
                    var trueBranch = ZenAdapterExpr<T1, T2>.CreateMulti(inner2.TrueExpr, expression.Converters);
                    var falseBranch = ZenAdapterExpr<T1, T2>.CreateMulti(inner2.FalseExpr, expression.Converters);
                    return ZenIfExpr<T1>.Create(inner2.GuardExpr, trueBranch, falseBranch);
                }

                return ZenAdapterExpr<T1, T2>.CreateMulti(e, expression.Converters);
            });
        }

        public Zen<bool> VisitZenAndExpr(ZenAndExpr expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var b1 = expression.Expr1.Accept(this);
                var b2 = expression.Expr2.Accept(this);

                if (b1 is ZenConstantBoolExpr x)
                {
                    return (x.Value ? b2 : b1);
                }

                if (b2 is ZenConstantBoolExpr y)
                {
                    return (y.Value ? b1 : b2);
                }

                return ZenAndExpr.Create(b1, b2);
            });
        }

        public Zen<T> VisitZenArbitraryExpr<T>(ZenArbitraryExpr<T> expression)
        {
            return expression;
        }

        public Zen<T> VisitZenArgumentExpr<T>(ZenArgumentExpr<T> expression)
        {
            return expression;
        }

        public Zen<T> VisitZenBitwiseAndExpr<T>(ZenBitwiseAndExpr<T> expression)
        {
            return LookupOrCompute<T>(expression, () =>
            {
                var v1 = expression.Expr1.Accept(this);
                var v2 = expression.Expr2.Accept(this);

                if (v1 is ZenConstantByteExpr xb && v2 is ZenConstantByteExpr yb)
                {
                    return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(xb.Value & yb.Value));
                }

                if (v1 is ZenConstantShortExpr xs && v2 is ZenConstantShortExpr ys)
                {
                    return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(xs.Value & ys.Value));
                }

                if (v1 is ZenConstantUshortExpr xus && v2 is ZenConstantUshortExpr yus)
                {
                    return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(xus.Value & yus.Value));
                }

                if (v1 is ZenConstantIntExpr xi && v2 is ZenConstantIntExpr yi)
                {
                    return (Zen<T>)(object)ZenConstantIntExpr.Create(xi.Value & yi.Value);
                }

                if (v1 is ZenConstantUintExpr xui && v2 is ZenConstantUintExpr yui)
                {
                    return (Zen<T>)(object)ZenConstantUintExpr.Create(xui.Value & yui.Value);
                }

                if (v1 is ZenConstantLongExpr xl && v2 is ZenConstantLongExpr yl)
                {
                    return (Zen<T>)(object)ZenConstantLongExpr.Create(xl.Value & yl.Value);
                }

                if (v1 is ZenConstantUlongExpr xul && v2 is ZenConstantUlongExpr yul)
                {
                    return (Zen<T>)(object)ZenConstantUlongExpr.Create(xul.Value & yul.Value);
                }

                return ZenBitwiseAndExpr<T>.Create(v1, v2);
            });
        }

        public Zen<T> VisitZenBitwiseNotExpr<T>(ZenBitwiseNotExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var v = expression.Expr.Accept(this);

                if (v is ZenConstantByteExpr xb)
                {
                    return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(~xb.Value));
                }

                if (v is ZenConstantShortExpr xs)
                {
                    return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(~xs.Value));
                }

                if (v is ZenConstantUshortExpr xus)
                {
                    return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(~xus.Value));
                }

                if (v is ZenConstantIntExpr xi)
                {
                    return (Zen<T>)(object)ZenConstantIntExpr.Create(~xi.Value);
                }

                if (v is ZenConstantUintExpr xui)
                {
                    return (Zen<T>)(object)ZenConstantUintExpr.Create(~xui.Value);
                }

                if (v is ZenConstantLongExpr xl)
                {
                    return (Zen<T>)(object)ZenConstantLongExpr.Create(~xl.Value);
                }

                if (v is ZenConstantUlongExpr xul)
                {
                    return (Zen<T>)(object)ZenConstantUlongExpr.Create(~xul.Value);
                }

                return ZenBitwiseNotExpr<T>.Create(expression.Expr.Accept(this));
            });
        }

        public Zen<T> VisitZenBitwiseOrExpr<T>(ZenBitwiseOrExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = expression.Expr1.Accept(this);
                var v2 = expression.Expr2.Accept(this);

                if (v1 is ZenConstantByteExpr xb && v2 is ZenConstantByteExpr yb)
                {
                    return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(xb.Value | yb.Value));
                }

                if (v1 is ZenConstantShortExpr xs && v2 is ZenConstantShortExpr ys)
                {
                    return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(xs.Value | ys.Value));
                }

                if (v1 is ZenConstantUshortExpr xus && v2 is ZenConstantUshortExpr yus)
                {
                    return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(xus.Value | yus.Value));
                }

                if (v1 is ZenConstantIntExpr xi && v2 is ZenConstantIntExpr yi)
                {
                    return (Zen<T>)(object)ZenConstantIntExpr.Create(xi.Value | yi.Value);
                }

                if (v1 is ZenConstantUintExpr xui && v2 is ZenConstantUintExpr yui)
                {
                    return (Zen<T>)(object)ZenConstantUintExpr.Create(xui.Value | yui.Value);
                }

                if (v1 is ZenConstantLongExpr xl && v2 is ZenConstantLongExpr yl)
                {
                    return (Zen<T>)(object)ZenConstantLongExpr.Create(xl.Value | yl.Value);
                }

                if (v1 is ZenConstantUlongExpr xul && v2 is ZenConstantUlongExpr yul)
                {
                    return (Zen<T>)(object)ZenConstantUlongExpr.Create(xul.Value | yul.Value);
                }

                return ZenBitwiseOrExpr<T>.Create(v1, v2);
            });
        }

        public Zen<T> VisitZenBitwiseXorExpr<T>(ZenBitwiseXorExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var v1 = expression.Expr1.Accept(this);
                var v2 = expression.Expr2.Accept(this);

                if (v1 is ZenConstantByteExpr xb && v2 is ZenConstantByteExpr yb)
                {
                    return (Zen<T>)(object)ZenConstantByteExpr.Create((byte)(xb.Value ^ yb.Value));
                }

                if (v1 is ZenConstantShortExpr xs && v2 is ZenConstantShortExpr ys)
                {
                    return (Zen<T>)(object)ZenConstantShortExpr.Create((short)(xs.Value ^ ys.Value));
                }

                if (v1 is ZenConstantUshortExpr xus && v2 is ZenConstantUshortExpr yus)
                {
                    return (Zen<T>)(object)ZenConstantUshortExpr.Create((ushort)(xus.Value ^ yus.Value));
                }

                if (v1 is ZenConstantIntExpr xi && v2 is ZenConstantIntExpr yi)
                {
                    return (Zen<T>)(object)ZenConstantIntExpr.Create(xi.Value ^ yi.Value);
                }

                if (v1 is ZenConstantUintExpr xui && v2 is ZenConstantUintExpr yui)
                {
                    return (Zen<T>)(object)ZenConstantUintExpr.Create(xui.Value ^ yui.Value);
                }

                if (v1 is ZenConstantLongExpr xl && v2 is ZenConstantLongExpr yl)
                {
                    return (Zen<T>)(object)ZenConstantLongExpr.Create(xl.Value ^ yl.Value);
                }

                if (v1 is ZenConstantUlongExpr xul && v2 is ZenConstantUlongExpr yul)
                {
                    return (Zen<T>)(object)ZenConstantUlongExpr.Create(xul.Value ^ yul.Value);
                }

                return ZenBitwiseXorExpr<T>.Create(v1, v2);
            });
        }

        public Zen<bool> VisitZenConstantBoolExpr(ZenConstantBoolExpr expression)
        {
            return expression;
        }

        public Zen<byte> VisitZenConstantByteExpr(ZenConstantByteExpr expression)
        {
            return expression;
        }

        public Zen<int> VisitZenConstantIntExpr(ZenConstantIntExpr expression)
        {
            return expression;
        }

        public Zen<long> VisitZenConstantLongExpr(ZenConstantLongExpr expression)
        {
            return expression;
        }

        public Zen<short> VisitZenConstantShortExpr(ZenConstantShortExpr expression)
        {
            return expression;
        }

        public Zen<uint> VisitZenConstantUintExpr(ZenConstantUintExpr expression)
        {
            return expression;
        }

        public Zen<ulong> VisitZenConstantUlongExpr(ZenConstantUlongExpr expression)
        {
            return expression;
        }

        public Zen<ushort> VisitZenConstantUshortExpr(ZenConstantUshortExpr expression)
        {
            return expression;
        }

        public Zen<TObject> VisitZenCreateObjectExpr<TObject>(ZenCreateObjectExpr<TObject> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var fieldValues = new List<(string, object)>();
                foreach (var fieldValuePair in expression.Fields)
                {
                    var field = fieldValuePair.Key;
                    dynamic value = fieldValuePair.Value;
                    fieldValues.Add((field, value.Accept(this)));
                }

                return ZenCreateObjectExpr<TObject>.Create(fieldValues.ToArray());
            });
        }

        public Zen<bool> VisitZenEqExpr<T>(ZenEqExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenEqExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<T2> VisitZenGetFieldExpr<T1, T2>(ZenGetFieldExpr<T1, T2> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var expr = expression.Expr.Accept(this);

                // there are several ways to create an object: createobject, getfield, withfield, adapter.
                // We only need to handle of the 3 here since getfield and adapter will be handled recursively.

                // get(with(o, name, f), name) == f
                // get(with(o, name', f), name) == get(o, name)
                if (expr is ZenWithFieldExpr<T1, T2> e1)
                {
                    return (e1.FieldName == expression.FieldName) ?
                            e1.FieldValue :
                            ZenGetFieldExpr<T1, T2>.Create(e1.Expr, expression.FieldName).Accept(this);
                }

                var type = expr.GetType();

                if (type.GetGenericTypeDefinition() == typeof(ZenWithFieldExpr<,>))
                {
                    var property = type.GetProperty("Expr");
                    return ZenGetFieldExpr<T1, T2>.Create((Zen<T1>)property.GetValue(expr), expression.FieldName).Accept(this);
                }

                // get(if e1 then e2 else e3, name) = if e1 then get(e2, name) else get(e3, name)
                if (expr is ZenIfExpr<T1> e2)
                {
                    var trueBranch = ZenGetFieldExpr<T1, T2>.Create(e2.TrueExpr, expression.FieldName);
                    var falseBranch = ZenGetFieldExpr<T1, T2>.Create(e2.FalseExpr, expression.FieldName);
                    return ZenIfExpr<T2>.Create(e2.GuardExpr, trueBranch.Accept(this), falseBranch.Accept(this));
                }

                var genericType = type.GetGenericTypeDefinition();

                // get(createobject(p1, ..., pn), namei) == pi
                if (expr is ZenCreateObjectExpr<T1> coe)
                {
                    return (Zen<T2>)coe.Fields[expression.FieldName];
                }

                return ZenGetFieldExpr<T1, T2>.Create(expr, expression.FieldName);
            });
        }

        public Zen<T> VisitZenIfExpr<T>(ZenIfExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var g = expression.GuardExpr.Accept(this);

                // if true then e1 else e2 = e1
                // if false then e1 else e2 = e2
                if (g is ZenConstantBoolExpr ce)
                {
                    if (ce.Value)
                    {
                        return expression.TrueExpr.Accept(this);
                    }
                    else
                    {
                        return expression.FalseExpr.Accept(this);
                    }
                }

                // if g then e else e = e
                var t = expression.TrueExpr.Accept(this);
                var f = expression.FalseExpr.Accept(this);
                if (ReferenceEquals(t, f))
                {
                    return t;
                }

                return ZenIfExpr<T>.Create(g, t, f);
            });
        }

        public Zen<bool> VisitZenLeqExpr<T>(ZenLeqExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenLeqExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<bool> VisitZenGeqExpr<T>(ZenGeqExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenGeqExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<T> VisitZenMaxExpr<T>(ZenMaxExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenMaxExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<T> VisitZenMinExpr<T>(ZenMinExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenMinExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<IList<T>> VisitZenListAddFrontExpr<T>(ZenListAddFrontExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenListAddFrontExpr<T>.Create(expression.Expr.Accept(this), expression.Element.Accept(this));
            });
        }

        public Zen<IList<T>> VisitZenListEmptyExpr<T>(ZenListEmptyExpr<T> expression)
        {
            return expression;
        }

        public Zen<TResult> VisitZenListMatchExpr<TList, TResult>(ZenListMatchExpr<TList, TResult> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var list = expression.ListExpr.Accept(this);

                // there are 5 ways to create a list: empty, addfront, if, match, getfield
                // however, the match case will be unrolled recusively, and the getfield
                // case should also be simplified, so we only need to handle the other 3 here.

                if (list is ZenListEmptyExpr<TList> l1)
                {
                    return expression.EmptyCase.Accept(this);
                }

                if (list is ZenListAddFrontExpr<TList> l2)
                {
                    return expression.ConsCase(l2.Element, l2.Expr).Accept(this);
                }

                if (list is ZenIfExpr<IList<TList>> l3)
                {
                    var tbranch = ZenListMatchExpr<TList, TResult>.Create(expression.UniqueId, l3.TrueExpr.Accept(this), expression.EmptyCase, expression.ConsCase);
                    var fbranch = ZenListMatchExpr<TList, TResult>.Create(expression.UniqueId, l3.FalseExpr.Accept(this), expression.EmptyCase, expression.ConsCase);
                    return ZenIfExpr<TResult>.Create(l3.GuardExpr.Accept(this), tbranch.Accept(this), fbranch.Accept(this));
                }

                return ZenListMatchExpr<TList, TResult>.Create(expression.UniqueId, list, expression.EmptyCase, expression.ConsCase);
            });
        }

        public Zen<T> VisitZenMinusExpr<T>(ZenMinusExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenMinusExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<T> VisitZenMultiplyExpr<T>(ZenMultiplyExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenMultiplyExpr<T>.Create(
                    expression.Expr1.Accept(this),
                    expression.Expr2.Accept(this));
            });
        }

        public Zen<bool> VisitZenNotExpr(ZenNotExpr expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var b = expression.Expr.Accept(this);

                if (b is ZenConstantBoolExpr x)
                {
                     return x.Value ? ZenConstantBoolExpr.False : ZenConstantBoolExpr.True;
                }

                if (b is ZenNotExpr y)
                {
                    return y.Expr;
                }

                return ZenNotExpr.Create(expression.Expr.Accept(this));
            });
        }

        public Zen<bool> VisitZenOrExpr(ZenOrExpr expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var b1 = expression.Expr1.Accept(this);
                var b2 = expression.Expr2.Accept(this);

                if (b1 is ZenConstantBoolExpr x)
                {
                    return (x.Value ? b1 : b2);
                }

                if (b2 is ZenConstantBoolExpr y)
                {
                    return (y.Value ? b2 : b1);
                }

                return ZenOrExpr.Create(b1, b2);
            });
        }

        public Zen<T> VisitZenSumExpr<T>(ZenSumExpr<T> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                return ZenSumExpr<T>.Create(expression.Expr1.Accept(this), expression.Expr2.Accept(this));
            });
        }

        public Zen<T1> VisitZenWithFieldExpr<T1, T2>(ZenWithFieldExpr<T1, T2> expression)
        {
            return LookupOrCompute(expression, () =>
            {
                var e = expression.Expr.Accept(this);
                var v = expression.FieldValue.Accept(this);
                return ZenWithFieldExpr<T1, T2>.Create(e, expression.FieldName, v);
            });
        }
    }
}
