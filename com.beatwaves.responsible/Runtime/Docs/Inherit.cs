using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Responsible.State;

namespace Responsible.Docs
{
	/// <summary>
	/// Class to help generate documentation with DocFX with inheritdoc tags.
	/// Not to be used in actual code!
	/// </summary>
	/// <remarks>
	/// Can't be excluded from DocFX or be internal without breaking functionality :/
	/// DocFX seems to require the argument count to match, thus this is a bit repetitive.
	/// </remarks>
	[ExcludeFromCodeCoverage]
	public abstract class Inherit
	{
		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public void CallerMember<T>(
			T arg1,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public void CallerMember<T1, T2>(
			T1 arg1,
			T2 arg2,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public void CallerMember<T1, T2, T3>(
			T1 arg1,
			T2 arg2,
			T3 arg3,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public void CallerMember<T1, T2, T3, T4>(
			T1 arg1,
			T2 arg2,
			T3 arg3,
			T4 arg4,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <inheritdoc cref="CallerMember{T1, T2}"/>
		/// <param name="description">Description of the operation, to be included in the state output.</param>
		public void CallerMemberWithDescription<T>(
			string description,
			T arg,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <inheritdoc cref="CallerMember{T1, T2, T3}"/>
		/// <param name="description">Description of the operation, to be included in the state output.</param>
		/// <param name="extraContext">Action for producing extra context into state descriptions.</param>
		public void CallerMemberWithDescriptionAndContext<T>(
			string description,
			T arg,
			Action<StateStringBuilder> extraContext = null,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <inheritdoc cref="CallerMember{T1, T2, T3, T4}"/>
		/// <param name="description">Description of the operation, to be included in the state output.</param>
		/// <param name="extraContext">Action for producing extra context into state descriptions.</param>
		public void CallerMemberWithDescriptionAndContext<T1, T2>(
			string description,
			T1 arg1,
			T2 arg2,
			Action<StateStringBuilder> extraContext = null,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <inheritdoc cref="CallerMember{T1, T2, T3}"/>
		/// <param name="executor">Test test instruction executor to use.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the instruction prematurely.</param>
		public void CallerMemberWithExecutor<T>(
			T arg1,
			TestInstructionExecutor executor,
			CancellationToken cancellationToken = default,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <inheritdoc cref="CallerMember{T1, T2, T3, T4}"/>
		/// <param name="executor">Test test instruction executor to use.</param>
		/// <param name="throwOnError">Whether or not to throw on cancellation or errors.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the instruction prematurely.</param>
		public void YieldInstruction<T>(
			T arg1,
			TestInstructionExecutor executor,
			bool throwOnError,
			CancellationToken cancellationToken = default,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <param name="description">Description of the step.</param>
		/// <param name="instruction">Instruction to execute in this step.</param>
		/// <returns>The given instruction as a BDD test step.</returns>
		public abstract Bdd.IBddStep BddKeyword(
			string description,
			ITestInstruction<object> instruction);
	}
}
