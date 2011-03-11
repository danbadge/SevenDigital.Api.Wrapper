﻿using System.Linq;
using NUnit.Framework;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.Schema.Tags;
using SevenDigital.Api.Wrapper.Utility.Http;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.TagsEndpoint
{
	[TestFixture]
	[Category("Integration")]
	public class ArtistTagsTests
	{
		[Test]
		public void Can_hit_endpoint()
		{
			var httpGetResolver = new EndpointResolver(new HttpGetResolver());

			ArtistTags tags = new FluentApi<ArtistTags>(httpGetResolver)
				.WithParameter("artistId", "1")
				.Resolve();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags.TagList.Count, Is.GreaterThan(0));
			Assert.That(tags.TagList.FirstOrDefault().Id, Is.Not.Empty);
			Assert.That(tags.TagList.Where(x => x.Id == "rock").FirstOrDefault().Text, Is.EqualTo("rock"));
		}

		[Test]
		public void Can_hit_endpoint_with_paging()
		{
			var httpGetResolver = new EndpointResolver(new HttpGetResolver());

			ArtistTags artistBrowse = new FluentApi<ArtistTags>(httpGetResolver)
				.WithParameter("artistId", "1")
				.WithParameter("page", "2")
				.WithParameter("pageSize", "1")
				.Resolve();

			Assert.That(artistBrowse, Is.Not.Null);
			Assert.That(artistBrowse.Page, Is.EqualTo(2));
			Assert.That(artistBrowse.PageSize, Is.EqualTo(1));
		}
	}
}