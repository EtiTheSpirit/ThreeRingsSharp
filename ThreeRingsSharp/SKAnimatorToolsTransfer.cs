using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData.XML.ConfigReferences;

namespace ThreeRingsSharp {

	/// <summary>
	/// Offers various bits and pieces that are used by the GUI portion of the program that are separate from this library.<para/>
	/// Its most important capability is to transfer data from the background worker thread to the main thread.
	/// </summary>
	public static class SKAnimatorToolsTransfer {

		/// <summary>
		/// A reference to the synchronization context for the GUI.
		/// </summary>
		public static SynchronizationContext UISyncContext { get; set; } = null;

		/// <summary>
		/// A delegate action that is called when the GUI needs to update. This can safely be <see langword="null"/> for contexts that do not have a GUI, as it is used for the SK Animator Tools UI.<para/>
		/// Pass in <see langword="null"/> for arguments to make their data remain unchanged.<para/>
		/// It is advised that you call this via <see cref="UpdateGUIThroughSync(string, string, string, string)"/> as this protects cross-thread actions.
		/// This is designed to work with the UI offered by SK Animator Tools V2. The parameters are as follows:<para/>
		/// <c>string fileName (the name of the file that was opened)<para/>
		/// string isCompressed (a string of true/false, yes/no, etc.)<para/>
		/// string formatVersion (represents clyde file version. Classic, Intermediate, or VarInt for example)<para/>
		/// string type (the base class, e.g. ModelConfig, AnimationConfig, ScriptedConfig)</c>
		/// </summary>
		public static Action<string, string, string, string> UpdateGUIAction { get; set; } = null;

		/// <summary>
		/// An <see cref="Action"/> to run when something goes wrong while configs are loading.<para/>
		/// Implemented by <see cref="ConfigReferenceBootstrapper"/>. It is advised that this is called through <see cref="ConfigsErroredThroughSync(Exception)"/>
		/// </summary>
		public static Action<Exception> ConfigsErroredAction { get; set; } = null;

		/// <summary>
		/// An action that fires whenever a single config reference file is loaded.
		/// </summary>
		public static Action<int?, int?, ProgressBarState?> ConfigsLoadingAction { get; set; } = null;

		/// <summary>
		/// An <see cref="Action"/> to run when configs are populated.<para/>
		/// Implemented by <see cref="ConfigReferenceBootstrapper"/>. It is advised that this is called through <see cref="ConfigsPopulatedThroughSync"/>
		/// </summary>
		public static Action ConfigsPopulatedAction { get; set; } = null;

		/// <summary>
		/// A reference to the <see cref="BackgroundWorker"/> that handles loading models.
		/// </summary>
		public static BackgroundWorker ModelLoaderWorker { get; set; } = null;

		/// <summary>
		/// A reference to the <see cref="ProgressBar"/>.
		/// </summary>
		public static ProgressBar Progress { get; set; } = null;

		/// <summary>
		/// Calls <see cref="UpdateGUIAction"/> through <see cref="UISyncContext"/>.<para/>
		/// This will do nothing if one or both of these is not defined.
		/// </summary>
		public static void UpdateGUIThroughSync(string fileName = null, string isCompressed = null, string formatVersion = null, string modelType = null) {
			UISyncContext?.Send(callback => {
				UpdateGUIAction?.Invoke(fileName, isCompressed, formatVersion, modelType);
			}, null);
		}

		/// <summary>
		/// Calls <see cref="ConfigsErroredAction"/> through <see cref="UISyncContext"/>.<para/>
		/// This will do nothing if one or both of these is not defined.
		/// </summary>
		/// <param name="error"></param>
		public static void ConfigsErroredThroughSync(Exception error) {
			UISyncContext?.Send(callback => {
				ConfigsErroredAction?.Invoke(error);
			}, null);
		}

		/// <summary>
		/// Calls <see cref="ConfigsPopulatedAction"/> through <see cref="UISyncContext"/>.<para/>
		/// This will do nothing if one or both of these is not defined.
		/// </summary>
		public static void ConfigsPopulatedThroughSync() {
			UISyncContext?.Send(callback => {
				ConfigsPopulatedAction?.Invoke();
			}, null);
		}

		/// <summary>
		/// Calls <see cref="ConfigsLoadingAction"/> through <see cref="UISyncContext"/>. Any parameters that are null implicitly mean "keep them the same" to the code.<para/>
		/// This will do nothing if one or both of these is not defined.
		/// </summary>
		/// <param name="numCfgsLoaded"></param>
		/// <param name="totalNumCfgs"></param>
		/// <param name="state"></param>
		public static void ConfigsLoadingThroughSync(int? numCfgsLoaded = null, int? totalNumCfgs = null, ProgressBarState? state = null) {
			UISyncContext?.Send(callback => {
				ConfigsLoadingAction?.Invoke(numCfgsLoaded, totalNumCfgs, state);
			}, null);
		}

		public static int CurrentProgress { get; private set; } = 0;

		public static int CurrentEnd { get; private set; } = 0;

		/// <summary>
		/// Reports progress to <see cref="ModelLoaderWorker"/> if it is not <see langword="null"/>.<para/>
		/// Unlike the typical <see cref="ReportProgress(int)"/> method of <see cref="BackgroundWorker"/>, this takes in an arbitrary number that directly sets the progress bar's value.
		/// </summary>
		/// <param name="numLoaded"></param>
		public static void ReportProgress(int numLoaded) {
			CurrentProgress = numLoaded;
			ModelLoaderWorker?.ReportProgress(numLoaded);
		}

		/// <summary>
		/// Increments the progress of <see cref="ModelLoaderWorker"/> forward by <paramref name="num"/> units.
		/// </summary>
		/// <param name="num"></param>
		public static void IncrementProgress(int num = 1) => ReportProgress(CurrentProgress + num);

		/// <summary>
		/// Increments the maximum value of the progress bar.
		/// </summary>
		/// <param name="num"></param>
		public static void IncrementEnd(int num = 1) {
			CurrentEnd += num;
			ModelLoaderWorker?.ReportProgress(CurrentProgress, CurrentEnd);
		}

		/// <summary>
		/// Alters the progress bar's appearance.
		/// </summary>
		/// <param name="state"></param>
		public static void SetProgressState(ProgressBarState state) {
			ModelLoaderWorker?.ReportProgress(-1, state);
		}

		/// <summary>
		/// Resets the progress bar.
		/// </summary>
		public static void ResetProgress() {
			CurrentProgress = 0;
			CurrentEnd = 0;
			if ((ModelLoaderWorker?.IsBusy).GetValueOrDefault(false)) {
				ModelLoaderWorker?.ReportProgress(CurrentProgress, CurrentEnd);
			} else {
				if (Progress != null) {
					Progress.Value = 0;
					Progress.Maximum = 0;
				}
			}
		}

		/// <summary>
		/// Used to register the virtual data tree to the main UI
		/// </summary>
		/// <param name="lastNodeParent"></param>
		/// <param name="rootDataTreeObject"></param>
		public static void RegisterNodes(dynamic lastNodeParent, DataTreeObject rootDataTreeObject) {
			UISyncContext?.Send(callback => {
				lastNodeParent.Nodes.Add(rootDataTreeObject.ConvertHierarchyToTreeNodes());
			}, null);
		}
	}

	public enum ProgressBarState {

		/// <summary>
		/// A green progress bar to represent typical behavior.
		/// </summary>
		OK = 1,

		/// <summary>
		/// A red progress bar to represent that something has failed.
		/// </summary>
		Error = 2,

		/// <summary>
		/// A yellow progress bar to represent that some additional work is being done (e.g. resolving references).
		/// </summary>
		ExtraWork = 3

	}
}
