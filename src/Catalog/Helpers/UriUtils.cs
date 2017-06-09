﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace NuGet.Services.Metadata.Catalog.Helpers
{
    public static class UriUtils
    {
        private const char QueryStartCharacter = '?';
        private const char BridgingCharacter = '&';

        private const string Filter = "$filter=";
        private const string NonhijackableFilter = "true";
        private const string And = "%20and%20";

        private const string OrderBy = "$orderby=";
        private const string NonhijackableOrderBy = "Version";

        private const string GetSpecificPackageFormatRegExp = 
            @"Packages\(Id='(?<Id>[^']*)',Version='(?<Version>[^']*)'\)";

        /// <summary>
        /// The format of a nonhijacked "Packages(Id='...',Version='...')" request.
        /// Note that we are using "normalized version" here.
        /// This is because a hijacked request does a comparison on the normalized version, but the nonhijacked request does not.
        /// </summary>
        private static string GetSpecificPackageNonhijackable = 
            $"Packages?{Filter}{NonhijackableFilter}{And}" + 
            "Id eq '{0}'" + $"{And}" + "NormalizedVersion eq '{1}'";

        private static IEnumerable<string> HijackableEndpoints = new List<string>
        {
            "/Packages",
            "/Search",
            "/FindPackagesById"
        };

        public static Uri GetNonhijackableUri(Uri originalUri)
        {
            var nonhijackableUri = originalUri;

            var originalUriString = originalUri.ToString();
            string nonhijackableUriString = null;

            // Modify the request uri so that it will not be hijacked by the search service.

            // This can be done in two ways:
            /// 1 - convert the query into a "Packages" query with filter that cannot be hijacked (<see cref="NonhijackableFilter"/>).
            /// 2 - specify an orderby that cannot be hijacked (<see cref="NonhijackableOrderBy"/>).
            if (originalUriString.Contains(OrderBy))
            {
                // If there is an orderby on the request, simply replace the orderby with a nonhijackable orderby.
                var orderByStartIndex = originalUriString.IndexOf(OrderBy);

                /// Find the start of the next parameter (<see cref="BridgingCharacter"/>) or the end of the query.
                var orderByEndIndex = originalUriString.IndexOf(BridgingCharacter, orderByStartIndex);
                if (orderByEndIndex == -1)
                {
                    orderByEndIndex = originalUriString.Length;
                }

                // Replace the entire orderby with a nonhijackable orderby.
                var orderByExpression = originalUriString.Substring(orderByStartIndex, orderByEndIndex - orderByStartIndex);
                nonhijackableUriString = originalUriString.Replace(orderByExpression, OrderBy + NonhijackableOrderBy);
            }
            else
            {
                // If this is a Packages(Id='...',Version='...') request, rewrite it as a request to Packages() with a filter and add the expression.
                // Note that Packages() returns a feed and Packages(Id='...',Version='...') returns an entry, but this is fine because the client reads both the same.
                var getSpecificPackageMatch = Regex.Match(originalUriString, GetSpecificPackageFormatRegExp);
                if (getSpecificPackageMatch.Success)
                {
                    nonhijackableUriString =
                        originalUriString.Substring(0, getSpecificPackageMatch.Index) +
                        string.Format(
                            GetSpecificPackageNonhijackable, 
                            getSpecificPackageMatch.Groups["Id"].Value, 
                            NuGetVersion.Parse(getSpecificPackageMatch.Groups["Version"].Value).ToNormalizedString());
                }
                else
                {
                    // If this is a request to a hijackable endpoint without an orderby, add the orderby to the request.
                    if (HijackableEndpoints.Any(endpoint => originalUriString.Contains(endpoint)))
                    {
                        var bridgingCharacter = BridgingCharacter;

                        if (!originalUriString.Contains(QueryStartCharacter))
                        {
                            bridgingCharacter = QueryStartCharacter;
                        }

                        nonhijackableUriString = $"{originalUri}{bridgingCharacter}{OrderBy}{NonhijackableOrderBy}";
                    }
                }
            }

            if (nonhijackableUriString != null)
            {
                nonhijackableUri = new Uri(nonhijackableUriString);
            }

            return nonhijackableUri;
        }

        /// <summary>
        /// Makes a web request to <paramref name="uri"/> and returns <see cref="HttpRequestMessage.RequestUri"/> of the <see cref="HttpResponseMessage.RequestMessage"/>.
        /// In other words, if <paramref name="uri"/> redirects to another address, this method returns the address that was redirected to.
        /// </summary>
        public static async Task<Uri> GetRedirectedRequestMessageUri(HttpClient client, Uri uri)
        {
            var response = await client.GetAsync(uri);
            return response.RequestMessage.RequestUri;
        }
    }
}