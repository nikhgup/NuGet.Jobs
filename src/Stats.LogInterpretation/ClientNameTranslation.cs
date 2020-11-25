﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stats.LogInterpretation
{
    public static class ClientNameTranslation
    {
        public static string GetClientCategory(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                return string.Empty;
            }

            if (clientName.Contains(ClientNames.NuGet))
            {
                return ClientCategories.NuGet;
            }

            if (clientName.Contains(ClientNames.WebMatrix))
            {
                return ClientCategories.WebMatrix;
            }

            if (clientName.Contains(ClientNames.NuGetPackageExplorer))
            {
                return ClientCategories.NuGetPackageExplorer;
            }

            if (clientName.Contains(ClientNames.Script))
            {
                return ClientCategories.Script;
            }

            if (clientName.Contains(ClientNames.Crawler))
            {
                return ClientCategories.Crawler;
            }

            if (clientName.Contains(ClientNames.Mobile))
            {
                return ClientCategories.Mobile;
            }

            // Check these late in the process, because other User Agents tend to also
            // send browser strings (e.g. PowerShell sends the Mozilla string along).
            if (clientName.Contains(ClientNames.Browser)
                || ClientNames.AbsoluteBrowserNames
                    .Any(abn => clientName.Equals(abn, StringComparison.OrdinalIgnoreCase)))
            {
                return ClientCategories.Browser;
            }

            // Explicitly categorize unknowns, test frameworks or others that should be filtered out in the reports
            if (clientName.Contains(ClientNames.Unknown))
            {
                return ClientCategories.Unknown;
            }

            // Return empty for all others to allow ecosystem user agents
            // to be picked up in the reports
            return string.Empty;
        }

        /// <summary>
        /// Extension method for ignore case `Contains` check.
        /// </summary>
        /// <param name="source">Source string in which to check the target</param>
        /// <param name="target">Target substring to verify</param>
        /// <param name="comparison">Instance of <see cref="StringComparison"/></param>
        /// <returns></returns>
        private static bool Contains(this string source, string target, StringComparison comparison)
        {
            return source?.IndexOf(target, comparison) >= 0;
        }

        /// <summary>
        /// Extension method that checks if any of the strings in the target list are a substring 
        /// in the source string.
        /// </summary>
        /// <param name="source">String to be checked</param>
        /// <param name="targetList">List of substrings to verify</param>
        /// <param name="comparison">Instane of <see cref="StringComparison"/>, defaults to <see cref="StringComparison.OrdinalIgnoreCase"/></param>
        /// <returns></returns>
        private static bool Contains(this string source, IList<string> targetList, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return targetList.Any(t => source.Contains(t, comparison));
        }
    }
}
