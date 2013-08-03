using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class Helper
	{
		public static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}
		public static double Map(double value,
							  double istart,
							  double istop,
							  double ostart,
							  double ostop)
		{
			return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
		}

	}
}
