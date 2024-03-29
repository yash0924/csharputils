﻿using CSharpUtils.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CSharpUtilsTests
{
	[TestClass]
	public class StructExtensionsTest
	{
		public struct TestStruct
		{
			public int Field1;
			public int Field2;
			public int Field3;
			public string Field4;
		}

		[TestMethod]
		public void ToStringDefaultTest()
		{
			Assert.AreEqual(
				"TestStruct(Field1=1,Field2=2,Field3=3,Field4=Hello World!)",
				new TestStruct()
				{
					Field1 = 1,
					Field2 = 2,
					Field3 = 3,
					Field4 = "Hello World!",
				}.ToStringDefault()
			);
		}
	}
}
