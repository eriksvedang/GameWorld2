using System;
using RelayLib;
using GameTypes;
using TingTing;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using System.Text;

namespace GameWorld2
{
	public class CreditCard : MimanTing, TingWithButton
	{
		ValueEntry<string> CELL_nameOfOwner;
		ValueEntry<string> CELL_programName;

		Program _program;

		protected override void SetupCells ()
		{
			base.SetupCells ();
			CELL_nameOfOwner = EnsureCell("nameOfOwner", "");
			CELL_programName = EnsureCell("masterProgramName", "PersonalCreditCard");
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		[EditableInEditor]
		public string nameOfOwner {
			get {
				return CELL_nameOfOwner.data;
			}
			set {
				CELL_nameOfOwner.data = value;
			}
		}

		public override bool canBePickedUp {
			get {
				return true;
			}
		}

		public override string tooltipName {
			get {
				return "credit card";
			}
		}

		public override string verbDescription {
			get {
				return "check balance";
			}
		}

		[EditableInEditor]
		public string masterProgramName {
			get {
				return CELL_programName.data;
			}
			set {
				CELL_programName.data = value;
			}
		}

		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(CreditCard)));
					functionDefs.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = functionDefs;
				}
				return _program;
			}
		}

		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}

		public void PushButton(Ting pUser) {
			logger.Log (name + " is getting used in hand");

			var user = (pUser as Character);

			if(user != null && user.creditCardUsageAmount != 0f) {
				RunMakeTransactionFunction(user.creditCardUsageAmount);
				user.creditCardUsageAmount = 0f;
			}
			else {
				masterProgram.maxExecutionTime = 10f;
				masterProgram.StartAtFunction ("CheckBalance", new object[] {}, null);
			}
		}

		public override void InteractWith (Ting pTingToInteractWith)
		{

		}

		public void RunMakeTransactionFunction(float transactionAmount) {
//			var mock = new MockProgram(val => {
//				D.Log("Return value from MakeTransaction: " + val.ToString());
//			});
			masterProgram.maxExecutionTime = 10f;
			masterProgram.StartAtFunctionWithMockReceiver ("MakeTransaction", new object[] { transactionAmount }, null);
		}

		// TODO: Remove this function
		[SprakAPI("Log")]
		public void API_Log(string text)
		{
			logger.Log("LOG: " + text);
		}

		[SprakAPI("Print")]
		public void API_Print(string text)
		{
#if DEBUG
			//D.Log("Credit card " + name + " is printing: " + text);
#endif
			Say(text, "");
		}
		
		[SprakAPI("Get the name of the owner of the card")]
		public string API_GetNameOfCardOwner()
		{
			return CELL_nameOfOwner.data;
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return false; // pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}

		public override int securityLevel {
			get {
				return 1;
			}
		}
	}
}

