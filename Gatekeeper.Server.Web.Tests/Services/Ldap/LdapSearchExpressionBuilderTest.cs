using System.Collections.Generic;
using System.Linq.Expressions;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Ldap;
using Xunit;
using static Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request.SearchRequest;

namespace AuthServer.Server.Tests.Services.Ldap
{
    public class LdapSearchExpressionBuilderTest
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void TestBuild(IFilterChoice filter, string expected)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(AppUser));

            SearchExpressionBuilder builder = new SearchExpressionBuilder();
            Expression result = builder.Build(filter, paramExpression);

            Assert.Equal(expected, result.ToString());
        }
        public static IEnumerable<object[]> GetData()
        {
            return new List<object[]>
            {
                new object[] { new EqualityMatchFilter { AttributeDesc = "cn", AssertionValue = "test@example.com" }, "(Param_0.NormalizedUserName == \"TEST@EXAMPLE.COM\")" },
                new object[] { new AndFilter { Filters = new List<IFilterChoice>{ new EqualityMatchFilter{AttributeDesc = "cn", AssertionValue = "test@example.com" }, new EqualityMatchFilter{AttributeDesc = "displayname", AssertionValue = "Test User" }}}, "((Param_0.NormalizedUserName == \"TEST@EXAMPLE.COM\") And (Param_0.NormalizedUserName == \"TEST USER\"))" },
                new object[] { new OrFilter { Filters = new List<IFilterChoice>{ new EqualityMatchFilter{AttributeDesc = "cn", AssertionValue = "test@example.com" }, new EqualityMatchFilter{AttributeDesc = "displayname", AssertionValue = "Test User" }}}, "((Param_0.NormalizedUserName == \"TEST@EXAMPLE.COM\") Or (Param_0.NormalizedUserName == \"TEST USER\"))" },
                new object[] { new PresentFilter { Value = "NotExistingAttribute" }, "(1 == 2)" },
                new object[] { new PresentFilter { Value = "ObjectClass" }, "(1 == 1)" },
                new object[] { new SubstringFilter { AttributeDesc = "cn", Initial = "Test", Any=new List<string>(){"s"}, Final = "er"}, "EF.Functions.ILike(Param_0.NormalizedUserName, \"Test%s%er\")" },
                new object[] { new SubstringFilter { AttributeDesc = "cn", Initial = "Test%", Any=new List<string>(){"s"}, Final = "er"}, "EF.Functions.ILike(Param_0.NormalizedUserName, \"Test\\%%s%er\")" },
                new object[] { new SubstringFilter { AttributeDesc = "cn", Final = "er"}, "EF.Functions.ILike(Param_0.NormalizedUserName, \"%er\")" },
                new object[] { new SubstringFilter { AttributeDesc = "cn", Initial = "Test"}, "EF.Functions.ILike(Param_0.NormalizedUserName, \"Test%\")" },
            };
        }
    }
}
