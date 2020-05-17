﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Model;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Provides methods that are nice for <see cref="List{T}"/>
	/// </summary>
	public static class EnumerableExtension {

		/// <summary>
		/// Sets the contents of the given <see cref="List{T}"/> to the given content.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="values"></param>
		public static void SetFrom<T>(this List<T> list, IEnumerable<T> values) {
			list.Clear();
			list.Capacity = values.Count();
			for (int idx = 0; idx < values.Count(); idx++) {
				list.Add(values.ElementAt(idx));
			}
		}

		/// <summary>
		/// Converts this <see cref="IEnumerable{T}"/> into a 2D array of <typeparamref name="T"/>, where the second dimension's size is <paramref name="groupSize"/>. For instance, calling this on an array of 16 items with an argument of 8 will return a 2D array with 2 elements in the first dimension, and 8 in the second dimension.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="groupSize">The size of the second dimension of the array.</param>
		/// <returns></returns>
		/// <exception cref="DataMisalignedException">Thrown if the length of the given <paramref name="enumerable"/> is not divisible by <paramref name="groupSize"/>.</exception>
		public static T[,] As2D<T>(this IEnumerable<T> enumerable, int groupSize) {
			int eCount = enumerable.Count();
			if (eCount % groupSize != 0) throw new DataMisalignedException($"Cannot convert Enumerable<T> with {eCount} elements into 2D array with a group size of {groupSize} ({eCount} is not divisible by {groupSize}!)");

			T[,] retnArray = new T[eCount / groupSize, groupSize];
			int baseIdx = 0;
			for (int i0 = 0; i0 < eCount / groupSize; i0++) {
				for (int i1 = 0; i1 < groupSize; i1++) {
					retnArray[i0, i1] = enumerable.ElementAt(baseIdx);
					baseIdx++;
				}
			}
			return retnArray;
		}

		/// <summary>
		/// Given an <paramref name="index"/>, this will return the second dimension of the 2D array. If the 2D array were constructed as an array of arrays, this would be the equivalent to getting array[x] in array[x][y].
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array2D"></param>
		/// <param name="index">The index in the first dimension corresponding to the desired second dimension.</param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of the range fo the array.</exception>
		public static T[] GetSecondDimensionAt<T>(this T[,] array2D, int index) {
			T[] retn = new T[array2D.GetLength(1)];
			for (int idx = 0; idx < retn.Length; idx++) {
				retn[idx] = array2D[index, idx];
			}
			return retn;
		}

		/// <summary>
		/// A very specialized method designed specifically for <see cref="GeometryConfigTranslator"/> which converts a float array to an int array.
		/// </summary>
		/// <param name="floatArray"></param>
		/// <returns></returns>
		public static int[] ToIntArray(this float[] floatArray) {
			int[] retn = new int[floatArray.Length];
			for (int idx = 0; idx < floatArray.Length; idx++) {
				retn[idx] = (int)floatArray[idx];
			}
			return retn;
		}

		/// <summary>
		/// Similar to <see cref="Array.CopyTo(Array, int)"/> but for <see cref="List{T}"/>s.<para/>
		/// If the offset is larger than the size of the list, the empty space will be filled with <see langword="default"/>.<para/>
		/// If the offset is somewhere within the list, it will overwrite elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="list"></param>
		/// <param name="offset"></param>
		public static void CopyToList<T>(this IEnumerable<T> enumerable, List<T> list, int offset = 0) {
			if (list.Count < offset) {
				int numElementsToAdd = offset - list.Count;
				for (int idx = 0; idx < numElementsToAdd; idx++) {
					list.Add(default);
				}
			}

			list.Capacity = Math.Max(enumerable.Count() + offset, list.Capacity);
			for (int idx = 0; idx < enumerable.Count(); idx++) {
				int listIdx = idx + offset;
				list[listIdx] = enumerable.ElementAt(idx);
			}
		}

		/// <summary>
		/// A utility designed exclusively for the glTF exporter which can populate a list with the given default value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="defaultValue"></param>
		/// <param name="values"></param>
		public static void SetListCap<T>(this List<dynamic> list, T defaultValue, int values) {
			list.Clear();
			list.Capacity = values;
			for (int idx = 0; idx < values; idx++) {
				list.Add(defaultValue);
			}
		}

	}
}
