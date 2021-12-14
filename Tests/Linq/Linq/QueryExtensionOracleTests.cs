﻿using System;
using System.Linq;

using LinqToDB;
using LinqToDB.DataProvider.Oracle;

using NUnit.Framework;

namespace Tests.Linq
{
	[TestFixture]
	public class QueryExtensionOracleTests : TestBase
	{
		[Test]
		public void TableSubqueryHintTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in
					(
						from p in db.Parent.TableHint(Hints.TableHint.Full).With(Hints.TableHint.Cache)
						select p
					)
					.AsSubQuery()
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ FULL(p_1.p) CACHE(p_1.p) */"));
		}

		[Test]
		public void TableSubqueryHintTest2([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in
					(
						from p in
							(
								from p in db.Parent.TableHint(Hints.TableHint.Full)
								from c in db.Child.TableHint(Hints.TableHint.Full)
								select p
							)
							.AsSubQuery()
						select p
					)
					.AsSubQuery()
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ FULL(p_2.p_1.p) FULL(p_2.p_1.c_1)"));
		}

		[Test]
		public void TableHintTest([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.TableHint.Cache,
				Hints.TableHint.Cluster,
				Hints.TableHint.DrivingSite,
				Hints.TableHint.Fact,
				Hints.TableHint.Full,
				Hints.TableHint.Hash,
				Hints.TableHint.NoCache,
				Hints.TableHint.NoFact,
				Hints.TableHint.NoParallel,
				Hints.TableHint.NoPxJoinFilter,
				Hints.TableHint.NoUseHash,
				Hints.TableHint.PxJoinFilter
				)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.With(hint)
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ {hint}(p) */"));
		}

		[Test]
		public void TableHintDynamicSamplingTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.TableHint(Hints.TableHint.DynamicSampling, 1)
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ DYNAMIC_SAMPLING(p 1)"));
		}

		[Test]
		public void TableHintParallelTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.TableHintEx(Hints.TableHint.Parallel, ", ", 5)
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ PARALLEL(p, 5)"));
		}

		[Test]
		public void TableHintParallelDefaultTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.TableHintEx(Hints.TableHint.Parallel, ", ", "DEFAULT")
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ PARALLEL(p, DEFAULT)"));
		}

		[Test]
		public void IndexHintSingleTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.TableHint(Hints.IndexHint.Index, "parent_ix")
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ INDEX(p parent_ix)"));
		}

		[Test]
		public void IndexHintTest([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.IndexHint.Index,
				Hints.IndexHint.IndexAsc,
				Hints.IndexHint.IndexCombine,
				Hints.IndexHint.IndexDesc,
				Hints.IndexHint.IndexFastFullScan,
				Hints.IndexHint.IndexJoin,
				Hints.IndexHint.IndexSkipScan,
				Hints.IndexHint.IndexSkipScanAsc,
				Hints.IndexHint.IndexSkipScanDesc,
				Hints.IndexHint.NoIndex,
				Hints.IndexHint.NoIndexFastFullScan,
				Hints.IndexHint.NoIndexSkipScan,
				Hints.IndexHint.NoParallelIndex,
				Hints.IndexHint.ParallelIndex,
				Hints.IndexHint.UseNlWithIndex
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
				from p in db.Parent.TableHint(hint, "parent_ix", "parent2_ix")
				select p;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ {hint}(p parent_ix parent2_ix)"));
		}

		[Test]
		public void QueryHintTest([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.AllRows,
				Hints.QueryHint.Append,
				Hints.QueryHint.CursorSharingExact,
				Hints.QueryHint.ModelMinAnalysis,
				Hints.QueryHint.NoAppend,
				Hints.QueryHint.NoExpand,
				Hints.QueryHint.NoPushSubQuery,
				Hints.QueryHint.NoRewrite,
				Hints.QueryHint.NoQueryTransformation,
				Hints.QueryHint.NoStarTransformation,
				Hints.QueryHint.NoUnnest,
				Hints.QueryHint.NoXmlQueryRewrite,
				Hints.QueryHint.Ordered,
				Hints.QueryHint.PushSubQueries,
				Hints.QueryHint.Rule,
				Hints.QueryHint.StarTransformation,
				Hints.QueryHint.Unnest,
				Hints.QueryHint.UseConcat
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from c in db.Child
				join p in db.Parent on c.ParentID equals p.ParentID
				select p
			)
			.QueryHint(hint);

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ {hint} */"));
		}

		[Test]
		public void QueryHintWithQueryBlockTest([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.NoUnnest
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from c in db.Child
				join p in
					db.Parent.AsSubQuery("Parent")
					on c.ParentID equals p.ParentID
				select p
			)
			.QueryHint(hint, "@Parent");

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ {hint}(@Parent) */"));
			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ QB_NAME(Parent) */"));
		}

		[Test]
		public void QueryHintFirstRowsTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q =
				(
					from c in db.Child
					join p in db.Parent on c.ParentID equals p.ParentID
					select p
				)
				.QueryHint(Hints.QueryHint.FirstRows(25));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ FIRST_ROWS(25) */"));
		}

		[Test]
		public void UnionTest([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q1 =
				from c in db.Child
				join p in db.Parent.TableHint(Hints.TableHint.Full) on c.ParentID equals p.ParentID
				select p;

			var q =
				q1.QueryName("qb_1").Union(q1.QueryName("qb_2"));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ QB_NAME(qb_1) FULL(p@qb_1) FULL(p_1@qb_2) */"));
		}

		[Test]
		public void UnionTest2([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q1 =
				from c in db.Child
				join p in db.Parent.TableHint(Hints.TableHint.Full) on c.ParentID equals p.ParentID
				select p;

			var q =
				from p in q1.QueryName("qb_1").Union(q1.QueryName("qb_2"))
				where p.ParentID > 0
				select p.ParentID;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ FULL(p@qb_1) FULL(p_1@qb_2) */"));
			Assert.That(LastQuery, Contains.Substring("SELECT /*+ QB_NAME(qb_1) */"));
			Assert.That(LastQuery, Contains.Substring("SELECT /*+ QB_NAME(qb_2) */"));
		}

		[Test]
		public void UnionTest3([IncludeDataSources(true, TestProvName.AllOracle)] string context)
		{
			using var db = GetDataContext(context);

			var q1 =
				from c in db.Child
				join p in db.Parent.TableHint(Hints.TableHint.Full) on c.ParentID equals p.ParentID
				select p;

			var q =
				from p in q1.Union(q1)
				where p.ParentID > 0
				select p.ParentID;

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring("SELECT /*+ FULL(p_2.p) FULL(p_2.p_1) */"));
		}

		[Test]
		public void TableIDTest([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.Leading
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from p in db.Parent.TableHint(Hints.TableHint.Full).TableID("Pr")
				from c in db.Child.TableID("Ch")
				select p
			)
			.QueryHint(hint, Sql.TableID("Pr"), Sql.TableID("Ch"));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ FULL(p) {hint}(p c_1) */"));
		}

		[Test]
		public void TableIDTest2([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.Leading
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from p in
					(
						from p in
							(
								from p in db.Parent.TableHint(Hints.TableHint.Full).TableID("Pr")
								from c in db.Child.TableID("Ch")
								select p
							)
							.AsSubQuery()
						select p
					)
					.AsSubQuery()
				select p
			)
			.QueryHint(hint, Sql.TableID("Pr"), Sql.TableID("Ch"));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ FULL(p_2.p_1.p) {hint}(p_2.p_1.p p_2.p_1.c_1) */"));
		}

		[Test]
		public void TableIDTest3([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.Leading
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from p in
					(
						from p in
							(
								from p in db.Parent.TableHint(Hints.TableHint.Full).TableID("Pr")
								where p.ParentID < 0
								select p
							)
							.AsSubQuery()
						select p
					)
					.AsSubQuery("qn")
				from c in db.Child.TableID("Ch")
				select p
			)
			.QueryHint(hint, Sql.TableID("Pr"), Sql.TableID("Ch"));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ FULL(p_1.p@qn) {hint}(p_1.p@qn c_1) */"));
		}

		[Test]
		public void TableIDTest4([IncludeDataSources(true, TestProvName.AllOracle)] string context,
			[Values(
				Hints.QueryHint.Leading
			)] string hint)
		{
			using var db = GetDataContext(context);

			var q =
			(
				from p in
					(
						from p in
							(
								from p in db.Parent.TableHint(Hints.TableHint.Full).TableID("Pr")
								where p.ParentID < 0
								select p
							)
							.AsSubQuery("qn")
						from c in db.Child.TableID("Ch")
						select p
					)
					.AsSubQuery()
				select p
			)
			.QueryHint(hint, Sql.TableID("Pr"), Sql.TableID("Ch"));

			_ = q.ToList();

			Assert.That(LastQuery, Contains.Substring($"SELECT /*+ FULL(p@qn) {hint}(p@qn p_2.c_1) */"));
		}
	}
}
