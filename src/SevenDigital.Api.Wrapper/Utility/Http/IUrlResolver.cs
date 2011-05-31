﻿using System;
using System.Collections.Generic;
using System.Net;

namespace SevenDigital.Api.Wrapper.Utility.Http
{
	public interface IUrlResolver{
        string Resolve(Uri endpoint, string method, Dictionary<string,string> headers);
	}
}