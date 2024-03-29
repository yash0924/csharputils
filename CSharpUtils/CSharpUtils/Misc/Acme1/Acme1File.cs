﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Extensions;
using System.Text.RegularExpressions;
using System.Collections;

namespace CSharpUtils.Misc
{
	public class Acme1File : IEnumerable<Acme1File.Entry>
	{
		public class Entry
		{
			public int Id;
			public String Text;
		}

		Dictionary<int, Entry> Entries = new Dictionary<int, Entry>();

		public Acme1File()
		{
		}

		public void Load(Stream Stream, Encoding Encoding)
		{
			Entries.Clear();
			var AllContent = Stream.ReadAllContentsAsString(Encoding, true).TrimStart();
			var Parts = AllContent.Split(new string[] { "## POINTER " }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var Part in Parts)
			{
				//Console.WriteLine(Part.EscapeString());

				var Subparts = Part.Split(new string[] { "\r\n", "\r", "\n" }, 2, StringSplitOptions.None);
				var InfoMatch = Regex.Match(Subparts[0], @"(\d+).*$", RegexOptions.Compiled | RegexOptions.Multiline);
				var Text = Subparts[1].TrimEnd();
				var Id = ConvertEx.FlexibleToInt(InfoMatch.Groups[1].Value);

				Entries[Id] = new Entry()
				{
					Id = Id,
					Text = Text,
				};
				//Console.WriteLine(Subparts.ToStringArray().EscapeString());
			}
		}

		public Entry this[int Index]
		{
			get
			{
				return Entries[Index];
			}
		}

		public IEnumerator<Acme1File.Entry> GetEnumerator()
		{
			return Entries.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Entries.Values.GetEnumerator();
		}
	}
}
