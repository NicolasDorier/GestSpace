using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class Metrics
	{
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if(n == 0)
			{
				return m;
			}

			if(m == 0)
			{
				return n;
			}

			// Step 2
			for(int i = 0 ; i <= n ; d[i, 0] = i++)
			{
			}

			for(int j = 0 ; j <= m ; d[0, j] = j++)
			{
			}

			// Step 3
			for(int i = 1 ; i <= n ; i++)
			{
				//Step 4
				for(int j = 1 ; j <= m ; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}
	public class FuzzyResult<T>
	{
		public int Distance
		{
			get;
			set;
		}
		public T Value
		{
			get;
			set;
		}
		public override string ToString()
		{
			return Value + " " + Distance;
		}
	}
	public class FuzzyCollection<T>
	{
		Func<T, T, int> _Metric;
		public FuzzyCollection(Func<T, T, int> metric)
		{
			_Metric = metric;
		}

		public List<FuzzyResult<T>> GetClosest(T source)
		{
			var results = _objs.Select(o => new FuzzyResult<T>
			{
				Distance = _Metric(source, o),
				Value = o
			}).ToList();
			results.Sort((a, b) => a.Distance.CompareTo(b.Distance));
			return results;
		}

		List<T> _objs = new List<T>();

		public T Add(T obj)
		{
			_objs.Add(obj);
			return obj;
		}
	}
}
