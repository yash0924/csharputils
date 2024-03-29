﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Templates.TemplateProvider;
using CSharpUtils.Templates.Utils;
using CSharpUtils.Templates.Runtime;
using System.Reflection.Emit;
using System.Reflection;

namespace CSharpUtils.Templates.ParserNodes
{
	abstract public class ParserNode
	{
		virtual public ParserNode Optimize(ParserNodeContext Context)
		{
			return this;
		}

		virtual public void Dump(int Level = 0, String Info = "")
		{
			Console.WriteLine("{0}{1}:{2}", new String(' ', Level * 4), Info, this);
		}

		virtual public void WriteTo(ParserNodeContext Context)
		{
		}

		virtual public void GenerateIL(ILGenerator ILGenerator, ParserNodeContext Context)
		{
			throw(new NotImplementedException());
		}

		protected T CreateThisInstanceAs<T>()
		{
			return (T)(Activator.CreateInstance(this.GetType()));
		}

		/*
		override public ParserNode Optimize(ParserNodeContext Context)
		{
			ParserNodeParent ParserNodeParent = Activator.CreateInstance(this.GetType());
			ParserNodeParent.Parent = Parent.Optimize(Context);
			return ParserNodeParent;
		}
			* */


		public override string ToString()
		{
			return String.Format("{0}", this.GetType().Name);
		}

		internal void OptimizeAndWrite(ParserNodeContext Context)
		{
			Optimize(Context).WriteTo(Context);
		}
	}

	public class DummyParserNode : ParserNode
	{
	}

	public class ParserNodeIf : ParserNode
	{
		public ParserNode ConditionNode;
		public ParserNode BodyIfNode;
		public ParserNode BodyElseNode;

		public override void Dump(int Level = 0, String Info = "")
		{
			base.Dump(Level, Info);
			ConditionNode.Dump(Level + 1, "Condition");
			BodyIfNode.Dump(Level + 1, "IfBody");
			BodyElseNode.Dump(Level + 1, "ElseBody");
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("if (DynamicUtils.ConvertToBool(");
			ConditionNode.WriteTo(Context);
			Context.Write(")) {");
			Context.WriteLine("");
			BodyIfNode.WriteTo(Context);
			Context.Write("}");
			if (!(BodyElseNode is DummyParserNode))
			{
				Context.Write(" else {");
				BodyElseNode.WriteTo(Context);
				Context.Write("}");
			}
			Context.WriteLine("");
		}
	}

	public class ParserNodeContainer : ParserNode
	{
		protected List<ParserNode> Nodes;

		public override void Dump(int Level = 0, String Info = "")
		{
			base.Dump(Level, Info);
			int n = 0;
			foreach (var Node in Nodes)
			{
				Node.Dump(Level + 1, String.Format("Node{0}", n));
				n++;
			}
		}

		public ParserNodeContainer()
		{
			Nodes = new List<ParserNode>();
		}

		public void Add(ParserNode Node)
		{
			Nodes.Add(Node);
		}

		override public ParserNode Optimize(ParserNodeContext Context)
		{
			ParserNodeContainer OptimizedNode = CreateThisInstanceAs<ParserNodeContainer>();
			foreach (var Node in Nodes)
			{
				OptimizedNode.Add(Node.Optimize(Context));
			}
			return OptimizedNode;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			foreach (var Node in Nodes)
			{
				Node.WriteTo(Context);
			}
		}
	}

	public class ParserNodeIdentifier : ParserNode
	{
		protected String Id;

		public ParserNodeIdentifier(String Id)
		{
			this.Id = Id;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("Context.GetVar({0})", StringUtils.EscapeString(Id));
		}

		public override string ToString()
		{
			return base.ToString() + "('" + Id + "')";
		}
	}

	public class ParserNodeConstant : ParserNode
	{
		protected String Name;

		public ParserNodeConstant(String Name)
		{
			this.Name = Name;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			switch (Name)
			{
				case "true": Context.Write("true"); break;
				case "false": Context.Write("false"); break;
				case "none": Context.Write("null"); break;
				default: throw(new Exception(String.Format("Unknown constant '{0}'", Name)));
			}
		}

		public override string ToString()
		{
			return base.ToString() + "('" + Name + "')";
		}
	}

	public class ParserNodeNumericLiteral : ParserNode
	{
		public long Value;

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("{0}", Value);
		}

		override public void GenerateIL(ILGenerator ILGenerator, ParserNodeContext Context)
		{
			ILGenerator.Emit(OpCodes.Ldc_I8, Value);
		}

		public override string ToString()
		{
			return base.ToString() + "(" + Value + ")";
		}
	}

	public class ParserNodeStringLiteral : ParserNode
	{
		protected String Value;

		public ParserNodeStringLiteral(String Value)
		{
			this.Value = Value;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write(StringUtils.EscapeString(Value));
		}

		override public void GenerateIL(ILGenerator ILGenerator, ParserNodeContext Context)
		{
			ILGenerator.Emit(OpCodes.Ldstr, Value);
		}

		public override string ToString()
		{
			return base.ToString() + "('" + Value + "')";
		}
	}

	public class ParserNodeLiteral : ParserNode
	{
		public String Text;

		override public void WriteTo(ParserNodeContext Context)
		{
			//Context.Write("Context.Output.Write(Context.AutoFilter({0}));", StringUtils.EscapeString(Text));
			Context.Write("Context.Output.Write({0});", StringUtils.EscapeString(Text));
			Context.WriteLine("");
		}

		/*
		override public void GenerateIL(ILGenerator ILGenerator, ParserNodeContext Context)
		{
			//ILGenerator.Emit(OpCodes.Call, typeof(StringUtils).GetMethod("EscapeString"));
		}
		*/

		public override string ToString()
		{
			return base.ToString() + "('" + Text + "')";
		}
	}

	public class ParserNodeParent : ParserNode
	{
		public ParserNode Parent;

		public override void Dump(int Level = 0, String Info = "")
		{
			base.Dump(Level, Info);
			Parent.Dump(Level + 1, "Parent");
		}

		override public ParserNode Optimize(ParserNodeContext Context)
		{
			var That = CreateThisInstanceAs<ParserNodeParent>();
			That.Parent = Parent.Optimize(Context);
			return That;
		}
	}

	public class ParserNodeOutputExpression : ParserNodeParent
	{
		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("Context.OutputWriteAutoFiltered(");
			Parent.WriteTo(Context);
			Context.Write(");");
			Context.WriteLine("");
		}
	}

	public class ParserNodeExtends : ParserNodeParent
	{
		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("SetAndRenderParentTemplate(");
			Parent.WriteTo(Context);
			Context.Write(", Context);");
			Context.WriteLine("");
		}
	}

	public class ParserNodeCallBlock : ParserNode
	{
		public String BlockName;

		public ParserNodeCallBlock(String BlockName)
		{
			this.BlockName = BlockName;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.WriteLine("CallBlock({0}, Context);", StringUtils.EscapeString(this.BlockName));
			Context.WriteLine("");
		}
	}

	public class ParserNodeUnaryOperation : ParserNodeParent
	{
		public String Operator;

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("{0}(", Operator);
			Parent.WriteTo(Context);
			Context.Write(")");
		}
	}

	public class ParserNodeTernaryOperation : ParserNode
	{
		public ParserNode ConditionNode;
		public ParserNode TrueNode;
		public ParserNode FalseNode;
		public String Operator;

		public ParserNodeTernaryOperation(ParserNode ConditionNode, ParserNode TrueNode, ParserNode FalseNode, String Operator)
		{
			this.ConditionNode = ConditionNode;
			this.TrueNode = TrueNode;
			this.FalseNode = FalseNode;
			this.Operator = Operator;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			switch (Operator)
			{
				case "?":
					Context.Write("DynamicUtils.ConvertToBool(");
					ConditionNode.WriteTo(Context);
					Context.Write(")");
					Context.Write("?");
					Context.Write("(");
					TrueNode.WriteTo(Context);
					Context.Write(")");
					Context.Write(":");
					Context.Write("(");
					FalseNode.WriteTo(Context);
					Context.Write(")");
					break;
				default:
					throw (new Exception(String.Format("Unknown Operator '{0}'", Operator)));
			}
		}
	}

	public class ParserNodeBlockParent : ParserNode
	{
		String BlockName;

		public ParserNodeBlockParent(String BlockName)
		{
			this.BlockName = BlockName;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.WriteLine("CallParentBlock({0}, Context);", StringUtils.EscapeString(BlockName));
		}
	}

	public class ParserNodeAccess : ParserNode
	{
		ParserNode Parent;
		ParserNode Key;

		public ParserNodeAccess(ParserNode Parent, ParserNode Key)
		{
			this.Parent = Parent;
			this.Key = Key;
		}

		override public void WriteTo(ParserNodeContext Context)
		{
			Context.Write("DynamicUtils.Access(");
			Parent.WriteTo(Context);
			Context.Write(",");
			Key.WriteTo(Context);
			Context.Write(")");
		}
	}
}
