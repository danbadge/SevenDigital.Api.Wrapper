using System;
using System.Collections.Generic;
using System.Net;
using SevenDigital.Api.Wrapper.Environment;
using SevenDigital.Api.Wrapper.Exceptions;
using SevenDigital.Api.Wrapper.Http;
using SevenDigital.Api.Wrapper.Requests;
using SevenDigital.Api.Wrapper.Responses;
using SevenDigital.Api.Wrapper.Responses.Parsing;

namespace SevenDigital.Api.Wrapper
{
	public class FluentApi<T> : IFluentApi<T> where T : class
	{
		private IHttpClient _httpClient;
		private readonly IRequestBuilder _requestBuilder;

		private readonly RequestData _requestData;
		private readonly IResponseParser<T> _parser;
		private IResponseCache _responseCache = new NullResponseCache();

		public FluentApi(IHttpClient httpClient, IRequestBuilder requestBuilder) 
		{
			_httpClient = httpClient;
			_requestBuilder = requestBuilder;

			var attributeValidation = new AttributeRequestDataBuilder<T>();
			_requestData = attributeValidation.BuildRequestData();

			_parser = new ResponseParser<T>();
		}

		public FluentApi(IRequestBuilder requestBuilder) : this(new HttpClientMediator(), requestBuilder)
		{}

		public FluentApi(IOAuthCredentials oAuthCredentials, IApiUri apiUri)
			: this(new HttpClientMediator(), new RequestBuilder(apiUri, oAuthCredentials))
			{}

		public FluentApi()
			: this(new HttpClientMediator(), new RequestBuilder(EssentialDependencyCheck<IApiUri>.Instance, EssentialDependencyCheck<IOAuthCredentials>.Instance)) 
			{}

		public IFluentApi<T> UsingClient(IHttpClient httpClient)
		{
			if (httpClient == null)
			{
				throw new ArgumentNullException("httpClient");
			}

			_httpClient = httpClient;
			return this;
		}

		public IFluentApi<T> UsingCache(IResponseCache responseCache)
		{
			if (responseCache == null)
			{
				throw new ArgumentNullException("responseCache");
			}

			_responseCache = responseCache;
			return this;
		}

		public virtual IFluentApi<T> WithMethod(string methodName)
		{
			_requestData.HttpMethod =  HttpMethodHelpers.Parse(methodName);
			return this;
		}

		public virtual IFluentApi<T> WithParameter(string parameterName, string parameterValue)
		{
			_requestData.Parameters[parameterName] = parameterValue;
			return this;
		}

		public virtual IFluentApi<T> ClearParameters()
		{
			_requestData.Parameters.Clear();
			return this;
		}

		public virtual IFluentApi<T> ForUser(string token, string secret)
		{
			_requestData.UserToken = token;
			_requestData.TokenSecret = secret;
			return this;
		}

		public virtual IFluentApi<T> ForShop(int shopId)
		{
			WithParameter("shopId", shopId.ToString());
			return this;
		}

		public IFluentApi<T> WithPayload(string contentType, string payload)
		{
			_requestData.Payload = new RequestPayload(contentType, payload);
			return this;
		} 

		public virtual T Please()
		{
			Response response;

			var foundInCache = _responseCache.TryGet(_requestData, out response);
			if (! foundInCache)
			{
				try
				{
					var request = _requestBuilder.BuildRequest(_requestData);
					response = _httpClient.Send(request);
				}
				catch (WebException webException)
				{
					throw new ApiWebException(webException.Message, EndpointUrl, webException);
				}
			}

			try
			{
				var result = _parser.Parse(response);

				// set to cache only after all validation and parsing has suceeded
				if (!foundInCache)
				{
					_responseCache.Set(_requestData, response);
				}
				return result;
			}
			catch (ApiResponseException apiXmlException)
			{
				apiXmlException.Uri = EndpointUrl;
				throw;
			}
		}

		public virtual string EndpointUrl
		{
			get
			{
				var request = _requestBuilder.BuildRequest(_requestData);
				return request.Url;
			}
		}

		public IDictionary<string, string> Parameters
		{
			get { return _requestData.Parameters; }
		}
	}
}