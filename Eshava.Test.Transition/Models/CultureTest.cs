using System;
using System.Collections.Generic;
using System.Text;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class CultureTest : IEmpty
	{
		public decimal NumberOne { get; set; }
		public double NumberTwo { get; set; }
		public float NumberThree { get; set; }
		public int NumberFour { get; set; }
		public long NumberFive { get; set; }

		public bool IsEmpty => false;
	}
}