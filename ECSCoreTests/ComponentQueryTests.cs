using System;
using System.Collections.Generic;
using System.Text;
using Core.ECS;
using Xunit;

namespace CoreTests
{
	public class ComponentQueryTests
	{
		public EntityArchetype archetypeEmpty = EntityArchetype.Empty;
		public EntityArchetype archetypeC1C2S1;
		public EntityArchetype archetypeC1C2S1S2;
		public EntityArchetype archetypeC1;
		public EntityArchetype archetypeC1C2;
		public EntityArchetype archetypeC1S1;
		public EntityArchetype archetypeC2S1;

		public ComponentQueryTests() {
			var shared1 = new SharedComponent1();
			var shared2 = new SharedComponent2();

			archetypeC1 = EntityArchetype.Empty.Add<TestComponent1>();
			archetypeC1C2 = archetypeC1.Add<TestComponent2>();
			archetypeC1S1 = archetypeC1.AddShared(shared1);
			archetypeC1C2S1 = archetypeC1S1.Add<TestComponent2>();
			archetypeC1C2S1S2 = archetypeC1C2S1.AddShared(shared2);

			archetypeC2S1 = EntityArchetype.Empty.Add<TestComponent2>().AddShared(shared1);
		}

		[Fact]
		public void Include() {
			ComponentQuery query = new ComponentQuery();
			query.Include<TestComponent1>();
			query.Include<TestComponent2>();

			Assert.False(query.Matches(archetypeEmpty));
			Assert.True(query.Matches(archetypeC1C2S1));
			Assert.True(query.Matches(archetypeC1C2S1S2));
			Assert.False(query.Matches(archetypeC1));
			Assert.True(query.Matches(archetypeC1C2));
			Assert.False(query.Matches(archetypeC1S1));
			Assert.False(query.Matches(archetypeC2S1));

		}

		[Fact]
		public void Exclude()
		{
			ComponentQuery query = new ComponentQuery();
			query.Exclude<TestComponent2>();

			Assert.True(query.Matches(archetypeEmpty));
			Assert.False(query.Matches(archetypeC1C2S1));
			Assert.False(query.Matches(archetypeC1C2S1S2));
			Assert.True(query.Matches(archetypeC1));
			Assert.False(query.Matches(archetypeC1C2));
			Assert.True(query.Matches(archetypeC1S1));
			Assert.False(query.Matches(archetypeC2S1));
		}

		[Fact]
		public void IncludeExclude()
		{
			ComponentQuery query = new ComponentQuery();
			query.Include<TestComponent1>();
			query.Exclude<TestComponent2>();

			Assert.False(query.Matches(archetypeEmpty));
			Assert.False(query.Matches(archetypeC1C2S1));
			Assert.False(query.Matches(archetypeC1C2S1S2));
			Assert.True(query.Matches(archetypeC1));
			Assert.False(query.Matches(archetypeC1C2));
			Assert.True(query.Matches(archetypeC1S1));
			Assert.False(query.Matches(archetypeC2S1));
		}

		[Fact]
		public void IncludeShared()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeShared<SharedComponent1>();
			query.IncludeShared<SharedComponent2>();

			Assert.False(query.Matches(archetypeEmpty));
			Assert.False(query.Matches(archetypeC1C2S1));
			Assert.True(query.Matches(archetypeC1C2S1S2));
			Assert.False(query.Matches(archetypeC1));
			Assert.False(query.Matches(archetypeC1C2));
			Assert.False(query.Matches(archetypeC1S1));
			Assert.False(query.Matches(archetypeC2S1));

		}

		[Fact]
		public void ExcludeShared()
		{
			ComponentQuery query = new ComponentQuery();
			query.ExcludeShared<SharedComponent2>();

			Assert.True(query.Matches(archetypeEmpty));
			Assert.True(query.Matches(archetypeC1C2S1));
			Assert.False(query.Matches(archetypeC1C2S1S2));
			Assert.True(query.Matches(archetypeC1));
			Assert.True(query.Matches(archetypeC1C2));
			Assert.True(query.Matches(archetypeC1S1));
			Assert.True(query.Matches(archetypeC2S1));
		}

		[Fact]
		public void IncludeExcludeShared()
		{
			ComponentQuery query = new ComponentQuery();
			query.IncludeShared<SharedComponent1>(); 
			query.ExcludeShared<SharedComponent2>();

			Assert.False(query.Matches(archetypeEmpty));
			Assert.True(query.Matches(archetypeC1C2S1));
			Assert.False(query.Matches(archetypeC1C2S1S2));
			Assert.False(query.Matches(archetypeC1));
			Assert.False(query.Matches(archetypeC1C2));
			Assert.True(query.Matches(archetypeC1S1));
			Assert.True(query.Matches(archetypeC2S1));
		}

		[Fact]
		public void IncludeExcludeAll()
		{
			ComponentQuery query = new ComponentQuery();
			query.Include<TestComponent1>();
			query.Exclude<TestComponent2>();
			query.IncludeShared<SharedComponent1>();
			query.ExcludeShared<SharedComponent2>();

			Assert.False(query.Matches(archetypeEmpty));
			Assert.False(query.Matches(archetypeC1C2S1));
			Assert.False(query.Matches(archetypeC1C2S1S2));
			Assert.False(query.Matches(archetypeC1));
			Assert.False(query.Matches(archetypeC1C2));
			Assert.True(query.Matches(archetypeC1S1));
			Assert.False(query.Matches(archetypeC2S1));
		}

		[Fact]
		public void HashCodeZero()
		{
			ComponentQuery query1 = new ComponentQuery();
			Assert.Equal(0, query1.GetHashCode());
		}

		[Fact]
		public void HashCodeNotZero() {
			ComponentQuery query1 = new ComponentQuery();
			query1.Include<TestComponent1>();
			Assert.NotEqual(0, query1.GetHashCode());

			query1 = new ComponentQuery();
			query1.Exclude<TestComponent1>();
			Assert.NotEqual(0, query1.GetHashCode());

			query1 = new ComponentQuery();
			query1.IncludeShared<SharedComponent1>();
			Assert.NotEqual(0, query1.GetHashCode());

			query1 = new ComponentQuery();
			query1.ExcludeShared<SharedComponent1>();
			Assert.NotEqual(0, query1.GetHashCode());
		}

		[Fact]
		public void HashCodeSame() {
			ComponentQuery query1 = new ComponentQuery();
			query1.Include<TestComponent1>();
			ComponentQuery query2 = new ComponentQuery();
			query2.Include<TestComponent1>();

			Assert.Equal(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.Exclude<TestComponent1>();
			query2 = new ComponentQuery();
			query2.Exclude<TestComponent1>();

			Assert.Equal(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.IncludeShared<SharedComponent1>();
			query2 = new ComponentQuery();
			query2.IncludeShared<SharedComponent1>();

			Assert.Equal(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.ExcludeShared<SharedComponent1>();
			query2 = new ComponentQuery();
			query2.ExcludeShared<SharedComponent1>();

			Assert.Equal(query1.GetHashCode(), query2.GetHashCode());
		}

		[Fact]
		public void HashCodeDifferent()
		{
			ComponentQuery query1 = new ComponentQuery();
			query1.Include<TestComponent1>();
			ComponentQuery query2 = new ComponentQuery();
			query2.Include<TestComponent2>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.Exclude<TestComponent1>();
			query2 = new ComponentQuery();
			query2.Exclude<TestComponent2>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.IncludeShared<SharedComponent1>();
			query2 = new ComponentQuery();
			query2.IncludeShared<SharedComponent2>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.ExcludeShared<SharedComponent1>();
			query2 = new ComponentQuery();
			query2.ExcludeShared<SharedComponent2>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.Include<TestComponent1>();
			query2 = new ComponentQuery();
			query2.Exclude<TestComponent1>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());

			query1 = new ComponentQuery();
			query1.IncludeShared<SharedComponent1>();
			query2 = new ComponentQuery();
			query2.ExcludeShared<SharedComponent1>();

			Assert.NotEqual(query1.GetHashCode(), query2.GetHashCode());
		}
	}
}
