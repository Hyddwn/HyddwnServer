// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Aura.Shared.Util
{
	public static class Extensions
	{
		/// <summary>
		/// Calculates differences between 2 strings.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Levenshtein_distance
		/// </remarks>
		/// <example>
		/// <code>
		/// "test".LevenshteinDistance("test")       // == 0
		/// "test1".LevenshteinDistance("test2")     // == 1
		/// "test1".LevenshteinDistance("test1 asd") // == 4
		/// </code>
		/// </example>
		public static int LevenshteinDistance(this string str, string compare, bool caseSensitive = true)
		{
			if (!caseSensitive)
			{
				str = str.ToLower();
				compare = compare.ToLower();
			}

			var sLen = str.Length;
			var cLen = compare.Length;
			var result = new int[sLen + 1, cLen + 1];

			if (sLen == 0)
				return cLen;

			if (cLen == 0)
				return sLen;

			for (int i = 0; i <= sLen; result[i, 0] = i++) ;
			for (int i = 0; i <= cLen; result[0, i] = i++) ;

			for (int i = 1; i <= sLen; i++)
			{
				for (int j = 1; j <= cLen; j++)
				{
					var cost = (compare[j - 1] == str[i - 1]) ? 0 : 1;
					result[i, j] = Math.Min(Math.Min(result[i - 1, j] + 1, result[i, j - 1] + 1), result[i - 1, j - 1] + cost);
				}
			}

			return result[sLen, cLen];
		}

		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static T Random<T>(this IList<T> list)
		{
			return list[RandomProvider.Get().Next(list.Count)];
		}

		/// <summary>
		/// Returns a random number between min and max (incl).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float Between(this Random rnd, float min, float max)
		{
			return (float)(min + (rnd.NextDouble() * (max - min)));
		}

		/// <summary>
		/// Returns a random value from the given ones.
		/// </summary>
		/// <param name="values"></param>
		public static T Rnd<T>(this Random rnd, params T[] values)
		{
			if (values == null || values.Length == 0)
				throw new ArgumentException("Values may not be null or empty.");

			return values[rnd.Next(values.Length)];
		}

		/// <summary>
		/// Breaks the specified string into chunks no longer than the
		/// given maximum length. This can be used for word wrapping purposes.
		/// 
		/// If a chunk would break in a word, the function backtracks to
		/// the previous splitter char, if there is one.
		/// 
		/// Splitters are defined by the localization files. In English:
		/// Splitter chars are hyphens and spaces. Spaces are stripped from the
		/// end of chunks while hyphens are not.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="maxChunkLength"></param>
		public static IEnumerable<string> Chunkify(this string str, int maxChunkLength)
		{
			return str.Chunkify(maxChunkLength, Localization.Get(" ").ToCharArray(), Localization.Get("-").ToCharArray());
		}

		/// <summary>
		/// Breaks the specified string into chunks no longer than the
		/// given maximum length. This can be used for word wrapping purposes.
		/// 
		/// If a chunk would break in a word, the function backtracks to
		/// the previous splitter char, if there is one.
		/// 
		/// Splitters in removedSplitters are stripped from the ends of chunks,
		/// while splitters in kepSplitters are not.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="maxChunkLength"></param>
		/// <param name="removedSplitters"></param>
		/// <param name="keptSplitters"></param>
		/// <returns></returns>
		public static IEnumerable<string> Chunkify(this string str, int maxChunkLength, char[] removedSplitters, char[] keptSplitters)
		{
			var splitters = removedSplitters.Concat(keptSplitters).ToArray();

			var startIndex = 0;

			while (startIndex < str.Length)
			{
				// Calculate the maximum length of this chunk.
				var maxIndex = Math.Min(startIndex + maxChunkLength, str.Length) - 1;

				// Try to make a chunk this big.
				var endIndex = maxIndex;

				if (!splitters.Contains(str[endIndex]) && (endIndex != str.Length - 1 && !splitters.Contains(str[endIndex + 1])))
				{
					// If the last char in our chunk is part of a word,
					// Try to find the start of the word
					endIndex = str.LastIndexOfAny(splitters, maxIndex);

					if (endIndex < startIndex) // We didn't find one in bounds
						endIndex = maxIndex; // So we have to return to splitting the word
				}

				// Make our chunk. We'll leave splitters at the start, if they exist.
				var chunk = str.Substring(startIndex, endIndex - startIndex + 1).TrimEnd(removedSplitters);

				// If we get a chunk that's all removed splitters, don't output it
				if (chunk.Length != 0)
					yield return chunk;

				// Start on the next chunk
				startIndex = endIndex + 1;
			}
		}
	}
}
