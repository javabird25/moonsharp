﻿using System;
using System.Linq;
using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	[TestFixture]
	public class SimpleTests
	{

		[Test]
		public void CSharpStaticFunctionCallStatement()
		{
			IList<RValue> args = null;

			string script = "print(\"hello\", \"world\");";

			var globalCtx = new Table();
			globalCtx[new RValue("print")] = new RValue(new CallbackFunction((x, a) => 
			{
				args = a.ToArray(); 
				return new RValue(1234.0); 
			}));

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);

			Assert.AreEqual(DataType.Nil, res.Type);
			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
		}

		[Test]
		public void CSharpStaticFunctionCallRedef()
		{
			IList<RValue> args = null;

			string script = "local print = print; print(\"hello\", \"world\");";

			var globalCtx = new Table();
			globalCtx[new RValue("print")] = new RValue(new CallbackFunction((_x, a) => { args = a.ToArray(); return new RValue(1234.0); }));

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);

			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
			Assert.AreEqual(DataType.Nil, res.Type);
		}

		[Test]
		public void CSharpStaticFunctionCall()
		{
			IList<RValue> args = null;

			string script = "return print(\"hello\", \"world\");";

			var globalCtx = new Table();
			globalCtx[new RValue("print")] = new RValue(new CallbackFunction((_x, a) => { args = a.ToArray(); return new RValue(1234.0); }));

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);

			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234.0, res.Number);
		}

		[Test]
		public void LongStrings()
		{
			string script = @"    
				x = [[
					ciao
				]];

				y = [=[ [[uh]] ]=];

				z = [===[[==[[=[[[eheh]]=]=]]===]

				return x,y,z";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(3, res.Tuple.Length);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual(DataType.String, res.Tuple[1].Type);
			Assert.AreEqual(DataType.String, res.Tuple[2].Type);
			Assert.AreEqual("\r\n\t\t\t\t\tciao\r\n\t\t\t\t", res.Tuple[0].String);
			Assert.AreEqual(" [[uh]] ", res.Tuple[1].String);
			Assert.AreEqual("[==[[=[[[eheh]]=]=]", res.Tuple[2].String);
		}

		[Test]
		public void OperatorSimple()
		{
			string script = @"return 6*7";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(42, res.Number);
		}


		[Test]
		public void SimpleBoolShortCircuit()
		{
			string script = @"    
				x = true or crash();
				y = false and crash();
			";

			var globalCtx = new Table();
			globalCtx[new RValue("crash")] = new RValue(new CallbackFunction((_x, a) =>
			{
				throw new Exception("FAIL!");
			}));

			MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);
		}


		[Test]
		public void BoolConversionAndShortCircuit()
		{
			string script = @"    
				i = 0;

				function f()
					i = i + 1;
					return '!';
				end					
				
				x = false;
				y = true;

				return false or f(), true or f(), false and f(), true and f(), i";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(5, res.Tuple.Length);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Boolean, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Boolean, res.Tuple[2].Type);
			Assert.AreEqual(DataType.String, res.Tuple[3].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[4].Type);
			Assert.AreEqual("!", res.Tuple[0].String);
			Assert.AreEqual(true, res.Tuple[1].Boolean);
			Assert.AreEqual(false, res.Tuple[2].Boolean);
			Assert.AreEqual("!", res.Tuple[3].String);
			Assert.AreEqual(2, res.Tuple[4].Number);
		}
		[Test]
		public void HanoiTowersDontCrash()
		{
			string script = @"
			function move(n, src, dst, via)
				if n > 0 then
					move(n - 1, src, via, dst)
					move(n - 1, via, dst, src)
				end
			end
 
			move(4, 1, 2, 3)
			";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);
		}

		[Test]
		public void Factorial()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					if (n == 0) then
						return 1
					else
						return n*fact(n - 1)
					end
				end
    
				return fact(5)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]
		public void IfStatmWithScopeCheck()
		{
			string script = @"    
				x = 0

				if (x == 0) then
					local i = 3;
					x = i * 2;
				end
    
				return i, x";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void ScopeBlockCheck()
		{
			string script = @"    
				local x = 6;
				
				do
					local i = 33;
				end
		
				return i, x";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void ForLoopWithBreak()
		{
			string script = @"    
				x = 0

				for i = 1, 10 do
					x = i
					break;
				end
    
				return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}


		[Test]
		public void ForEachLoopWithBreak()
		{
			string script = @"    
				x = 0
				y = 0

				t = { 2, 4, 6, 8, 10, 12 };

				function iter (a, ii)
				  ii = ii + 1
				  local v = a[ii]
				  if v then
					return ii, v
				  end
				end
    
				function ipairslua (a)
				  return iter, a, 0
				end

				for i,j in ipairslua(t) do
					x = x + i
					y = y + j

					if (i >= 3) then
						break
					end
				end
    
				return x, y";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[0].Number);
			Assert.AreEqual(12, res.Tuple[1].Number);
		}


		[Test]
		public void ForEachLoop()
		{
			string script = @"    
				x = 0
				y = 0

				t = { 2, 4, 6, 8, 10, 12 };

				function iter (a, ii)
				  ii = ii + 1
				  local v = a[ii]
				  if v then
					return ii, v
				  end
				end
    
				function ipairslua (a)
				  return iter, a, 0
				end

				for i,j in ipairslua(t) do
					x = x + i
					y = y + j
				end
    
				return x, y";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(21, res.Tuple[0].Number);
			Assert.AreEqual(42, res.Tuple[1].Number);
		}

		[Test]
		public void LengthOperator()
		{
			string script = @"    
				x = 'ciao'
				y = { 1, 2, 3 }
   
				return #x, #y";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(4, res.Tuple[0].Number);
			Assert.AreEqual(3, res.Tuple[1].Number);
		}


		[Test]
		public void ForLoopWithBreakAndScopeCheck()
		{
			string script = @"    
				x = 0

				for i = 1, 10 do
					x = x + i

					if (i == 3) then
						break
					end
				end
    
				return i, x";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void FactorialWithOneReturn()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					if (n == 0) then
						return 1
					end
					return n*fact(n - 1)
				end
    
				return fact(5)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]
		public void OperatorPrecedenceAndAssociativity()
		{
			string script = @"return 5+3*7-2*5+2^3^2";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(528, res.Number);
		}

		[Test]
		public void OperatorParenthesis()
		{
			string script = @"return (5+3)*7-2*5+(2^3)^2";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(110, res.Number);
		}

		[Test]
		public void GlobalVarAssignment()
		{
			string script = @"x = 1; return x;";    

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}
		[Test]
		public void TupleAssignment1()
		{
			string script = @"    
				function y()
					return 2, 3
				end

				function x()
					return 1, y()
				end

				w, x, y, z = 0, x()
    
				return w+x+y+z";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void IterativeFactorialWithWhile()
		{
			string script = @"    
				function fact (n)
					local result = 1;
					while(n > 0) do
						result = result * n;
						n = n - 1;
					end
					return result;
				end
    
				return fact(5)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}



		[Test]
		public void IterativeFactorialWithRepeatUntilAndScopeCheck()
		{
			string script = @"    
				function fact (n)
					local result = 1;
					repeat
						local checkscope = 1;
						result = result * n;
						n = n - 1;
					until (n == 0 and checkscope == 1)
					return result;
				end
    
				return fact(5)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]

		public void SimpleForLoop()
		{
			string script = @"    
					x = 0
					for i = 1, 3 do
						x = x + i;
					end

					return x;
			";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6.0, res.Number);
		}

		[Test]
		public void SimpleFunc()
		{
			string script = @"    
				function fact (n)
					return 3;
				end
    
				return fact(3)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		public void IterativeFactorialWithFor()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					x = 1
					for i = n, 1, -1 do
						x = x * i;
					end

					return x;
				end
    
				return fact(5)";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}


		[Test]
		public void LocalFunctionsObscureScopeRule()
		{
			string script = @"    
				local function fact()
					return fact;
				end

				return fact();
				";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Function, res.Type);
		}

		[Test]
		public void FunctionWithStringArg()
		{
			string script = @"    
				x = 0;

				function fact(y)
					x = y
				end

				fact 'ciao';

				return x;
				";


			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciao", res.String);

		}

		[Test]
		public void FunctionWithTableArg()
		{
			string script = @"    
				x = 0;

				function fact(y)
					x = y
				end

				fact { 1,2,3 };

				return x;
				";


			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Table, res.Type);

		}


		[Test]
		public void TupleAssignment2()
		{
			string script = @"    
				function boh()
					return 1, 2;
				end

				x,y,z = boh(), boh()

				return x,y,z;
				";


			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[2].Type);
			Assert.AreEqual(1, res.Tuple[0].Number);
			Assert.AreEqual(1, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
		}
		[Test]
		public void LoopWithReturn()
		{
			string script = @"function Allowed( )
									for i = 1, 20 do
  										return false 
									end
									return true
								end
						Allowed();
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

		}
		[Test]
		public void IfWithLongExpr()
		{
			string script = @"function Allowed( )
									for i = 1, 20 do
									if ( false ) or ( true and true ) or ( 7+i <= 9 and false ) then 
  										return false 
									end
									end		
									return true
								end
						Allowed();
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

		}

		[Test]
		public void IfWithLongExprTbl()
		{
			string script = @"
						t = { {}, {} }
						
						function Allowed( )
									for i = 1, 20 do
									if ( t[1][3] ) or ( i <= 17 and t[1][1] ) or ( 7+i <= 9 and t[1][1] ) then 
  										return false 
									end
									end		
									return true
								end
						Allowed();
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

		}

		[Test]
		public void ExpressionReducesTuples()
		{
			string script = @"
					function x()
						return 1,2
					end

					do return (x()); end
					do return x(); end
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Number, res);
			Assert.AreEqual(1, res.Number);
		}

		[Test]
		public void ExpressionReducesTuples2()
		{
			string script = @"
					function x()
						return 3,4
					end

					return 1,x(),x()
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(DataType.Tuple, res);
			Assert.AreEqual(4, res.Tuple.Length);
		}


		[Test]
		public void ArgsDoNotChange()
		{
			string script = @"
					local a = 1;
					local b = 2;

					function x(c, d)
						c = c + 3;
						d = d + 4;
						return c + d;
					end

					return x(a, b+1), a, b;
								";

			RValue res = MoonSharpInterpreter.LoadFromString(script).Execute(null);

			Assert.AreEqual(3, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[2].Type);
			Assert.AreEqual(11, res.Tuple[0].Number);
			Assert.AreEqual(1, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
		}

	}
}