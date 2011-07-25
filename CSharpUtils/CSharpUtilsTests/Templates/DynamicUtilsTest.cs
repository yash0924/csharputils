﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Templates.Runtime;
using System.Collections;

namespace CSharpUtilsTests.Templates
{
	[TestClass]
	public class DynamicUtilsTest
	{
		[TestMethod]
		public void TestConvertToBool()
		{
			Assert.AreEqual(true, DynamicUtils.ConvertToBool((float)7.1));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool((double)7.1));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(7));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(true));
			Assert.AreEqual(true, DynamicUtils.ConvertToBool(new Object()));

			Assert.AreEqual(false, DynamicUtils.ConvertToBool(0));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool((double)0.0));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool(false));
			Assert.AreEqual(false, DynamicUtils.ConvertToBool(null));
		}

		[TestMethod]
		public void TestAdd()
		{
			Assert.AreEqual("10a", DynamicUtils.BinaryOperation_Add(10, "a"));
			Assert.AreEqual("a", DynamicUtils.BinaryOperation_Add(null, "a"));
			Assert.AreEqual("a", DynamicUtils.BinaryOperation_Add("a", null));
			Assert.AreEqual(null, DynamicUtils.BinaryOperation_Add(null, null));
			Assert.AreEqual(21.5, DynamicUtils.BinaryOperation_Add(10, 11.5));
		}

		[TestMethod]
		public void TestAccess()
		{
			Assert.AreEqual("Value", DynamicUtils.Access(new Hashtable() { { "MyKey", "Value" } }, "MyKey"));
			Assert.AreEqual(10, DynamicUtils.Access(new ClassTestAccess(), "SampleField"));
			Assert.AreEqual(20, DynamicUtils.Access(new ClassTestAccess(), "SampleProperty"));
			Assert.AreEqual(30, DynamicUtils.Access(new ClassTestAccess(), "SampleMethod"));
			Assert.AreEqual(null, DynamicUtils.Access(null, "Test"));
			Assert.AreEqual(null, DynamicUtils.Access(null, null));
			Assert.AreEqual(null, DynamicUtils.Access(10, "Test"));
			Assert.AreEqual(null, DynamicUtils.Access("Test", "Test"));
			Assert.AreEqual(2, DynamicUtils.Access(new int[] { 0, 1, 2, 3, 4 }, 2));
			Assert.AreEqual(2, DynamicUtils.Access(new List<int>(new int[] { 0, 1, 2, 3, 4 }), 2));
		}

		class ClassTestAccess
		{
			public int SampleField = 10;

			public int SampleProperty {
				get
				{
					return 20;
				}
			}

			public int SampleMethod()
			{
				return 30;
			}
		}
	}
}