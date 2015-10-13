using System;
using ProgrammingLanguageNr1;
using TingTing;
using GameTypes;
using System.Collections.Generic;
using System.Linq;

namespace GameWorld2
{
	public class ConnectionAPI_Optimized
	{
		private TingRunner _tingRunner;
		private MimanTing _caller;
		private Program _program;
		
		// OBS!!! With this solution only one program in the Ting can use the API safely!!!
		
		public ConnectionAPI_Optimized(MimanTing pCaller, TingRunner pTingRunner, Program pProgram) {
			_caller = pCaller;
			_tingRunner = pTingRunner;
			_program = pProgram;
		}

		public object DisconnectAll (object[] args)
		{
			if(_caller != null) {
				_caller.connectedTings = new MimanTing[] {};
			}
			return VoidType.voidType;
		}

		public static MimanTing GetTingFromNameOrSourceCodeName(TingRunner pTingRunner, string pName) {
			//Console.WriteLine("Will get ting with name " + pName);

			var mimanTing = pTingRunner.GetTingUnsafe(pName) as MimanTing;

			if(mimanTing != null) {
				return mimanTing;
			}

			// now try to get it through program name
			foreach(var ting in pTingRunner.GetTings()) {
				var mimanTingWithProgram = (ting as MimanTing);

				if(mimanTingWithProgram.masterProgram == null) {
					continue;
				}

				if(mimanTingWithProgram.masterProgram.sourceCodeName == pName) {
					return mimanTingWithProgram;
				}
//				else {
//					Console.WriteLine("Discarding miman ting with sourceCodeName " + mimanTingWithProgram.masterProgram.sourceCodeName);
//				}
			}

			return null;
		}

		public object Connect(object[] args)
		{
			string name = ReturnValueConversions.SafeUnwrap<string>(args, 0);

			//Console.WriteLine(_caller + " connecting to " + name);
			MimanTing maybeTing = GetTingFromNameOrSourceCodeName(_tingRunner, name);

//			if (maybeTing is Character) {
//				_caller.Say("Can't connect to human " + name);
//				return new ReturnValue(-1);
//			} else 
			if(maybeTing != null) {
				//_caller.PlaySound ("Modem");
				return _caller.AddConnectionToTing (maybeTing);
			} else {
				if (_caller is Computer) {
					(_caller as Computer).API_Print ("Can't find " + name + " to connect to");
				} else {
					_caller.Say ("Can't find " + name + " to connect to", "");
				}
				return -1f;
			}
		}
		
		public object RemoteFunctionCall(object[] args)
		{
			float receiverIndex = (int)ReturnValueConversions.SafeUnwrap<float>(args, 0);
			string functionName = ReturnValueConversions.SafeUnwrap<string>(args, 1);

			if (args [2].GetType() != typeof(SortedDictionary<KeyWrapper,object>)) {
				throw new Error ("Argument array to " + functionName + " is not an array");
			}

			var argsAsReturnValues = ReturnValueConversions.SafeUnwrap<SortedDictionary<KeyWrapper,object>>(args, 2);

			if(_caller.connectedTings.Length == 0) {
				_caller.Say("No connected object to call function on", "");
				return 0f;
			}

			if (receiverIndex == -1) {
				_caller.Say("Connection not open, can't call function.", "");
				return 0f;
			}
			else if (receiverIndex < 0) {
				_caller.Say("Receiver index is below 0: " + receiverIndex, "");
				return 0f;
			}
			else if (receiverIndex >= _caller.connectedTings.Length) {
				_caller.Say("Receiver index is too big (" + _caller.connectedTings.Length + ")", "");
				return 0f;
			}
			
			MimanTing receiverTing = _caller.connectedTings[(int)receiverIndex];
			
			receiverTing.PrepareForBeingHacked();
			
			if(receiverTing.programs.Length == 0) {
				_caller.Say("Connected thing has no programs to call", "");
				return 0f;
			}

			var fixedArgs = argsAsReturnValues.Values.ToArray();

			receiverTing.masterProgram.StartAtFunctionIfItExists (functionName, fixedArgs, _program);				
			_program.waitingForInput = true;
			
			return 0f; // "WAITING FOR RETURN VALUE FROM REMOTE FUNCTION";
		}
	}

	public class ConnectionAPI
	{
		private TingRunner _tingRunner;
		private MimanTing _caller;
		private Program _program;
		
		// OBS!!! With this solution only one program in the Ting can use the API safely!!!
		
		public ConnectionAPI(MimanTing pCaller, TingRunner pTingRunner, Program pProgram) {
			_caller = pCaller;
			_tingRunner = pTingRunner;
			_program = pProgram;
		}

		[SprakAPI("Remove all connections", "")]
		public void API_DisconnectAll()
		{
			_caller.connectedTings = new MimanTing[] {};
		}
		
		[SprakAPI("Connect to something", "name")]
		public float API_Connect(string name)
		{
			//Console.WriteLine(_caller + " connecting to " + name);
			MimanTing maybeTing = ConnectionAPI_Optimized.GetTingFromNameOrSourceCodeName(_tingRunner, name); // _tingRunner.GetTingUnsafe (name) as MimanTing;

			if(maybeTing != null) {
				return _caller.AddConnectionToTing (maybeTing);
			} else {
				string msg = "Can't find " + name + " to connect to";
				if(_caller is Computer) {
					(_caller as Computer).API_Print(msg);
				} else {
					_caller.Say(msg, "");
				}
				return -1f;
			}
		}
		
		private int RemoteFunctionCall(float receiverIndex, string functionName, object [] arguments)
		{
			var argsAsReturnValues = new object[arguments.Length];
			
			int i = 0;
			foreach(object o in arguments)
			{
				if(o is float) {
					argsAsReturnValues[i] = (float)o;
				}
				else if(o is string) {
					argsAsReturnValues[i] = (string)o;
				}
				else if(o is bool) {
					argsAsReturnValues[i] = (bool)o;
				}
				else {
					throw new Exception("Can't handle argument to remote function call.");	
				}
				i++;
			}
			
			if(_caller.connectedTings.Length == 0) {
				_caller.Say("No connected object to call function on", "");
				return 0;
			}

			if (receiverIndex == -1) {
				_caller.Say("Connection not open, can't call function.", "");
				return 0;
			}
			else if (receiverIndex < 0) {
				_caller.Say("Receiver index is below 0: " + receiverIndex, "");
				return 0;
			}
			else if (receiverIndex >= _caller.connectedTings.Length) {
				_caller.Say("Receiver index is too big (" + _caller.connectedTings.Length + ")", "");
				return 0;
			}

			MimanTing receiverTing = _caller.connectedTings[(int)receiverIndex];
			
			receiverTing.PrepareForBeingHacked();
			
			if(receiverTing.programs.Length == 0) {
				_caller.Say("Connected thing has no programs to call", "");
				return 0;
			}

			//Program.OnProgramEndsReturnValue onReturnHandler = OnReturnValue;

			receiverTing.masterProgram.StartAtFunctionIfItExists (functionName, argsAsReturnValues, _program);				
			_program.waitingForInput = true;
			
			return 0; // "WAITING FOR RETURN VALUE FROM REMOTE FUNCTION";
		}
		
		[SprakAPI("Call remote function on connected objects", "function name", "arguments")]
		public int API_RemoteFunctionCall(float receiverIndex, string functionName, object[] arguments)
		{
			return RemoteFunctionCall(receiverIndex, functionName, arguments);			
		}
	}
}

