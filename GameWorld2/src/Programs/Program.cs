//#define LOG

using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using System.Text;
using System.IO;
using RelayLib;
using GameTypes;
using TingTing;

namespace GameWorld2
{
	public interface IReturnValueReceiver {
		void OnReturnValue (object pReturnValue);
	}
	
	public class Program : RelayObjectTwo, IReturnValueReceiver
	{
		public const string TABLE_NAME = "Programs";
		
		public Logger logger = new Logger();
		
		private SprakRunner _sprakRunner;
		private ProgramRunner _programRunner;
		private List<FunctionDefinition> _functionDefinitions = new List<FunctionDefinition>();
		private List<VariableDefinition> _variableDefinitions = new List<VariableDefinition>();
		
		private ValueEntry<string> CELL_name;
		private ValueEntry<string> CELL_sourceCodeName;
		private ValueEntry<bool> CELL_isOn;
		private ValueEntry<string> CELL_sourceCodeContent;
		private ValueEntry<float> CELL_sleepTimer;
		private ValueEntry<int> CELL_remoteCaller;
		private ValueEntry<bool> CELL_waitingForInput;
		private ValueEntry<int> CELL_executionCounter;
		private ValueEntry<int> CELL_executionsPerFrame;
		private ValueEntry<bool> CELL_compilationTurnedOn;
		private ValueEntry<float> CELL_executionTime;
		private ValueEntry<float> CELL_maxExecutionTime;
		
		bool _isOn_Cache;
		
		public bool waitForNextFrame; // used by graphical programs to not do more than necessary for one frame
		
		private MockProgram _mockProgram; // used by the hackdev (and others?) to be a receiver for return values from program
		
		int uniqueCompilationId = 0;
		int callersUniqueCompilationId = -1;
		
		public string nameOfOwner = "unknown"; // Just for debugging
		
		protected override void SetupCells()
		{
			CELL_sourceCodeName = EnsureCell("sourceCodeName", "");
			CELL_isOn = EnsureCell("isOn", false);
			CELL_name = EnsureCell("name", "undefined");
			CELL_sourceCodeContent = EnsureCell("sourceCode", "");
			CELL_sleepTimer = EnsureCell ("sleepTimer", 0f);
			CELL_remoteCaller = EnsureCell("remoteCaller", -1);
			CELL_waitingForInput = EnsureCell("waitingForInput", false);
			CELL_executionCounter = EnsureCell("executionCounter", 0);
			CELL_executionsPerFrame = EnsureCell("executionsPerFrame", 50);
			CELL_compilationTurnedOn = EnsureCell("compilationTurnedOn", true);
			
			CELL_executionTime = EnsureCell("executionTime", 0f);
			CELL_maxExecutionTime = EnsureCell("maxExecutionTime", -1f);
			
			_isOn_Cache = CELL_isOn.data;
		}
		
		public void Init(ProgramRunner pProgramRunner) {
			_programRunner = pProgramRunner;
		}
		
		public void StopAndReset()
		{
			uniqueCompilationId++;
			isOn = false;
			sleepTimer = 0f;
			remoteCaller = null;
			_mockProgram = null;
			waitingForInput = false;
			executionCounter = 0;
			executionTime = 0;

			if(_sprakRunner != null) {
				_sprakRunner.returnFromExternalFunctionCall = false;
			}
		}

		public void ClearErrors()
		{
			if (_sprakRunner != null) {
				_sprakRunner.getCompileTimeErrorHandler ().getErrors ().Clear ();
				_sprakRunner.getRuntimeErrorHandler ().getErrors ().Clear ();
			}
		}
		
		public void ClearRuntimeErrors() {
			if (_sprakRunner != null) {
				_sprakRunner.getRuntimeErrorHandler ().getErrors ().Clear ();
			}
		}
		
		public Error[] Compile()
		{
			if(!compilationTurnedOn) {
				return new Error[] {
					new Error("Uncompiled program.")
				};
			}

			//D.Log("Compiling " + this.ToString());
			
			StopAndReset();
			
			//_sprakRunner = new SprakRunner(new StringReader(sourceCodeContent), FunctionDefinitions.ToArray(), VariableDefinitions.ToArray());
			
			if(_sprakRunner == null) {
				//D.Log("Creating new SprakRunner for " + this.ToString());
				_sprakRunner = new SprakRunner(new StringReader(sourceCodeContent), FunctionDefinitions.ToArray(), VariableDefinitions.ToArray());
			}
			else {
				_sprakRunner.Reset();
			}
			
			PrintErrorsToD();
			return GetErrors();
		}
		
		private void EnsureSprakRunner()
		{
			if (_sprakRunner == null) {
				Compile();
			}
		}
		
		public SprakRunner sprakRunner {
			get {
				return _sprakRunner;
			}
		}
		
		public Dictionary<string, ProfileData> GetProfileData ()
		{
			return _sprakRunner.GetProfileData ();
		}
		
		public void Start()
		{
			//          D.Log ("uniqueExecutionId of " + ToString () + " bumped up to " + uniqueCompilationId);
			Compile();
			isOn = true;
			//          D.Log("Program " + this.ToString() + " set to execute at TOP LEVEL, waitingForInput: " + waitingForInput);
			//          System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			//          D.Log("Stacktrace: " + t.ToString());
		}
		
		public void StartAtFunction(string functionName, object[] args, Program pCaller)
		{
			StartAtFunction (functionName, args, pCaller, true);
		}
		
		public void StartAtFunctionIfItExists(string functionName, object[] args, Program pCaller)
		{
			StartAtFunction (functionName, args, pCaller, false);
		}
		
		public void StartAtFunction(string functionName, object[] args, Program pCaller, bool pEnsureThatFunctionExists) 
		{
			if(_sprakRunner == null) {
				#if LOG_START_AT_FUNCTION
				D.Log("Calling remote function " + functionName + " on " + nameOfOwner + " but it's sprakrunner is null, will try to compile");
				#endif
				var errors = Compile();
				
				if(_sprakRunner == null) {
					if(pCaller != null) {
						#if LOG_START_AT_FUNCTION
						D.Log("Sprak runner is still null, will send back " + errors.Length + " errors to caller:");
						#endif
						foreach(var e in errors) {
							pCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
							D.Log(" - " + e);
						}
					} else {
						#if LOG_START_AT_FUNCTION
						D.Log("Sprak runner is still null, and no caller");
						#endif
					}
					D.Log("Will NOT run " + functionName + " on " + nameOfOwner + ", failed to create sprak runner second time");
					return;
				}
			}
			else {
				if(ContainsErrors()) {
					D.Log("Clearing errors (" + GetErrors().Length + ") on " + this.ToString() + " because of incoming function call " + functionName);
					ClearErrors(); // <- TODO: trying this out now
				}
			}
			
			//          var errors = Compile();
			//          
			//          if(_sprakRunner == null || _sprakRunner.interpreter == null) {
			//              
			//              if(pCaller != null) {
			//                  #if LOG_START_AT_FUNCTION
			//                  D.Log("Sprak runner is still null, will send back " + errors.Length + " errors to caller:");
			//                  #endif
			//                  foreach(var e in errors) {
			//                      pCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
			//                      D.Log(" - " + e);
			//                  }
			//              } else {
			//                  #if LOG_START_AT_FUNCTION
			//                  D.Log("Sprak runner is still null, and no caller");
			//                  #endif
			//              }
			//              
			//              D.Log("Will NOT run " + functionName + " on " + nameOfOwner + ", failed to create sprak runner second time");
			//              return;
			//          }
			
			try {
				remoteCaller = pCaller;
				_mockProgram = null;
				
				if(remoteCaller != null) {
					//D.Log("Set caller of " + this.ToString() + " to " + remoteCaller);
					callersUniqueCompilationId = pCaller.uniqueCompilationId;
					//D.Log("Set callersUniqueCompilationId of " + this.ToString() + " to the caller " + pCaller.ToString() + "'s uniqueCompilationId " + callersUniqueCompilationId);
				}
				else {
					//D.Log("Program " + this.ToString() + " has no caller");
				}

				var functionCallStatus = _sprakRunner.ResetAtFunction (functionName, args);
				
				if(functionCallStatus == InterpreterTwo.ProgramFunctionCallStatus.NO_FUNCTION) {
					var e = new Error("Can't find function '" + functionName + "' (forgot quotes?)");
					if(pEnsureThatFunctionExists) {
						throw e;
					} else {
						//D.Log("Can't find optional function '" + functionName + "' in " + this.ToString());
						if(remoteCaller != null) {
							remoteCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
						}
						StopAndReset();
						return;
					}
				}
				else if(functionCallStatus == InterpreterTwo.ProgramFunctionCallStatus.NORMAL_FUNCTION) {
					isOn = true;
				}
				else if(functionCallStatus == InterpreterTwo.ProgramFunctionCallStatus.EXTERNAL_FUNCTION) {
					isOn = true; // turning it on but it will only return it's value and then end
					sprakRunner.returnFromExternalFunctionCall = true;
				}
				
				waitingForInput = false; // TODO: Is this an OK hack, why didn't I do this before?!!
				
				#if LOG
				D.Log("Program " + this.ToString() + " set to execute function " + functionName + ", waitingForInput: " + waitingForInput);
				#endif
			}
			catch(Error e) {
				D.Log("Error when trying to call function: " + e + " of type " + e.getErrorType());
				if (remoteCaller != null && e.getErrorType() != Error.ErrorType.RUNTIME) {
					D.Log("Logging error on only remote caller: " + remoteCaller);
					remoteCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
				}
				else {
					D.Log("Logging error on self: " + this + " and remote caller " + remoteCaller);
					
					if(remoteCaller != null && remoteCaller._sprakRunner != null && remoteCaller._sprakRunner.getRuntimeErrorHandler() != null) {
						remoteCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
					}
					
					if(_sprakRunner == null) {
						Compile();
					}
					if(_sprakRunner != null) {
						_sprakRunner.getRuntimeErrorHandler().errorOccured (e);
					} else {
						D.Log("Sprak runner in " + this.ToString() + " still null, when trying to add runtime error: " + e);
					}
				}
			}
		}
		
		public void StartAtFunctionWithMockReceiver(string functionName, object[] args, MockProgram pMockProgram) 
		{
			Start();
			
			try {
				remoteCaller = null;
				_mockProgram = pMockProgram;
				_sprakRunner.ResetAtFunction (functionName, args);
			}
			catch(Error e) {
				D.Log("Error when trying to call function using mock receiver: " + e);
				if (remoteCaller != null) {
					remoteCaller._sprakRunner.getRuntimeErrorHandler().errorOccured (e);
				}
				_sprakRunner.getRuntimeErrorHandler().errorOccured (e);
			}
		}
		
		public void Update(float dt)
		{
			executionTime += dt;
			if (maxExecutionTime > 0.0f && executionTime >= maxExecutionTime) {
				#if LOG
				#endif
				//D.Log("Stopping program " + this.ToString() + " because max execution time was reached (" + maxExecutionTime + ")");
				StopAndReset();
				return;
			}
			
			if(ContainsErrors()) {
				#if LOG
				D.Log("Program " + this.ToString() + " contains broken programs, will not update it");
				#endif
				return;
			}
			
			if (waitingForInput) {
				#if LOG
				//D.Log("Program " + this.ToString() + " is waiting for input, will not update it");
				#endif
				return;
			}
			
			if (sleepTimer > 0f) {
				#if LOG
				D.Log("Program " + this.ToString() + " is sleeping, will not update it");
				#endif
				sleepTimer -= dt;
				return;
			}
			
			int nrOfExecutions = executionsPerFrame;
			Execute(nrOfExecutions);
			executionCounter += nrOfExecutions;
		}
		
		private void Execute(int pExecutions)
		{
			if(!compilationTurnedOn) {
				return;
			}
			
			#if LOG
			D.Log("Time to execute program " + this.ToString() + " " + pExecutions + " steps, returnFromExternalFunctionCall: " + _sprakRunner.returnFromExternalFunctionCall);
			#endif
			
			//System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			//D.Log("Stacktrace: " + t.ToString());
			
			if (!_sprakRunner.isStarted)
			{
				bool success = _sprakRunner.Start();
				if (!success) {
					D.Log(this.ToString() + " failed to start, will not execute");
					//StopAndReset(); // TODO: this is new
					PrintErrorsToD();
					return;
				}
#if LOG
				else {
					D.Log(this + " Started!");
				}
#endif
			}
			
			for(int i = 0; i < pExecutions; i++)
			{
				InterpreterTwo.Status s = _sprakRunner.Step();
				
				if (sleepTimer > 0f) {
					#if LOG
					D.Log("Program " + this.ToString() + " starts sleeping");
					#endif
					break;
				}
				
				if (waitingForInput) {
					#if LOG
					D.Log("Program " + this.ToString() + " starts waiting for input");
					#endif
					break;
				}
				
				if (waitForNextFrame) {
					#if LOG
					D.Log("Program " + this.ToString() + " starts waiting for next frame");
					#endif
					waitForNextFrame = false;
					break;
				}
				
				if (s == InterpreterTwo.Status.FINISHED || _sprakRunner.returnFromExternalFunctionCall) {
					#if LOG
					D.Log("Execution of program " + this.ToString() + " finished");
					#endif
					
					if (remoteCaller != null) {
						if (remoteCaller.uniqueCompilationId != callersUniqueCompilationId) {
							D.Log ("The uniqueExecutionId of " + this.ToString() + " has changed after it called the remote function on " + remoteCaller.ToString());
						} else {
							var retVal = _sprakRunner.GetFinalReturnValue();
							#if LOG
							D.Log("Has remoteCaller, retVal: " + retVal + " of type " + retVal.GetType());
							#endif
							remoteCaller.OnReturnValue (retVal);
						}
					}
					else if(_mockProgram != null) {
						var retVal = _sprakRunner.GetFinalReturnValue ();
						#if LOG
						D.Log("Has mock remoteCaller, retVal: " + retVal);
						#endif
						_mockProgram.OnReturnValue (retVal);
					} 
					else {
						#if LOG
						D.Log(this.ToString() + " has no remoteCaller, this function has no listener");
						#endif
					}
					
					StopAndReset();
					break;
				}
				else if (s == InterpreterTwo.Status.ERROR) {
					isOn = false;
					//StopAndReset(); // TODO: this is new
					PrintErrorsToD();                   
					break;
				}
			}
		}
		
		public void OnReturnValue(object pReturnValue) {
			#if LOG
			D.Log (this.ToString() + " got return value from remote function call: " + pReturnValue);
			#endif
			try {
				SwapStackTopValueTo (pReturnValue);
			}
			catch(Error e) {
				#if LOG
				D.Log("Error when swapping stack top value in " + this.ToString() + ": " + e);
				#endif
				_sprakRunner.getRuntimeErrorHandler().errorOccured(e);
			}
			finally {
				waitingForInput = false;
			}
		}
		
		void PrintErrorsToD()
		{
			foreach(Error e in GetErrors()) {
				D.Log("Error in program '" + this.ToString() + "': " + e.Message);
			}
		}
		
		#region FIDDLE WITH THE RUNNER
		
		public List<FunctionDefinition> FunctionDefinitions
		{
			get { return _functionDefinitions; }
			set { _functionDefinitions = value; }
		}
		
		public bool HasFunction(string pFunctionName, bool pFullExpensiveCheck)
		{
			//D.Log("Looking for function " + pFunctionName + " in " + name);
			
			if(pFullExpensiveCheck) {
				if(_sprakRunner == null) {
					Compile();
					if(_sprakRunner == null) {
						D.Log("Failed to Compile() when calling HasFunction with function name " + pFunctionName);
						return false;
					}
				}
				else {
					Compile();
				}
				
				if(_sprakRunner.HasFunction(pFunctionName)) {
					//D.Log("Was user defined");
					return true;
				}
			}
			
			//D.Log(" ting specific: ");
			foreach (FunctionDefinition f in _functionDefinitions) {
				//D.Log(f.functionName);
				if (f.functionName == pFunctionName) {
					return true;
				}
			}
			
			//D.Log(" built in: ");
			foreach(FunctionDefinition f in SprakRunner.builtInFunctions ) {
				//D.Log(f.functionName);
				if (f.functionName == pFunctionName) {
					return true;
				}
			}
			
			return false;
		}
		
		public bool TryGetFunctionDefinition(string pFunctionName, out FunctionDefinition pOutput)
		{
			foreach (FunctionDefinition f in _functionDefinitions)
				if (f.functionName == pFunctionName)
			{
				pOutput = f;
				return true;
			}
			foreach (FunctionDefinition f in SprakRunner.builtInFunctions)
				if (f.functionName == pFunctionName)
			{
				pOutput = f;
				return true;
			}
			pOutput = new FunctionDefinition();
			return false;
		}
		
		public List<VariableDefinition> VariableDefinitions
		{
			get { return _variableDefinitions; }
			set { _variableDefinitions = value; }
		}
		
		public void SwapStackTopValueTo(object pValue)
		{
			if(_sprakRunner == null) {
				D.Log("Sprak runner is null");
				return;
			}
			else if(pValue == null) {
				D.Log("pValue is null, won't swap stack top value");
				return;
			}
			_sprakRunner.SwapStackTopValueTo(pValue);
		}
		
		public void ChangeGlobalVariableInitValue(string pName, object pValue) 
		{
			_sprakRunner.ChangeGlobalVariableInitValue(pName, pValue);
		}
		
		public object GetGlobalVariableValue(string pName) 
		{
			return _sprakRunner.GetGlobalVariableValue(pName);
		}
		
		#endregion
		
		#region ACCESSORS
		
		public bool isOn {
			get {
				return _isOn_Cache;
				//return CELL_isOn.data;
			}
			private set {
				//              if(value == false && _isOn_Cache == true && _sprakRunner != null && _sprakRunner.interpreter != null) {
				//                  D.Log("Detected that sprak runner is not cleared for " + this + " when stopping it, will fix that.");
				//                  _sprakRunner.HardReset();
				//              }
				
				_isOn_Cache = value;
				CELL_isOn.data = value;
			}
		}
		
		public string name {
			get {
				return CELL_name.data;
			}
			set {
				CELL_name.data = value;
			}
		}
		
		public string sourceCodeName {
			get {
				return CELL_sourceCodeName.data;
			}
			set {
				CELL_sourceCodeName.data = value;
			}
		}
		
		public float sleepTimer {
			get {
				return CELL_sleepTimer.data;
			}
			set {
				CELL_sleepTimer.data = value;
			}
		}
		
		public bool waitingForInput {
			get {
				return CELL_waitingForInput.data;
			}
			set {
				//              System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
				//              D.Log(this.ToString() + " waiting for input: " + value + ", Stacktrace: " + t.ToString());
				CELL_waitingForInput.data = value;
			}
		}
		
		public bool compilationTurnedOn {
			get {
				return CELL_compilationTurnedOn.data;
			}
			set {
				CELL_compilationTurnedOn.data = value;
			}
		}
		
		public int executionCounter {
			get {
				return CELL_executionCounter.data;
			}
			set {
				CELL_executionCounter.data = value;
			}
		}
		
		public string sourceCodeContent {
			get {
				return CELL_sourceCodeContent.data;
			}
			set {
				//D.Log ("Source code in " + ToString() + " set to: " + value);
				
				CELL_sourceCodeContent.data = value;
				
				DeleteSprakRunner ();
			}
		}
		
		void DeleteSprakRunner ()
		{
			if (_sprakRunner != null) {
				_sprakRunner.HardReset ();
			}
			_sprakRunner = null;
		}
		
		public Program remoteCaller {
			get {
				return _programRunner.GetProgramUnsafe(CELL_remoteCaller.data);
			}
			set {
				if (value == null) {
					CELL_remoteCaller.data = -1;
				} else {
					CELL_remoteCaller.data = value.objectId;
				}
			}
		}
		
		public Error[] GetErrors()
		{
			if(!compilationTurnedOn) {
				return new Error[] {};
			}
			
			EnsureSprakRunner();
			List<Error> l = new List<Error>();
			//if(_sprakRunner != null) {
			l.AddRange(_sprakRunner.getRuntimeErrorHandler().getErrors());
			l.AddRange(_sprakRunner.getCompileTimeErrorHandler().getErrors());
			//}
			return l.ToArray();
		}
		
		public bool ContainsErrors()
		{
			if(!compilationTurnedOn) {
				return false;
			}
			
			EnsureSprakRunner();
			return _sprakRunner.getRuntimeErrorHandler().getErrors().Count > 0 || _sprakRunner.getCompileTimeErrorHandler().getErrors().Count > 0;
		}
		
		public override string ToString()
		{
			return name + "/" + sourceCodeName + "/" + nameOfOwner;
		}
		
		public int executionsPerFrame {
			get {
				return CELL_executionsPerFrame.data;
			}
			set {
				CELL_executionsPerFrame.data = value;
			}
		}
		
		public float executionTime {
			get {
				return CELL_executionTime.data;
			}
			set {
				CELL_executionTime.data = value;
			}
		}
		
		public float maxExecutionTime {
			get {
				return CELL_maxExecutionTime.data;
			}
			set {
				CELL_maxExecutionTime.data = value;
			}
		}
		
		#endregion
	}
	
	public class MockProgram : IReturnValueReceiver
	{
		public delegate void OnReturnValueDelegate(object pReturnValue);
		public OnReturnValueDelegate onReturnValue;
		
		public MockProgram(OnReturnValueDelegate pOnReturnValue) {
			onReturnValue = pOnReturnValue;
		}
		
		public void OnReturnValue(object pReturnValue) {
			if (onReturnValue != null) {
				onReturnValue (pReturnValue);
			}
		}
	}
	
}


