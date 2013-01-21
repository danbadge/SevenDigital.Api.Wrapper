﻿using System;
using NUnit.Framework;
using SevenDigital.Api.Schema;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests
{
	[TestFixture]
	public class StatusTests
	{
		[Test]
		public async void Can_hit_endpoint()
		{
			Status status = await Api<Status>.Create.PleaseAsync();

			Assert.That(status, Is.Not.Null);
			Assert.That(status.ServerTime.Day, Is.EqualTo(DateTime.Now.Day));
		}
	}
}
