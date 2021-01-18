using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using static Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request.SearchRequest;

[assembly: InternalsVisibleTo("Gatekeeper.Server.Web.Tests")]
namespace AuthServer.Server.Services.Ldap
{
    internal class SearchExpressionBuilder
    {
        public Expression Build(IFilterChoice filter, Expression itemExpression)
        {
            Expression? filterExpr = null;
            switch (filter)
            {
                case AndFilter af:
                    filterExpr = BuildAndFilter(af, itemExpression);
                    break;
                case OrFilter of:
                    filterExpr = BuildOrFilter(of, itemExpression);
                    break;
                case PresentFilter pf:
                    filterExpr = BuildPresentFilter(pf, itemExpression);
                    break;
                case EqualityMatchFilter eq:
                    filterExpr = BuildEqualityFilter(eq, itemExpression);
                    break;
                case SubstringFilter sf:
                    filterExpr = BuildSubstringFilter(sf, itemExpression);
                    break;
                default:
                    throw new NotImplementedException("Filter for " + filter.GetType() + " is not implemented");
            }

            return filterExpr;
        }

        private Expression BuildOrFilter(OrFilter filter, Expression itemExpression)
        {
            List<Expression> expressions = new List<Expression>();

            Expression? orFilterExpr = null;
            foreach (IFilterChoice subFilter in filter.Filters)
            {
                Expression subExpr = Build(subFilter, itemExpression);
                if (orFilterExpr == null)
                {
                    orFilterExpr = subExpr;
                }
                else
                {
                    orFilterExpr = Expression.Or(orFilterExpr, subExpr);
                }
            }

            return orFilterExpr;
        }

        private Expression BuildAndFilter(AndFilter filter, Expression itemExpression)
        {
            List<Expression> expressions = new List<Expression>();

            Expression? andFilterExpr = null;
            foreach (IFilterChoice subFilter in filter.Filters)
            {
                Expression subExpr = Build(subFilter, itemExpression);
                if (andFilterExpr == null)
                {
                    andFilterExpr = subExpr;
                }
                else
                {
                    andFilterExpr = Expression.And(andFilterExpr, subExpr);
                }
            }

            return andFilterExpr;
        }

        private Expression BuildPresentFilter(PresentFilter filter, Expression itemExpression)
        {
            List<string> existingAttributes = new List<string>() {
                "dn",
                "displayname",
                "email",
                "objectclass",
                "entryuuid",
            };

            if (existingAttributes.Contains(filter.Value.ToLower()))
            {
                Expression left = Expression.Constant(1);
                Expression right = Expression.Constant(1);

                return Expression.Equal(left, right);
            }
            else
            {
                Expression left = Expression.Constant(1);
                Expression right = Expression.Constant(2);

                return Expression.Equal(left, right);
            }
        }

        private string EscapeLikeString(string input)
        {
            input = input.Replace("%", "\\%");
            input = input.Replace("_", "\\_");

            return input;
        }

        private MethodCallExpression BuildLikeForPropertyAndString(Expression itemExpression, MemberExpression property, ConstantExpression search)
        {
            MethodInfo methodInfo = typeof(NpgsqlDbFunctionsExtensions)
                .GetMethods()
                .Where(p => p.Name == "ILike")
                .First();

            return Expression.Call(methodInfo, new Expression[]
            {
                Expression.Property(null, typeof(EF).GetProperty("Functions")),
                property,
                search
            });
        }

        private BinaryExpression GetAlwaysFalseExpression()
        {
            Expression left = Expression.Constant(1);
            Expression right = Expression.Constant(2);

            return Expression.Equal(left, right);
        }

        private BinaryExpression GetAlwaysTrueExpression()
        {
            Expression left = Expression.Constant(1);
            Expression right = Expression.Constant(1);

            return Expression.Equal(left, right);
        }

        private Expression BuildSubstringFilter(SubstringFilter filter, Expression itemExpression)
        {
            string suppliedLikeString = "";

            if (filter.Initial != null)
            {
                suppliedLikeString = EscapeLikeString(filter.Initial);
            }
            else
            {
                suppliedLikeString = "%";
            }

            foreach (string anyString in filter.Any)
            {
                suppliedLikeString = suppliedLikeString + "%" + EscapeLikeString(anyString) + "%";
            }

            if (filter.Final != null)
            {
                suppliedLikeString = suppliedLikeString + EscapeLikeString(filter.Final);
            }
            else
            {
                suppliedLikeString = suppliedLikeString + "%";
            }

            ConstantExpression searchExpr = Expression.Constant(suppliedLikeString);

            if (filter.AttributeDesc == "cn")
            {
                MemberExpression emailProperty = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.CN);
                return BuildLikeForPropertyAndString(itemExpression, emailProperty, searchExpr);
            }
            else if (filter.AttributeDesc == "email")
            {
                MemberExpression emailProperty = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.Email);
                return BuildLikeForPropertyAndString(itemExpression, emailProperty, searchExpr);
            }
            else if (filter.AttributeDesc == "displayname")
            {
                MemberExpression emailProperty = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.Email);
                return BuildLikeForPropertyAndString(itemExpression, emailProperty, searchExpr);
            }
            else if (filter.AttributeDesc == "entryuuid")
            {
                MemberExpression emailProperty = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.EntryUUID);
                return BuildLikeForPropertyAndString(itemExpression, emailProperty, searchExpr);
            }

            return GetAlwaysFalseExpression();
        }

        private MemberExpression GetMemberExpressionForAttribute(Expression itemExpression, MemberExpressionAttributes attribute)
        {
            string name = "";
            switch (attribute)
            {
                case MemberExpressionAttributes.CN:
                    name = "NormalizedUserName";
                    break;
                case MemberExpressionAttributes.Email:
                    name = "NormalizedEmail";
                    break;
                case MemberExpressionAttributes.DisplayName:
                    name = "NormalizedUserName";
                    break;
                case MemberExpressionAttributes.EntryUUID:
                    name = "Id";
                    break;
            }

            return MemberExpression.Property(itemExpression, name);
        }

        private enum MemberExpressionAttributes
        {
            CN = 1,
            Email = 2,
            DisplayName = 3,
            EntryUUID = 4,
        }

        private Expression BuildEqualityFilter(EqualityMatchFilter filter, Expression itemExpression)
        {
            if (filter.AttributeDesc == "cn")
            {
                Expression left = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.CN);
                Expression right = Expression.Constant(filter.AssertionValue.ToUpper());
                return Expression.Equal(left, right);
            }
            else if (filter.AttributeDesc == "email")
            {
                Expression left = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.Email);
                Expression right = Expression.Constant(filter.AssertionValue.ToUpper());
                return Expression.Equal(left, right);
            }
            else if (filter.AttributeDesc == "displayname")
            {
                Expression left = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.DisplayName);
                Expression right = Expression.Constant(filter.AssertionValue.ToUpper());
                return Expression.Equal(left, right);
            }
            else if (filter.AttributeDesc == "entryuuid")
            {
                Expression left = GetMemberExpressionForAttribute(itemExpression, MemberExpressionAttributes.EntryUUID);
                Expression right = Expression.Constant(new Guid(filter.AssertionValue));
                return Expression.Equal(left, right);
            }
            else if (filter.AttributeDesc == "objectclass")
            {
                if (filter.AssertionValue == "inetOrgPerson")
                {
                    return GetAlwaysTrueExpression();
                }
                else
                {
                    return GetAlwaysFalseExpression();
                }
            }
            else if (filter.AttributeDesc == "dn")
            {
                return GetAlwaysFalseExpression();
            }

            return GetAlwaysFalseExpression();
        }
    }
}
