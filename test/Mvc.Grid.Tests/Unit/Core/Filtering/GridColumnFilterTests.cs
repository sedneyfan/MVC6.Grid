﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace NonFactors.Mvc.Grid.Tests.Unit
{
    public class GridColumnFilterTests
    {
        private GridColumnFilter<GridModel, String> filter;
        private IQueryable<GridModel> items;

        public GridColumnFilterTests()
        {
            Grid<GridModel> grid = new Grid<GridModel>(new GridModel[0]);
            GridColumn<GridModel, String> column = new GridColumn<GridModel, String>(grid, model => model.Name);

            filter = new GridColumnFilter<GridModel, String>(column) { IsEnabled = true };

            items = new[]
            {
                new GridModel { Name = "aa", NSum = 10, Sum = 40 },
                new GridModel { Name = "Aa", NSum = 15, Sum = 35 },
                new GridModel { Name = "AA", NSum = 20, Sum = 35 },
                new GridModel { Name = "bb", NSum = 20, Sum = 30 },
                new GridModel { Name = "Bb", NSum = 25, Sum = 25 },
                new GridModel { Name = "BB", NSum = 30, Sum = 15 },
                new GridModel { Name = null, NSum = 30, Sum = 20 },
                new GridModel { Name = "Cc", NSum = null, Sum = 10 }
            }.AsQueryable();
        }

        #region Options

        [Fact]
        public void Options_Set_Caches()
        {
            Object expected = filter.Options = new List<SelectListItem>();
            Object actual = filter.Options;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Options_Get_FromFilters()
        {
            filter.Column.Grid.ViewContext = new ViewContext();
            IGridFilters filters = Substitute.For<IGridFilters>();
            filters.GetFilterOptions(filter.Column).Returns(new SelectListItem[0]);
            filter.Column.Grid.ViewContext.HttpContext = Substitute.For<HttpContext>();
            filter.Column.Grid.ViewContext.HttpContext.RequestServices.GetService<IGridFilters>().Returns(filters);

            Object expected = filters.GetFilterOptions(filter.Column);
            Object actual = filter.Options;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Options_Get_Caches()
        {
            filter.Column.Grid.ViewContext = new ViewContext();
            IGridFilters filters = Substitute.For<IGridFilters>();
            filters.GetFilterOptions(filter.Column).Returns(new SelectListItem[0]);
            filter.Column.Grid.ViewContext.HttpContext = Substitute.For<HttpContext>();
            filter.Column.Grid.ViewContext.HttpContext.RequestServices.GetService<IGridFilters>().Returns(filters);

            Object options = filter.Options;

            filters.GetFilterOptions(filter.Column).Returns(new SelectListItem[0]);

            Object actual = filter.Options;
            Object expected = options;

            Assert.Same(expected, actual);
        }

        #endregion

        #region Operator

        [Fact]
        public void Operator_Set_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-op=and");

            filter.Operator = null;

            Assert.Null(filter.Operator);
        }

        [Theory]
        [InlineData("", "", null)]
        [InlineData("", "op", null)]
        [InlineData("", "name-op", "")]
        [InlineData("", "name-op=", "")]
        [InlineData("", "name-op=or", "or")]
        [InlineData("", "name-op=and", "and")]
        [InlineData("", "name-op-and=and", null)]
        [InlineData("", "name-op=and&name-op=or", "and")]
        [InlineData("", "NAME-OP=AND&NAME-OP=OR", "and")]
        [InlineData(null, "", null)]
        [InlineData(null, "op", null)]
        [InlineData(null, "name-op", "")]
        [InlineData(null, "name-op=", "")]
        [InlineData(null, "name-op=or", "or")]
        [InlineData(null, "name-op=and", "and")]
        [InlineData(null, "name-op-and=and", null)]
        [InlineData(null, "name-op=and&name-op=or", "and")]
        [InlineData(null, "NAME-OP=AND&NAME-OP=OR", "and")]
        [InlineData("grid", "", null)]
        [InlineData("grid", "name-op", null)]
        [InlineData("grid", "grid-name-op", "")]
        [InlineData("grid", "grid-name-op=", "")]
        [InlineData("grid", "grid-name-op=or", "or")]
        [InlineData("grid", "grid-name-op=and", "and")]
        [InlineData("grid", "grid-name-op-and=and", null)]
        [InlineData("grid", "grid-name-op=and&grid-name-op=or", "and")]
        [InlineData("grid", "GRID-NAME-OP=AND&GRID-NAME-OP=OR", "and")]
        public void Operator_Get_FromQuery(String name, String query, String op)
        {
            filter.Column.Grid.Name = name;
            filter.Column.Grid.Query = HttpUtility.ParseQueryString(query);

            String actual = filter.Operator;
            String expected = op;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Operator_Get_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-op=or");

            String op = filter.Operator;

            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-op=and");

            String actual = filter.Operator;
            String expected = op;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region First

        [Fact]
        public void First_Set_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-contains=a");

            filter.First = null;

            Assert.Null(filter.First);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "name=a")]
        [InlineData("", "name-=a")]
        [InlineData("", "name-eq=a")]
        [InlineData(null, "")]
        [InlineData(null, "name=a")]
        [InlineData(null, "name-=a")]
        [InlineData(null, "name-eq=a")]
        [InlineData("grid", "")]
        [InlineData("grid", "grid-")]
        [InlineData("grid", "grid-name=a")]
        [InlineData("grid", "grid-name-=a")]
        [InlineData("grid", "grid-name-eq=a")]
        public void First_Get_NotFoundReturnNull(String name, String query)
        {
            filter.Column.Grid.Name = name;
            filter.Column.Grid.Query = HttpUtility.ParseQueryString(query);

            Assert.Null(filter.First);
        }

        [Theory]
        [InlineData("", "name-equals=a&name-eq=b", "a")]
        [InlineData("", "name-equals=&name-equals=b", "")]
        [InlineData("", "name-equals=&name-contains=b", "")]
        [InlineData("", "name-equals=a&name-contains=b", "a")]
        [InlineData("", "name-equals=a&name-equals=b", "a")]
        [InlineData("", "name-equals=a&name-contains=b&name-op=or", "a")]
        [InlineData("", "NAME-EQUALS=A&NAME-CONTAINS=B&NAME-OP=OR", "A")]
        [InlineData(null, "name-equals=a&name-eq=b", "a")]
        [InlineData(null, "name-equals=&name-equals=b", "")]
        [InlineData(null, "name-equals=&name-contains=b", "")]
        [InlineData(null, "name-equals=a&name-contains=b", "a")]
        [InlineData(null, "name-equals=a&name-equals=b", "a")]
        [InlineData(null, "name-equals=a&name-contains=b&name-op=or", "a")]
        [InlineData(null, "NAME-EQUALS=A&NAME-CONTAINS=B&NAME-OP=OR", "A")]
        [InlineData("grid", "grid-name-equals=a&grid-name-eq=b", "a")]
        [InlineData("grid", "grid-name-equals=&grid-name-equals=b", "")]
        [InlineData("grid", "grid-name-equals=&grid-name-contains=b", "")]
        [InlineData("grid", "grid-name-equals=a&grid-name-contains=b", "a")]
        [InlineData("grid", "grid-name-equals=a&grid-name-equals=b", "a")]
        [InlineData("grid", "grid-name-equals=a&grid-name-contains=b&grid-name-op=or", "a")]
        [InlineData("grid", "GRID-NAME-EQUALS=A&GRID-NAME-CONTAINS=B&GRID-NAME-OP=OR", "A")]
        public void First_Get_FromQuery(String name, String query, String value)
        {
            filter.Column.Grid.Name = name;
            filter.Column.Grid.Query = HttpUtility.ParseQueryString(query);

            IGridFilter actual = filter.First;

            Assert.Equal(typeof(StringEqualsFilter), actual.GetType());
            Assert.Equal("equals", actual.Method);
            Assert.Equal(value, actual.Value);
        }

        [Fact]
        public void First_Get_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-contains=a");

            IGridFilter expected = filter.First;

            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-equals=b");

            IGridFilter actual = filter.First;

            Assert.Equal(typeof(StringContainsFilter), actual.GetType());
            Assert.Equal("contains", actual.Method);
            Assert.Equal("a", actual.Value);
            Assert.Same(expected, actual);
        }

        #endregion

        #region Second

        [Fact]
        public void Second_Set_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-contains=a&name-equals=b");

            filter.Second = null;

            Assert.Null(filter.Second);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "name=a")]
        [InlineData("", "name-=a")]
        [InlineData("", "name-eq=a")]
        [InlineData("", "name-equals=a")]
        [InlineData("", "name-equals=a&")]
        [InlineData("", "name-equals=a&name=b")]
        [InlineData("", "name-equals=a&name-=b")]
        [InlineData("", "name-equals=a&name-eq=b")]
        [InlineData(null, "")]
        [InlineData(null, "name=a")]
        [InlineData(null, "name-=a")]
        [InlineData(null, "name-eq=a")]
        [InlineData(null, "name-equals=a")]
        [InlineData(null, "name-equals=a&")]
        [InlineData(null, "name-equals=a&name=b")]
        [InlineData(null, "name-equals=a&name-=b")]
        [InlineData(null, "name-equals=a&name-eq=b")]
        [InlineData("grid", "")]
        [InlineData("grid", "grid-")]
        [InlineData("grid", "grid-name=a")]
        [InlineData("grid", "grid-name-=a")]
        [InlineData("grid", "grid-name-eq=a")]
        [InlineData("grid", "grid-name-equals=a")]
        [InlineData("grid", "grid-name-equals=a&")]
        [InlineData("grid", "grid-name-equals=a&grid-name=b")]
        [InlineData("grid", "grid-name-equals=a&grid-name-=b")]
        [InlineData("grid", "grid-name-equals=a&grid-name-eq=b")]
        public void Second_Get_NotFoundReturnNull(String name, String query)
        {
            filter.Column.Grid.Name = name;
            filter.Column.Grid.Query = HttpUtility.ParseQueryString(query);

            Assert.Null(filter.Second);
        }

        [Theory]
        [InlineData("", "name-eq=a&name-equals=b", "b")]
        [InlineData("", "name-equals=a&name-equals=b", "b")]
        [InlineData("", "name-contains=a&name-equals=", "")]
        [InlineData("", "name-equals=a&name-equals=", "")]
        [InlineData("", "name-contains=a&name-equals=ba", "ba")]
        [InlineData("", "name-contains=a&name-equals=b&name-op=or", "b")]
        [InlineData("", "NAME-CONTAINS=A&NAME-EQUALS=B&NAME-OP=OR", "B")]
        [InlineData(null, "name-eq=a&name-equals=b", "b")]
        [InlineData(null, "name-equals=a&name-equals=b", "b")]
        [InlineData(null, "name-contains=a&name-equals=", "")]
        [InlineData(null, "name-equals=a&name-equals=", "")]
        [InlineData(null, "name-contains=a&name-equals=ba", "ba")]
        [InlineData(null, "name-contains=a&name-equals=b&name-op=or", "b")]
        [InlineData(null, "NAME-CONTAINS=A&NAME-EQUALS=B&NAME-OP=OR", "B")]
        [InlineData("grid", "grid-name-eq=a&grid-name-equals=b", "b")]
        [InlineData("grid", "grid-name-equals=a&grid-name-equals=b", "b")]
        [InlineData("grid", "grid-name-contains=a&grid-name-equals=", "")]
        [InlineData("grid", "grid-name-equals=a&grid-name-equals=", "")]
        [InlineData("grid", "grid-name-contains=a&grid-name-equals=ba", "ba")]
        [InlineData("grid", "grid-name-contains=a&grid-name-equals=b&grid-name-op=or", "b")]
        [InlineData("grid", "GRID-NAME-CONTAINS=A&GRID-NAME-EQUALS=B&GRID-NAME-OP=OR", "B")]
        public void Second_Get_FromQuery(String name, String query, String value)
        {
            filter.Column.Grid.Name = name;
            filter.Column.Grid.Query = HttpUtility.ParseQueryString(query);

            IGridFilter actual = filter.Second;

            Assert.Equal(typeof(StringEqualsFilter), actual.GetType());
            Assert.Equal("equals", actual.Method);
            Assert.Equal(value, actual.Value);
        }

        [Fact]
        public void Second_Get_Caches()
        {
            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-contains=a&name-equals=b");

            IGridFilter expected = filter.Second;

            filter.Column.Grid.Query = HttpUtility.ParseQueryString("name-starts-with=d&name-ends-with=e");

            IGridFilter actual = filter.Second;

            Assert.Equal(typeof(StringEqualsFilter), actual.GetType());
            Assert.Equal("equals", actual.Method);
            Assert.Equal("b", actual.Value);
            Assert.Same(expected, actual);
        }

        #endregion

        #region GridColumnFilter(IGridColumn<T, TValue> column)

        [Fact]
        public void GridColumnFilter_SetsColumn()
        {
            Object actual = new GridColumnFilter<GridModel, String>(filter.Column).Column;
            Object expected = filter.Column;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void GridColumnFilter_SetsNameForEnum()
        {
            AssertFilterNameFor(model => model.EnumField, "enum");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForSByte()
        {
            AssertFilterNameFor(model => model.SByteField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForByte()
        {
            AssertFilterNameFor(model => model.ByteField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForInt16()
        {
            AssertFilterNameFor(model => model.Int16Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForUInt16()
        {
            AssertFilterNameFor(model => model.UInt16Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForInt32()
        {
            AssertFilterNameFor(model => model.Int32Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForUInt32()
        {
            AssertFilterNameFor(model => model.UInt32Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForInt64()
        {
            AssertFilterNameFor(model => model.Int64Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForUInt64()
        {
            AssertFilterNameFor(model => model.UInt64Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForSingle()
        {
            AssertFilterNameFor(model => model.SingleField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForDouble()
        {
            AssertFilterNameFor(model => model.DoubleField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForDecimal()
        {
            AssertFilterNameFor(model => model.DecimalField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForString()
        {
            AssertFilterNameFor(model => model.StringField, "text");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForBoolean()
        {
            AssertFilterNameFor(model => model.BooleanField, "boolean");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForDateTime()
        {
            AssertFilterNameFor(model => model.DateTimeField, "date");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableEnum()
        {
            AssertFilterNameFor(model => model.NullableEnumField, "enum");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableSByte()
        {
            AssertFilterNameFor(model => model.NullableSByteField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableByte()
        {
            AssertFilterNameFor(model => model.NullableByteField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableInt16()
        {
            AssertFilterNameFor(model => model.NullableInt16Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableUInt16()
        {
            AssertFilterNameFor(model => model.NullableUInt16Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableInt32()
        {
            AssertFilterNameFor(model => model.NullableInt32Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableUInt32()
        {
            AssertFilterNameFor(model => model.NullableUInt32Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableInt64()
        {
            AssertFilterNameFor(model => model.NullableInt64Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableUInt64()
        {
            AssertFilterNameFor(model => model.NullableUInt64Field, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableSingle()
        {
            AssertFilterNameFor(model => model.NullableSingleField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableDouble()
        {
            AssertFilterNameFor(model => model.NullableDoubleField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableDecimal()
        {
            AssertFilterNameFor(model => model.NullableDecimalField, "number");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableBoolean()
        {
            AssertFilterNameFor(model => model.NullableBooleanField, "boolean");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForNullableDateTime()
        {
            AssertFilterNameFor(model => model.NullableDateTimeField, "date");
        }

        [Fact]
        public void GridColumnFilter_SetsNameForOtherTypes()
        {
            AssertFilterNameFor(model => model, null);
        }

        [Fact]
        public void GridColumnFilter_NotMemberExpression_IsNotEnabled()
        {
            IGridColumn<GridModel, String> column = new GridColumn<GridModel, String>(filter.Column.Grid, model => model.ToString());

            Assert.False(new GridColumnFilter<GridModel, String>(column).IsEnabled);
        }

        [Fact]
        public void GridColumnFilter_MemberExpression_IsEnabledNull()
        {
            IGridColumn<GridModel, String> column = new GridColumn<GridModel, String>(filter.Column.Grid, model => model.Name);

            Assert.Null(new GridColumnFilter<GridModel, String>(column).IsEnabled);
        }

        #endregion

        #region Apply(IQueryable<T> items)

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public void Apply_NotEnabled_ReturnsSameItems(Boolean? isEnabled)
        {
            filter.IsMulti = true;
            filter.IsEnabled = isEnabled;
            filter.First = new StringContainsFilter { Value = "A" };

            Object actual = filter.Apply(items);
            Object expected = items;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Apply_NoFilters_ReturnsSameItems()
        {
            filter.First = null;
            filter.Second = null;
            filter.IsMulti = true;
            filter.IsEnabled = true;

            Object expected = items;
            Object actual = filter.Apply(items);

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Apply_NullAppliedFilter_ReturnsSameItems()
        {
            filter.IsMulti = true;
            filter.Operator = "or";
            filter.First = Substitute.For<IGridFilter>();
            filter.Second = Substitute.For<IGridFilter>();

            Object expected = items;
            Object actual = filter.Apply(items);

            Assert.Same(expected, actual);
        }

        [Theory]
        [InlineData("or")]
        [InlineData("OR")]
        public void Apply_UsingAndOperator(String op)
        {
            filter.Operator = op;
            filter.IsMulti = true;
            filter.First = new StringContainsFilter { Value = "a" };
            filter.Second = new StringContainsFilter { Value = "aA" };

            IQueryable expected = items.Where(item => item.Name != null && item.Name.ToUpper().Contains("A") && item.Name.ToUpper().Contains("AA"));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("or")]
        [InlineData("OR")]
        public void Apply_UsingOrOperator(String op)
        {
            filter.Operator = op;
            filter.IsMulti = true;
            filter.First = new StringContainsFilter { Value = "a" };
            filter.Second = new StringContainsFilter { Value = "bB" };

            IQueryable expected = items.Where(item => item.Name != null && (item.Name.ToUpper().Contains("A") || item.Name.ToUpper().Contains("BB")));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("xor")]
        public void Apply_InvalidOperator_FirstFilter(String op)
        {
            filter.Operator = op;
            filter.IsMulti = true;
            filter.First = new StringContainsFilter { Value = "a" };
            filter.Second = new StringContainsFilter { Value = "BB" };

            IQueryable expected = items.Where(item => item.Name != null && item.Name.ToUpper().Contains("A"));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("xor")]
        public void Apply_InvalidOperator_SecondFilter(String op)
        {
            filter.Operator = op;
            filter.IsMulti = true;
            filter.First = Substitute.For<IGridFilter>();
            filter.Second = new StringContainsFilter { Value = "a" };

            IQueryable expected = items.Where(item => item.Name != null && item.Name.ToUpper().Contains("A"));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Apply_FirstFilter()
        {
            filter.IsMulti = false;
            filter.Operator = "or";
            filter.First = new StringContainsFilter { Value = "a" };
            filter.Second = new StringContainsFilter { Value = "bb" };

            IQueryable expected = items.Where(item => item.Name != null && item.Name.ToUpper().Contains("A"));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Apply_SecondFilter()
        {
            filter.IsMulti = false;
            filter.Operator = "or";
            filter.First = Substitute.For<IGridFilter>();
            filter.Second = new StringContainsFilter { Value = "bB" };

            IQueryable expected = items.Where(item => item.Name != null && item.Name.ToUpper().Contains("BB"));
            IQueryable actual = filter.Apply(items);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Apply_FiltersByExpressions()
        {
            GridColumn<GridModel, Int32?> testColumn = new GridColumn<GridModel, Int32?>(new Grid<GridModel>(new GridModel[0]), model => model.NSum);
            GridColumnFilter<GridModel, Int32?> testFilter = new GridColumnFilter<GridModel, Int32?>(testColumn);
            testFilter.Second = new NumberFilter<Int32> { Method = "greater-than", Value = "25" };
            testFilter.First = new NumberFilter<Int32> { Method = "equals", Value = "10" };
            testFilter.IsEnabled = true;
            testFilter.Operator = "or";
            testFilter.IsMulti = true;

            IQueryable expected = items.Where(item => item.NSum == 10 || item.NSum > 25);
            IQueryable actual = testFilter.Apply(items);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Test helpers

        private void AssertFilterNameFor<TValue>(Expression<Func<GridModel, TValue>> property, String name)
        {
            Grid<GridModel> grid = new Grid<GridModel>(new GridModel[0]);

            String actual = new GridColumnFilter<GridModel, TValue>(new GridColumn<GridModel, TValue>(grid, property)).Name;
            String expected = name;

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
