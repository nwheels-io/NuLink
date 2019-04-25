using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters;
using NUnit.Framework;
using Shouldly;

namespace Simple.CommandLine.Tests
{
    public class ExpressionTests
    {
        [Test]
        public void TestInterpolatedStringExpression()
        {
            var expression = CallStringAsExpression(xx: 246, yy: "b");
            var methodCall = (MethodCallExpression)((LambdaExpression) expression).Body;

            for (int i = 0; i < 100; i++)
            {
                var format = EvaluateExpression(methodCall.Arguments[0]);
                var arg1 = EvaluateExpression(methodCall.Arguments[1]);
                var arg2 = EvaluateExpression(methodCall.Arguments[2]);

                Console.WriteLine($"Format = {format}");
                Console.WriteLine($"p1 = {arg1}");
                Console.WriteLine($"p2 = {arg2}");
            }

            object EvaluateExpression(Expression expr)
            {
                Stopwatch clock = Stopwatch.StartNew(); 
                
                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(expr);
                Func<object> compiled = lambda.Compile();
                var value = compiled();
                
                clock.Stop();
                Console.WriteLine($"EVAL = [{value}] : [{value?.GetType().FullName ?? "NULL"}], took {clock.Elapsed}");
                
                return value;
            }
                
            Expression CallStringAsExpression(int xx, string yy)
            {
                return StringAsExpression(() => $"Num={xx / 2:-5} Str={"a" + yy + "c"}");
            }
            
            Expression StringAsExpression(Expression<Func<string>> str)
            {
                return str;
            }
            
        }
    }
}